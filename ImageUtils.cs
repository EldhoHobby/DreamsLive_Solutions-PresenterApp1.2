using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DreamsLive_Solutions_PresenterApp1
{
    public static class ImageUtils
    {
        public static void CorrectRotation(Image img)
        {
            if (img == null) return;
            // EXIF Orientation Tag = 0x0112
            if (img.PropertyIdList.Contains(0x0112))
            {
                var prop = img.GetPropertyItem(0x0112);
                int rotationValue = 1; // Default to normal

                if (prop.Type == 3) // PropertyTagTypeShort
                    rotationValue = BitConverter.ToUInt16(prop.Value, 0);
                else if (prop.Type == 4) // PropertyTagTypeLong
                    rotationValue = (int)BitConverter.ToUInt32(prop.Value, 0);
                else if (prop.Value.Length > 0)
                    rotationValue = prop.Value[0];

                switch (rotationValue)
                {
                    case 1: break; // Normal
                    case 2: img.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                    case 3: img.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                    case 4: img.RotateFlip(RotateFlipType.Rotate180FlipX); break;
                    case 5: img.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                    case 6: img.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                    case 7: img.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                    case 8: img.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                }

                // Strip the tag so it isn't applied twice by other viewers or re-renders
                try { img.RemovePropertyItem(0x0112); } catch { }
            }
        }

        public static Image LoadImage(string path)
        {
            if (!File.Exists(path)) return null;

            try
            {
                // Use a FileStream to avoid locking the file
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (var imgTemp = Image.FromStream(fs))
                    {
                        CorrectRotation(imgTemp);

                        // "Bake" the rotation and orientation into a new bitmap
                        // This effectively strips metadata and ensures the image is physically upright
                        Bitmap baked = new Bitmap(imgTemp.Width, imgTemp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        baked.SetResolution(imgTemp.HorizontalResolution, imgTemp.VerticalResolution);

                        using (Graphics g = Graphics.FromImage(baked))
                        {
                            g.Clear(Color.Transparent);
                            // Perf: this bake is a 1:1 copy (CorrectRotation already applied the
                            // orientation in-place), so use NearestNeighbor — an exact pixel copy
                            // with no resampling. HighQualityBicubic here only burned CPU on the UI
                            // thread and slightly softened the image; a 1:1 copy is faster AND sharper.
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                            g.DrawImage(imgTemp, new Rectangle(0, 0, imgTemp.Width, imgTemp.Height));
                        }
                        return baked;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ImageUtils.LoadImage: {ex.Message}");
                return null;
            }
        }

        public static void ApplyRotation(Image img, int angle)
        {
            angle = ((angle % 360) + 360) % 360;
            if (angle == 90) img.RotateFlip(RotateFlipType.Rotate90FlipNone);
            else if (angle == 180) img.RotateFlip(RotateFlipType.Rotate180FlipNone);
            else if (angle == 270) img.RotateFlip(RotateFlipType.Rotate270FlipNone);
        }
    }
}
