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
            if (img.PropertyIdList.Contains(0x0112))
            {
                var prop = img.GetPropertyItem(0x0112);
                int rotationValue = prop.Type == 3 ? BitConverter.ToUInt16(prop.Value, 0) : prop.Value[0];
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
                img.RemovePropertyItem(0x0112);
            }
        }

        public static Image LoadImage(string path)
        {
            if (!File.Exists(path)) return null;

            using (var bmpTemp = new Bitmap(path))
            {
                CorrectRotation(bmpTemp);
                return new Bitmap(bmpTemp);
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
