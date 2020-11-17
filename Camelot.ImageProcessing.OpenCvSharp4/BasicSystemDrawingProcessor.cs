using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Graphics.Colors;
using static UglyToad.PdfPig.Core.PdfSubpath;

namespace Camelot.ImageProcessing.OpenCvSharp4
{
    /// <summary>
    /// Only draws pdf paths and images - letters are ignored.
    /// </summary>
    public class BasicSystemDrawingProcessor : IDrawingProcessor
    {
        private static Matrix GetInitialMatrix(int rotation, CropBox mediaBox)
        {
            float cos, sin;
            float dx = 0, dy = 0;
            switch (rotation)
            {
                case 0:
                    cos = 1;
                    sin = 0;
                    break;
                case 90:
                    cos = 0;
                    sin = 1;
                    dy = (float)mediaBox.Bounds.Height;
                    break;
                case 180:
                    cos = -1;
                    sin = 0;
                    dx = (float)mediaBox.Bounds.Width;
                    dy = (float)mediaBox.Bounds.Height;
                    break;
                case 270:
                    cos = 0;
                    sin = -1;
                    dx = (float)mediaBox.Bounds.Width;
                    break;
                default:
                    throw new InvalidOperationException($"Invalid value for page rotation: {rotation}.");
            }

            return new Matrix(cos, -sin,
                              sin, cos,
                              dx, dy);
        }

        public MemoryStream DrawPage(Page page, double pageScale)
        {
            var ms = new MemoryStream();
            using (var bitmap = new Bitmap((int)Math.Ceiling(page.Width * pageScale), (int)Math.Ceiling(page.Height * pageScale), PixelFormat.Format32bppRgb))
            using (var currentGraphics = Graphics.FromImage(bitmap))
            {
                //bitmap.SetResolution(300, 300);
                currentGraphics.SmoothingMode = SmoothingMode.HighQuality;
                currentGraphics.CompositingQuality = CompositingQuality.HighQuality;
                currentGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                currentGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                currentGraphics.Clear(Color.White);

                // flip transform
                currentGraphics.Transform = GetInitialMatrix(page.Rotation.Value, page.CropBox);
                currentGraphics.TranslateTransform(0, -(float)page.Height, MatrixOrder.Append);
                currentGraphics.ScaleTransform((float)pageScale, -(float)pageScale, MatrixOrder.Append);

                foreach (var image in page.GetImages())
                {
                    DrawImage(image, currentGraphics);
                }

                foreach (var path in page.ExperimentalAccess.Paths)
                {
                    GraphicsPath gp = new GraphicsPath();
                    foreach (var subpath in path)
                    {
                        foreach (var command in subpath.Commands)
                        {
                            if (command is Move)
                            {
                                gp.StartFigure();
                            }
                            else if (command is Line line)
                            {
                                gp.AddLine((float)line.From.X, (float)line.From.Y, (float)line.To.X, (float)line.To.Y);
                            }
                            else if (command is BezierCurve curve)
                            {
                                gp.AddBezier((float)curve.StartPoint.X, (float)curve.StartPoint.Y,
                                    (float)curve.FirstControlPoint.X, (float)curve.FirstControlPoint.Y,
                                    (float)curve.SecondControlPoint.X, (float)curve.SecondControlPoint.Y,
                                    (float)curve.EndPoint.X, (float)curve.EndPoint.Y);
                            }
                            else if (command is Close)
                            {
                                gp.CloseFigure();
                            }
                            else
                            {
                                throw new ArgumentException();
                            }
                        }
                    }

                    if (path.IsFilled)
                    {
                        gp.FillMode = ToSystemFillMode(path.FillingRule);
                        using (var brush = new SolidBrush(ToSystemColor(path.FillColor)))
                        {
                            currentGraphics.FillPath(brush, gp);
                        }
                    }

                    if (path.IsStroked)
                    {
                        try
                        {
                            using (var pen = new Pen(ToSystemColor(path.StrokeColor)))
                            {
                                currentGraphics.DrawPath(pen, gp);
                            }
                        }
                        catch (OutOfMemoryException)
                        {
                            //you will get an OutOfMemoryException if you try to use a LinearGradientBrush to fill a rectangle whose width or height is zero
                            var bounds = gp.GetBounds();
                            if (bounds.Width >= 1 && bounds.Height >= 1)
                            {
                                throw;
                            }
                        }
                    }
                }

                bitmap.Save(ms, ImageFormat.Png);
            }
            return ms;
        }

        private void DrawImage(IPdfImage image, Graphics graphics)
        {
            if (image.TryGetPng(out var png))
            {
                using (var img = Image.FromStream(new MemoryStream(png)))
                {
                    img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    graphics.DrawImage(img, new RectangleF(0, 0, 1, 1));
                }
            }
            else
            {
                if (image.TryGetBytes(out var bytes))
                {
                    try
                    {
                        using (var img = Image.FromStream(new MemoryStream(bytes.ToArray())))
                        {
                            img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            graphics.DrawImage(img, new RectangleF(0, 0, 1, 1));
                        }
                        return;
                    }
                    catch (Exception)
                    {

                    }
                }

                try
                {
                    using (var img = Image.FromStream(new MemoryStream(image.RawBytes.ToArray())))
                    {
                        img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        graphics.DrawImage(img, new RectangleF(0, 0, 1, 1));
                    }
                }
                catch (Exception)
                {
                    graphics.FillRectangle(Brushes.HotPink, new RectangleF(0, 0, 1, 1));
                }
            }
        }

        /// <summary>
        /// Default to Black.
        /// </summary>
        /// <param name="pdfColor"></param>
        /// <returns></returns>
        public static Color ToSystemColor(IColor pdfColor)
        {
            if (pdfColor != null)
            {
                var colorRgb = pdfColor.ToRGBValues();
                return Color.FromArgb((int)(colorRgb.r * 255), (int)(colorRgb.g * 255), (int)(colorRgb.b * 255));
            }
            return Color.Black;
        }

        public static FillMode ToSystemFillMode(FillingRule fillingRule)
        {
            switch (fillingRule)
            {
                case FillingRule.NonZeroWinding:
                    return FillMode.Winding;

                case FillingRule.EvenOdd:
                default:
                    return FillMode.Alternate;
            }
        }
    }
}
