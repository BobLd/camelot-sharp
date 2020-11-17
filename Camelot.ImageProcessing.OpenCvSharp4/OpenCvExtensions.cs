using OpenCvSharp;
using System;

namespace Camelot.ImageProcessing.OpenCvSharp4
{
    internal static class OpenCvExtensions
    {
        /// <summary>
        /// Height, Width, Channels
        /// </summary>
        public static int[] shape(this Mat mat)
        {
            if (mat.Channels() > 1)
            {
                return new int[] { mat.Height, mat.Width, mat.Channels() };
            }
            else
            {
                return new int[] { mat.Height, mat.Width };
            }
        }

        public static int FloorDiv(this int a, int b)
        {
            return (int)Math.Floor(a / (double)b);
        }

        public static (int x, int y, int w, int h) ToTuple(this Rect rect) => (rect.X, rect.Y, rect.Width, rect.Height);
    }
}
