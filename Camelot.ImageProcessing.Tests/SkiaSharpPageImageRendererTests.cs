using Camelot.Backends.SkiaSharp;
using System.Drawing;
using System.IO;
using UglyToad.PdfPig;
using Xunit;

namespace Camelot.ImageProcessing.Tests
{
    public class SkiaSharpPageImageRendererTests
    {
        public SkiaSharpPageImageRendererTests()
        {
            Directory.CreateDirectory("Files/Output/");
        }

        [Fact]
        public void DrawFooScale1()
        {
            var draw = new SkiaSharpPageImageRenderer();
            using (var document = PdfDocument.Open("Files/foo.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment
                using (var stream = new MemoryStream(draw.Render(page, 1)))
                using (var img = Image.FromStream(stream))
                {
                    img.Save("Files/Output/foo_basic_render_1.png");
                }
            }
        }

        [Fact]
        public void DrawFooScale3()
        {
            var draw = new SkiaSharpPageImageRenderer();
            using (var document = PdfDocument.Open("Files/foo.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment

                using (var stream = new MemoryStream(draw.Render(page, 3)))
                using (var img = Image.FromStream(stream))
                {
                    img.Save("Files/Output/foo_basic_render_3.png");
                }
            }
        }

        [Fact]
        public void DrawColumnSpanScale1()
        {
            var draw = new SkiaSharpPageImageRenderer();
            using (var document = PdfDocument.Open("Files/column_span_2.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment
                using (var stream = new MemoryStream(draw.Render(page, 1)))
                using (var img = Image.FromStream(stream))
                {
                    img.Save("Files/Output/column_span_2_basic_render_1.png");
                }
            }
        }

        [Fact]
        public void DrawColumnSpanScale3()
        {
            var draw = new SkiaSharpPageImageRenderer();
            using (var document = PdfDocument.Open("Files/column_span_2.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment

                using (var stream = new MemoryStream(draw.Render(page, 3)))
                using (var img = Image.FromStream(stream))
                {
                    img.Save("Files/Output/column_span_2_basic_render_3.png");
                }
            }
        }

        [Fact]
        public void DrawImageScale1()
        {
            var draw = new SkiaSharpPageImageRenderer();
            using (var document = PdfDocument.Open("Files/image.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment
                using (var stream = new MemoryStream(draw.Render(page, 1)))
                using (var img = Image.FromStream(stream))
                {
                    img.Save("Files/Output/image_render_1.png");
                }
            }
        }

        [Fact]
        public void DrawImageScale3()
        {
            var draw = new SkiaSharpPageImageRenderer();
            using (var document = PdfDocument.Open("Files/image.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment

                using (var stream = new MemoryStream(draw.Render(page, 3)))
                using (var img = Image.FromStream(stream))
                {
                    img.Save("Files/Output/image_basic_render_3.png");
                }
            }
        }
    }
}
