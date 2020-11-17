using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace Camelot
{
    internal static class PdfPigExtensions
    {
        /// <summary>
        /// BottomLeft.X = Left
        /// </summary>
        public static float X0(this PdfRectangle rectangle) => (float)rectangle.BottomLeft.X;

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float Y0(this PdfRectangle rectangle) => (float)rectangle.BottomLeft.Y;

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float X1(this PdfRectangle rectangle) => (float)rectangle.TopRight.X;

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float Y1(this PdfRectangle rectangle) => (float)rectangle.TopRight.Y;

        /// <summary>
        /// BottomLeft.X = Left
        /// </summary>
        public static float X0(this TextLine textLine) => (float)textLine.BoundingBox.X0();

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float Y0(this TextLine textLine) => (float)textLine.BoundingBox.Y0();

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float X1(this TextLine textLine) => (float)textLine.BoundingBox.X1();

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float Y1(this TextLine textLine) => (float)textLine.BoundingBox.Y1();

        /// <summary>
        /// (x0, y0, x1, y1)
        /// </summary>
        public static float[] Bbox(this TextLine textLine) => new float[] { textLine.X0(), textLine.Y0(), textLine.X1(), textLine.Y1() };

        /// <summary>
        /// https://github.com/euske/pdfminer/blob/423f851fc20ebd701bc4c8b5b7ba0e7904e18e3b/pdfminer/layout.py#L112
        /// </summary>
        /// <param name="textLine"></param>
        public static bool IsEmpty(this TextLine textLine) => textLine.BoundingBox.Width <= 0 || textLine.BoundingBox.Height <= 0;

        public static IEnumerable<Letter> Objs(this TextLine textLine)
        {
            foreach (var w in textLine.Words)
            {
                foreach (var l in w.Letters)
                {
                    yield return l;
                }
                yield return new LTAnno(" "); // add space
            }
            yield return new LTAnno("\n"); // add new line
        }

        /// <summary>
        /// BottomLeft.X = Left
        /// </summary>
        public static float X0(this Letter letter) => (float)letter.GlyphRectangle.X0();

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float Y0(this Letter letter) => (float)letter.GlyphRectangle.Y0();

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float X1(this Letter letter) => (float)letter.GlyphRectangle.X1();

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float Y1(this Letter letter) => (float)letter.GlyphRectangle.Y1();
    }
}
