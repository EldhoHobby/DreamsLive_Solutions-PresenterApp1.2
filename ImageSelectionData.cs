using System;
using System.Drawing;

namespace DreamsLive_Solutions_PresenterApp1
{
    public class ImageSelectionData
    {
        public string ImagePath { get; set; }
        public float SelectionX { get; set; }
        public float SelectionY { get; set; }
        public float SelectionWidth { get; set; }
        public float SelectionHeight { get; set; }

        // Parameterless constructor for deserialization
        public ImageSelectionData() { }

        public ImageSelectionData(string imagePath, RectangleF rect)
        {
            ImagePath = imagePath;
            SelectionX = rect.X;
            SelectionY = rect.Y;
            SelectionWidth = rect.Width;
            SelectionHeight = rect.Height;
        }

        public RectangleF ToRectangleF()
        {
            return new RectangleF(SelectionX, SelectionY, SelectionWidth, SelectionHeight);
        }
    }
}
