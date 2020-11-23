using System;
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
        public static float X0(this TextLine textLine) => textLine.BoundingBox.X0();

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float Y0(this TextLine textLine) => textLine.BoundingBox.Y0();

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float X1(this TextLine textLine) => textLine.BoundingBox.X1();

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float Y1(this TextLine textLine) => textLine.BoundingBox.Y1();

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
        public static float X0(this Letter letter) => letter.GlyphRectangle.X0();

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float Y0(this Letter letter) => letter.GlyphRectangle.Y0();

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float X1(this Letter letter) => letter.GlyphRectangle.X1();

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float Y1(this Letter letter) => letter.GlyphRectangle.Y1();

        /// <summary>
        /// Returns True if two characters can coexist in the same line.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="obj"></param>
        /// <returns>true</returns>
        public static bool is_compatible(this Letter l, Letter obj)
        {
            return true;
        }

        //    def is_hoverlap(self, obj):
        //    assert isinstance(obj, LTComponent), str(type(obj))
        //    return obj.x0 <= self.x1 and self.x0 <= obj.x1

        //def hdistance(self, obj) :
        //    assert isinstance(obj, LTComponent), str(type(obj))
        //    if self.is_hoverlap(obj):
        //        return 0
        //    else:
        //        return min(abs(self.x0-obj.x1), abs(self.x1-obj.x0))

        //def hoverlap(self, obj):
        //    assert isinstance(obj, LTComponent), str(type(obj))
        //    if self.is_hoverlap(obj):
        //        return min(abs(self.x0-obj.x1), abs(self.x1-obj.x0))
        //    else:
        //        return 0

        public static bool is_hoverlap(this Letter l, Letter obj)
        {
            return obj.X0() <= l.X1() && l.X0() <= obj.X1();
        }

        public static float hdistance(this Letter l, Letter obj)
        {
            if (l.is_hoverlap(obj))
            {
                return 0;
            }
            else
            {
                return Math.Min(Math.Abs(l.X0() - obj.X1()), Math.Abs(l.X1() - obj.X0()));
            }
        }

        public static float hoverlap(this Letter l, Letter obj)
        {
            if (l.is_hoverlap(obj))
            {
                return Math.Min(Math.Abs(l.X0() - obj.X1()), Math.Abs(l.X1() - obj.X0()));
            }
            else
            {
                return 0;
            }
        }

        public static bool is_voverlap(this Letter l, Letter obj)
        {
            return obj.Y0() <= l.Y1() && l.Y0() <= obj.Y1();
        }

        public static float vdistance(this Letter l, Letter obj)
        {
            if (l.is_voverlap(obj))
            {
                return 0;
            }
            else
            {
                return Math.Min(Math.Abs(l.Y0() - obj.Y1()), Math.Abs(l.Y1() - obj.Y0()));
            }
        }

        public static float voverlap(this Letter l, Letter obj)
        {
            if (l.is_voverlap(obj))
            {
                return Math.Min(Math.Abs(l.Y0() - obj.Y1()), Math.Abs(l.Y1() - obj.Y0()));
            }
            else
            {
                return 0;
            }
        }

        public static bool isHorizontal(this TextLine line)
        {
            return line.TextOrientation == TextOrientation.Horizontal || line.TextOrientation == TextOrientation.Rotate180;
        }

        public static bool isVertical(this TextLine line)
        {
            return line.TextOrientation == TextOrientation.Rotate90 || line.TextOrientation == TextOrientation.Rotate270;
        }

        /// <summary>
        /// BottomLeft.X = Left
        /// </summary>
        public static float X0(this TextBlock box) => box.BoundingBox.X0();

        /// <summary>
        /// BottomLeft.Y = Bottom
        /// </summary>
        public static float Y0(this TextBlock box) => box.BoundingBox.Y0();

        /// <summary>
        /// TopRight.X = Right
        /// </summary>
        public static float X1(this TextBlock box) => box.BoundingBox.X1();

        /// <summary>
        /// TopRight.Y = Top
        /// </summary>
        public static float Y1(this TextBlock box) => box.BoundingBox.Y1();
    }
}
