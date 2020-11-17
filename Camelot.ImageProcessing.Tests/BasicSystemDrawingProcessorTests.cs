﻿using Camelot.ImageProcessing.OpenCvSharp4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using UglyToad.PdfPig;
using Xunit;

namespace Camelot.ImageProcessing.Tests
{
    public class BasicSystemDrawingProcessorTests
    {
        public BasicSystemDrawingProcessorTests()
        {
            Directory.CreateDirectory(@"Files\Output\");
        }

        [Fact]
        public void DrawScale1()
        {
            BasicSystemDrawingProcessor draw = new BasicSystemDrawingProcessor();
            using (var document = PdfDocument.Open(@"Files\foo.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment

                var stream = draw.DrawPage(page, 1);
                using (var img = Bitmap.FromStream(stream))
                {
                    img.Save(@"Files\Output\foo_basic_render_1.png");
                }
            }
        }

        [Fact]
        public void DrawScale3()
        {
            BasicSystemDrawingProcessor draw = new BasicSystemDrawingProcessor();
            using (var document = PdfDocument.Open(@"Files\foo.pdf", new ParsingOptions() { ClipPaths = true }))
            {
                var page = document.GetPage(1); // always page 1 for the moment

                var stream = draw.DrawPage(page, 3);
                using (var img = Bitmap.FromStream(stream))
                {
                    img.Save(@"Files\Output\foo_basic_render_3.png");
                }
            }
        }
    }
}