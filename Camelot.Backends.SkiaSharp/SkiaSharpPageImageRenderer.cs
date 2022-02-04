using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Graphics.Colors;
using UglyToad.PdfPig.Rendering;
using static UglyToad.PdfPig.Core.PdfSubpath;

namespace Camelot.Backends.SkiaSharp
{
    public class SkiaSharpPageImageRenderer : IPageImageRenderer
    {
        /// <inheritdoc/>
        public byte[] Render(Page page, double pageScale, PdfRendererImageFormat imageFormat = PdfRendererImageFormat.Png)
        {
            var info = new SKImageInfo((int)Math.Ceiling(page.Width * pageScale), (int)Math.Ceiling(page.Height * pageScale));

            using (var surface = SKSurface.Create(info))
            {
                var currentGraphics = surface.Canvas;
                //bitmap.SetResolution(300, 300);
                //currentGraphics.SmoothingMode = SmoothingMode.HighQuality;
                //currentGraphics.CompositingQuality = CompositingQuality.HighQuality;
                //currentGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //currentGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                currentGraphics.Clear(SKColors.White);

                // flip transform
                var initMatrix = GetInitialMatrix(page.Rotation.Value, page.CropBox);

                //currentGraphics.SetMatrix(initMatrix); //.Transform = GetInitialMatrix(page.Rotation.Value, page.CropBox);
                //currentGraphics.Translate(0, -(float)page.Height); //.TranslateTransform(0, -(float)page.Height, MatrixOrder.Append);
                //currentGraphics.Scale((float)pageScale, -(float)pageScale); //.ScaleTransform((float)pageScale, -(float)pageScale, MatrixOrder.Append);

                initMatrix = initMatrix.PostConcat(SKMatrix.CreateTranslation(0, -(float)page.Height));
                initMatrix = initMatrix.PostConcat(SKMatrix.CreateScale((float)pageScale, -(float)pageScale));
                currentGraphics.SetMatrix(initMatrix);

                foreach (var image in page.GetImages())
                {
                    DrawImage(image, currentGraphics);
                }

                foreach (var path in page.ExperimentalAccess.Paths)
                {
                    var gp = new SKPath();
                    foreach (var subpath in path)
                    {
                        foreach (var command in subpath.Commands)
                        {
                            if (command is Move m)
                            {
                                gp.MoveTo((float)m.Location.X, (float)m.Location.Y);
                            }
                            else if (command is Line line)
                            {
                                gp.LineTo((float)line.To.X, (float)line.To.Y);
                            }
                            else if (command is BezierCurve curve)
                            {
                                gp.CubicTo((float)curve.FirstControlPoint.X, (float)curve.FirstControlPoint.Y,
                                    (float)curve.SecondControlPoint.X, (float)curve.SecondControlPoint.Y,
                                    (float)curve.EndPoint.X, (float)curve.EndPoint.Y);
                            }
                            else if (command is Close)
                            {
                                gp.Close();
                            }
                            else
                            {
                                throw new ArgumentException($"Unknown command type '{command.GetType()}'.");
                            }
                        }
                    }

                    // Can we do stroke fill?
                    if (path.IsFilled)
                    {
                        gp.FillType = ToSkiaFillMode(path.FillingRule);
                        using (var brush = new SKPaint()
                        {
                            Style = SKPaintStyle.Fill,
                            Color = ToSkiaColor(path.FillColor),
                            FilterQuality = SKFilterQuality.High,
                            IsAntialias = false,
                            IsDither = false
                        })
                        {
                            currentGraphics.DrawPath(gp, brush);
                        }
                    }

                    if (path.IsStroked)
                    {
                        using (var pen = new SKPaint()
                        {
                            StrokeWidth = (float)path.LineWidth,
                            Style = SKPaintStyle.Stroke,
                            Color = ToSkiaColor(path.StrokeColor),
                            FilterQuality = SKFilterQuality.High,
                            IsAntialias = false,
                            IsDither = false
                        })
                        {
                            currentGraphics.DrawPath(gp, pen);
                        }
                    }
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(ToSkiaImageFormat(imageFormat), 100))
                using (var stream = new MemoryStream())
                {
                    data.SaveTo(stream);
                    return stream.ToArray();
                }
            }
        }

        private static SKMatrix GetInitialMatrix(int rotation, CropBox mediaBox)
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

            return new SKMatrix(cos, -sin, 0,
                              sin, cos, 0,
                              dx, dy, 1);
        }

        private void DrawImage(IPdfImage image, SKCanvas graphics)
        {
            if (image.TryGetPng(out var png))
            {
                using (var img = SKImage.FromEncodedData(new MemoryStream(png)))
                {
                    //img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    graphics.DrawImage(img, new SKRect(0, 0, 1, 1));
                }
            }
            else
            {
                if (image.TryGetBytes(out var bytes))
                {
                    try
                    {
                        using (var img = SKImage.FromEncodedData(new MemoryStream(bytes.ToArray())))
                        {
                            //img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            graphics.DrawImage(img, new SKRect(0, 0, 1, 1));
                        }
                        return;
                    }
                    catch (Exception)
                    { }
                }

                try
                {
                    using (var img = SKImage.FromEncodedData(new MemoryStream(image.RawBytes.ToArray())))
                    {
                        //img.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        graphics.DrawImage(img, new SKRect(0, 0, 1, 1));
                    }
                }
                catch (Exception)
                {
                    graphics.DrawRect(0, 0, 1, 1, new SKPaint()
                    {
                        Style = SKPaintStyle.Fill, Color = SKColors.HotPink
                    }); //.FillRectangle(Brushes.HotPink, new RectangleF(0, 0, 1, 1));
                }
            }
        }

        private static SKPathFillType ToSkiaFillMode(FillingRule fillingRule)
        {
            switch (fillingRule)
            {
                case FillingRule.NonZeroWinding:
                    return SKPathFillType.Winding;

                case FillingRule.None:
                case FillingRule.EvenOdd:
                default:
                    return SKPathFillType.EvenOdd;
            }
        }

        /// <summary>
        /// Default to Black.
        /// </summary>
        /// <param name="pdfColor"></param>
        /// <returns></returns>
        public static SKColor ToSkiaColor(IColor pdfColor)
        {
            if (pdfColor != null)
            {
                var (r, g, b) = pdfColor.ToRGBValues();
                return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
            }
            return SKColors.Black;
        }

        private static SKEncodedImageFormat ToSkiaImageFormat(PdfRendererImageFormat pdfRendererImageFormat)
        {
            switch (pdfRendererImageFormat)
            {
                case PdfRendererImageFormat.Bmp:
                    return SKEncodedImageFormat.Bmp;

                case PdfRendererImageFormat.Gif:
                    return SKEncodedImageFormat.Gif;

                case PdfRendererImageFormat.Jpeg:
                    return SKEncodedImageFormat.Jpeg;

                case PdfRendererImageFormat.Png:
                default:
                    return SKEncodedImageFormat.Png;

                case PdfRendererImageFormat.Tiff:
                    throw new ArgumentException("Tiff is not supported", nameof(pdfRendererImageFormat));
            }
        }
    }
}
