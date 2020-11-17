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
        public static float x0(this PdfRectangle rectangle) => (float)rectangle.BottomLeft.X;

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float y0(this PdfRectangle rectangle) => (float)rectangle.BottomLeft.Y;

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float x1(this PdfRectangle rectangle) => (float)rectangle.TopRight.X;

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float y1(this PdfRectangle rectangle) => (float)rectangle.TopRight.Y;

        /// <summary>
        /// BottomLeft.X = Left
        /// </summary>
        public static float x0(this TextLine textLine) => (float)textLine.BoundingBox.x0();

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float y0(this TextLine textLine) => (float)textLine.BoundingBox.y0();

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float x1(this TextLine textLine) => (float)textLine.BoundingBox.x1();

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float y1(this TextLine textLine) => (float)textLine.BoundingBox.y1();

        /// <summary>
        /// (x0, y0, x1, y1)
        /// </summary>
        public static float[] bbox(this TextLine textLine) => new float[] { textLine.x0(), textLine.y0(), textLine.x1(), textLine.y1() };

        /// <summary>
        /// https://github.com/euske/pdfminer/blob/423f851fc20ebd701bc4c8b5b7ba0e7904e18e3b/pdfminer/layout.py#L112
        /// </summary>
        /// <param name="textLine"></param>
        public static bool is_empty(this TextLine textLine) => textLine.BoundingBox.Width <= 0 || textLine.BoundingBox.Height <= 0;

        public static IEnumerable<Letter> _objs(this TextLine textLine)
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
        public static float x0(this Letter letter) => (float)letter.GlyphRectangle.x0();

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float y0(this Letter letter) => (float)letter.GlyphRectangle.y0();

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float x1(this Letter letter) => (float)letter.GlyphRectangle.x1();

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float y1(this Letter letter) => (float)letter.GlyphRectangle.y1();
    }
}
