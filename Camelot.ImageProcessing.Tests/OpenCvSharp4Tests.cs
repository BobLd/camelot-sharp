using Camelot.ImageProcessing.OpenCvSharp4;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using Xunit;

namespace Camelot.ImageProcessing.Tests
{
    public class OpenCvSharp4Tests
    {
        /// <summary>
        /// Height, Width, Channels
        /// </summary>
        public static int[] Shape(Mat mat)
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

        public OpenCvSharp4Tests()
        {
            Directory.CreateDirectory(@"Files\Output\");
        }

        [Fact]
        public void AdaptiveThreshold()
        {
            const string imagePath = @"Files\PMC5055614_00002.jpg";

            OpenCvImageProcesser _imageProcessing = new OpenCvImageProcesser();

            (var img, var threshold) = _imageProcessing.AdaptiveThreshold(imagePath, false);
            Assert.Equal(new int[] { 794, 596, 3 }, Shape(img));
            Assert.Equal(new int[] { 794, 596 }, Shape(threshold));
            using (var th = threshold.ToBitmap())
            {
                th.Save(@"Files\Output\PMC5055614_00002_threshold.png");
            }

            (var img_bg, var threshold_bg) = _imageProcessing.AdaptiveThreshold(imagePath, true);
            Assert.Equal(new int[] { 794, 596, 3 }, Shape(img_bg));
            Assert.Equal(new int[] { 794, 596 }, Shape(threshold_bg));
            using (var th = threshold_bg.ToBitmap())
            {
                th.Save(@"Files\Output\PMC5055614_00002_threshold_bg.png");
            }

            img.Dispose();
            threshold.Dispose();
            img_bg.Dispose();
            threshold_bg.Dispose();
        }

        [Fact]
        public void FindLines()
        {
            const string imagePath = @"Files\PMC5055614_00002.jpg";
            OpenCvImageProcesser _imageProcessing = new OpenCvImageProcesser();

            // horizontal
            (var img, var threshold) = _imageProcessing.AdaptiveThreshold(imagePath, false);

            // find_lines: no region, horizontal
            (var dmask, var lines) = _imageProcessing.FindLines(threshold);
            var expected_lines = new List<(int, int, int, int)>()
            {
                (414, 377, 476, 377),
                (265, 377, 326, 377),
                (340, 376, 402, 376),
                (191, 376, 252, 376),
                (120, 368, 192, 368),
                (120, 335, 192, 335),
                (414, 329, 476, 329),
                (265, 329, 326, 329),
                (340, 328, 402, 328),
                (191, 328, 252, 328),
                (389, 310, 451, 310),
                (307, 310, 369, 310),
                (223, 310, 285, 310),
                (121, 306, 224, 306),
                (121, 273, 224, 273),
                (389, 262, 451, 262),
                (307, 262, 369, 262),
                (223, 262, 285, 262),
                (268, 259, 417, 259),
                (267, 257, 417, 257),
                (268, 250, 351, 250),
                (120, 242, 450, 242),
                (120, 210, 225, 210),
                (389, 194, 450, 194),
                (309, 194, 371, 194),
                (224, 194, 285, 194),
                (120, 180, 447, 180),
                (245, 161, 312, 161),
                (371, 147, 447, 147),
                (120, 147, 312, 147),
                (311, 132, 372, 132),
                (121, 119, 447, 119),
                (374, 85, 447, 85),
                (121, 85, 314, 85),
                (313, 71, 375, 71)
            };

            Assert.Equal(expected_lines.Count, lines.Count);
            Assert.Equal(expected_lines, lines);

            // find_lines: region, horizontal
            (var dmask1, var lines1) = _imageProcessing.FindLines(threshold, new List<(int, int, int, int)>() { (0, 0, 200, 200) });
            var expected_lines1 = new List<(int, int, int, int)>()
            {
                (120, 180, 200, 180),
                (120, 147, 200, 147),
                (121, 118, 200, 118),
                (121, 85, 200, 85)
            };

            Assert.Equal(expected_lines1.Count, lines1.Count);
            Assert.Equal(expected_lines1, lines1);

            img.Dispose();
            threshold.Dispose();
            dmask.Dispose();
            dmask1.Dispose();

            // vertical
            (var img_bg, var threshold_bg) = _imageProcessing.AdaptiveThreshold(imagePath, true);

            // find_lines: no region, vertical
            (var dmask_v, var lines_v) = _imageProcessing.FindLines(threshold_bg, direction: "vertical");
            var expected_lines2 = new List<(int, int, int, int)>()
            {
                (55, 734, 55, 659),
                (195, 665, 195, 608),
                (505, 661, 505, 606),
                (393, 651, 393, 583),
                (351, 627, 351, 573),
                (308, 672, 308, 551),
                (541, 659, 541, 540),
                (55, 648, 55, 538),
                (286, 601, 286, 527),
                (185, 554, 185, 498),
                (540, 525, 540, 441),
                (287, 514, 287, 431),
                (243, 481, 243, 425),
                (308, 538, 308, 417),
                (54, 527, 54, 416),
                (478, 383, 478, 325),
                (453, 317, 453, 258),
                (452, 249, 452, 190)
            };
            Assert.Equal(expected_lines2.Count, lines_v.Count);
            Assert.Equal(expected_lines2, lines_v);

            // find_lines: region, vertical
            (var dmask_v1, var lines_v1) = _imageProcessing.FindLines(threshold_bg, regions: new List<(int, int, int, int)>() { (0, 400, 190, 100) }, direction: "vertical");
            var expected_lines3 = new List<(int, int, int, int)>()
            {
                (54, 501, 54, 416)
            };
            Assert.Equal(expected_lines3.Count, lines_v1.Count);
            Assert.Equal(expected_lines3, lines_v1);

            (var dmask_v2, var lines_v2) = _imageProcessing.FindLines(threshold_bg, regions: new List<(int, int, int, int)>() { (0, 680, 190, 100) }, direction: "vertical");
            var expected_lines4 = new List<(int, int, int, int)>()
            {
                (55, 734, 55, 681)
            };
            Assert.Equal(expected_lines4.Count, lines_v2.Count);
            Assert.Equal(expected_lines4, lines_v2);

            img_bg.Dispose();
            threshold_bg.Dispose();
            dmask_v.Dispose();
            dmask_v1.Dispose();
            dmask_v2.Dispose();
        }

        [Fact]
        public void FindContours()
        {
            const string imagePath = @"Files\PMC5055614_00002.jpg";
            OpenCvImageProcesser _imageProcessing = new OpenCvImageProcesser();

            (var img, var threshold) = _imageProcessing.AdaptiveThreshold(imagePath, false);
            (var vertical_mask, var _) = _imageProcessing.FindLines(threshold, direction: "vertical");
            (var horizontal_mask, var _) = _imageProcessing.FindLines(threshold); //, direction: "horizontal");

            var contours = _imageProcessing.FindContours(vertical_mask, horizontal_mask);
            var expected_contours = new List<(int, int, int, int)>()
            {
                (120, 242, 330, 2),
                (120, 179, 327, 2),
                (121, 305, 103, 2),
                (268, 249, 83, 2),
                (414, 376, 62, 2),
                (307, 309, 62, 2),
                (265, 376, 61, 2),
                (340, 375, 62, 1),
                (223, 309, 62, 1),
                (121, 118, 326, 1)
            };
            Assert.Equal(expected_contours.Count, contours.Count);
            Assert.Equal(expected_contours, contours);

            (var img_bg, var threshold_bg) = _imageProcessing.AdaptiveThreshold(imagePath, true);
            (var vertical_mask_bg, var _) = _imageProcessing.FindLines(threshold_bg, direction: "vertical");
            (var horizontal_mask_bg, var _) = _imageProcessing.FindLines(threshold_bg, direction: "horizontal");

            var contours_bg = _imageProcessing.FindContours(vertical_mask_bg, horizontal_mask_bg);
            var expected_contours_bg = new List<(int, int, int, int)>()
            {
                (53, 415, 237, 319),
                (306, 540, 235, 132),
                (308, 416, 236, 122),
                (52, 396, 491, 6),
                (52, 407, 476, 5),
                (242, 260, 214, 57),
                (306, 705, 236, 5),
                (307, 717, 236, 7),
                (246, 181, 204, 5),
                (122, 86, 191, 6)
            };
            Assert.Equal(expected_contours_bg.Count, contours_bg.Count);
            Assert.Equal(expected_contours_bg, contours_bg);

            img.Dispose();
            threshold.Dispose();
            vertical_mask.Dispose();
            horizontal_mask.Dispose();

            img_bg.Dispose();
            threshold_bg.Dispose();
            vertical_mask_bg.Dispose();
            horizontal_mask_bg.Dispose();
        }

        [Fact]
        public void FindJoints()
        {
            const string imagePath = @"Files\PMC5055614_00002.jpg";
            OpenCvImageProcesser _imageProcessing = new OpenCvImageProcesser();

            (var img, var threshold) = _imageProcessing.AdaptiveThreshold(imagePath, true);
            (var vertical_mask, var _) = _imageProcessing.FindLines(threshold, direction: "vertical");
            (var horizontal_mask, var _) = _imageProcessing.FindLines(threshold, direction: "horizontal");

            var contours = _imageProcessing.FindContours(vertical_mask, horizontal_mask);
            var table_bbox = _imageProcessing.FindJoints(contours, vertical_mask, horizontal_mask);

            img.Dispose();
            threshold.Dispose();
            vertical_mask.Dispose();
            horizontal_mask.Dispose();

            var expected = new Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>>()
            {
                {
                    (53, 734, 290, 415),
                    new List<(float, float)>()
                    {
                        (55, 732),
                        (55, 721),
                        (55, 708),
                        (55, 695),
                        (55, 683),
                        (55, 671),
                        (55, 668),
                        (55, 660),
                        (195, 658),
                        (54, 646),
                        (195, 647),
                        (195, 634),
                        (55, 634),
                        (195, 621),
                        (55, 621),
                        (195, 610),
                        (55, 609),
                        (286, 598),
                        (55, 597),
                        (286, 585),
                        (55, 585),
                        (286, 573),
                        (54, 573),
                        (286, 561),
                        (54, 560),
                        (286, 549),
                        (54, 549),
                        (185, 549),
                        (54, 538),
                        (286, 536),
                        (185, 536),
                        (185, 524),
                        (54, 524),
                        (287, 512),
                        (185, 512),
                        (54, 512),
                        (185, 504),
                        (287, 501),
                        (185, 500),
                        (54, 500),
                        (287, 488),
                        (54, 487),
                        (287, 476),
                        (243, 476),
                        (54, 476),
                        (287, 465),
                        (243, 463),
                        (54, 463),
                        (54, 452),
                        (287, 452),
                        (243, 452),
                        (243, 443),
                        (287, 440),
                        (243, 440),
                        (54, 440),
                        (243, 427),
                        (54, 427),
                        (54, 416)
                    }
                },
                {
                    (306, 672, 541, 540),
                    new List<(float, float)>()
                    {
                        (309, 670),
                        (309, 658),
                        (540, 656),
                        (505, 657),
                        (393, 649),
                        (540, 645),
                        (505, 645),
                        (393, 645),
                        (308, 645),
                        (308, 633),
                        (540, 632),
                        (505, 632),
                        (393, 632),
                        (505, 624),
                        (540, 620),
                        (505, 620),
                        (393, 620),
                        (351, 621),
                        (308, 620),
                        (505, 612),
                        (308, 609),
                        (540, 608),
                        (505, 608),
                        (393, 608),
                        (351, 608),
                        (540, 597),
                        (351, 598),
                        (393, 597),
                        (308, 596),
                        (540, 585),
                        (393, 586),
                        (351, 584),
                        (308, 584),
                        (351, 576),
                        (308, 576),
                        (351, 574),
                        (540, 574),
                        (308, 573),
                        (540, 562),
                        (308, 561),
                        (540, 549)
                    }
                },
                {
                    (308, 538, 544, 416),
                    new List<(float, float)>()
                    {
                        (309, 535),
                        (309, 524),
                        (540, 522),
                        (540, 511),
                        (309, 511),
                        (540, 500),
                        (309, 499),
                        (540, 489),
                        (309, 487),
                        (309, 476),
                        (540, 475),
                        (540, 464),
                        (308, 464),
                        (308, 452),
                        (540, 451),
                        (540, 441),
                        (308, 440),
                        (308, 426)
                    }
                }
            };

            Assert.Equal(expected.Count, table_bbox.Count);
            Assert.Equal(expected, table_bbox);
        }

        [Fact]
        public void Process()
        {
            OpenCvImageProcesser _imageProcessing = new OpenCvImageProcesser();

#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var document = PdfDocument.Open(@"Files\foo.pdf", new ParsingOptions() { ClipPaths = true }))
#pragma warning restore IDE0063 // Use simple 'using' statement
            {
                var page = document.GetPage(1);

                (var table_bbox, var vertical_segments, var horizontal_segments) = _imageProcessing.Process(page, new BasicSystemDrawingProcessor(), false, 15, -2, 15, 0, null, null); //, out var table_bbox_unscaled);

                Assert.Single(table_bbox);
                Assert.Equal(new[] { (120.0f, 116.66666666666666f, 492.0f, 234.33333333333331f) }.ToList(), table_bbox.Keys.ToList(), new TestHelper.ListTuple4EqualityComparer(0));
                Assert.Equal(52, table_bbox.Values.First().Count); // should be 57, difference comes from rendering

                var vertical_segments_expected = new List<(float, float, float, float)>()
                {
                    (430.0f, 116.66666666666666f, 430.0f, 218.33333333333331f),
                    (374.66666666666663f,  116.66666666666666f,  374.66666666666663f,  218.33333333333331f),
                    (313.3333333333333f,  116.66666666666666f,  313.3333333333333f,  218.33333333333331f),
                    (491.3333333333333f, 116.66666666666666f, 491.3333333333333f, 234.0f),
                    (257.3333333333333f, 116.66666666666666f, 257.3333333333333f, 234.0f),
                    (205.0f, 116.66666666666666f, 205.0f, 234.0f),
                    (164.66666666666666f, 116.66666666666666f, 164.66666666666666f, 234.0f),
                    (120.33333333333333f, 116.66666666666666f, 120.33333333333333f, 234.0f)
                };
                Assert.Equal(vertical_segments_expected.Count, vertical_segments.Count);
                Assert.Equal(vertical_segments_expected, vertical_segments, new TestHelper.ListTuple4EqualityComparer(1));

                var horizontal_segments_expected = new List<(float, float, float, float)>()
                {
                    (120.33333333333333f, 117.33333333333333f, 492.0f, 117.33333333333333f),
                    (120.66f, 133.0f, 492.0f, 133.0f),                               // (120.3333333333333f, 133.0f, 492.0f, 133.0f),
                    (120.66f, 148.33333333333331f, 492.0f, 148.33333333333331f),    // (120.33333333333333f, 148.33333333333331f, 492.0f, 148.33333333333331f),
                    (120.66f, 164.0f, 492.0f, 164.0f),                              // (120.33333333333333f, 164.0f, 492.0f, 164.0f),
                    (120.66f, 179.33333333333331f, 492.0f, 179.33333333333331f),    // (120.33333333333333f, 179.33333333333331f, 492.0f, 179.33333333333331f),
                    (120.66f, 195.0f, 492.0f, 195.0f),                              // (120.33333333333333f, 195.0f, 492.0f, 195.0f),
                    (257.66666666666663f, 218.33333333333331f, 492.0f, 218.33333333333331f),
                    (120.33333333333333f, 234.0f, 492.0f, 234.0f)                               // (120.33333333333333f, 234.0f, 492.0f, 234.0f)
                };
                Assert.Equal(horizontal_segments_expected.Count, horizontal_segments.Count);
                Assert.Equal(horizontal_segments_expected, horizontal_segments, new TestHelper.ListTuple4EqualityComparer(1));
            }
        }
    }
}
