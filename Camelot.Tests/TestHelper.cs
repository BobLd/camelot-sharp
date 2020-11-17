using System;
using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Graphics.Colors;
using UglyToad.PdfPig.PdfFonts;

namespace Camelot.Tests
{
    public static class TestHelper
    {
        public static TextLine MakeTextLine(double x0, double y0, double x1, double y1, string text)
        {
            return MakeTextLine((float)x0, (float)y0, (float)x1, (float)y1, text);
        }

        public static TextLine MakeTextLine(float x0, float y0, float x1, float y1, string text)
        {
            return MakeTextLine(new float[] { x0, y0, x1, y1 }, text);
        }

        public static TextLine MakeTextLine(float[] bbox, string text)
        {
            float x0 = bbox[0];
            float y0 = bbox[1];
            float x1 = bbox[2];
            float y1 = bbox[3];

            return new TextLine(new List<Word>()
                {
                    new Word(new List<Letter>()
                    {
                        new Letter(text,
                                   new PdfRectangle(x0, y0, x1, y1),
                                   new PdfPoint(x0, y0),
                                   new PdfPoint(x1, y0),
                                   1, 1, new FontDetails(string.Empty, false, 1, false),
                                   RGBColor.Black, 1, -1)
                    })
                });
        }

        public class ListTuple2EqualityComparer : IEqualityComparer<List<(float, float)>>
        {
            private readonly int precision;

            public ListTuple2EqualityComparer(int precision)
            {
                this.precision = precision;
            }

            public bool Equals(List<(float, float)> x, List<(float, float)> y)
            {
                if (x.Count != y.Count) return false;
                for (int i = 0; i < x.Count; i++)
                {
                    var _x = x[i];
                    var _y = y[i];
                    if (!Math.Round(_x.Item1, precision).Equals(Math.Round(_y.Item1, precision)))
                    {
                        return false;
                    }

                    if (!Math.Round(_x.Item2, precision).Equals(Math.Round(_y.Item2, precision)))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(List<(float, float)> obj)
            {
                return obj.ConvertAll(t => (Math.Round(t.Item1, precision), Math.Round(t.Item2, precision))).GetHashCode();
            }
        }
    }
}
