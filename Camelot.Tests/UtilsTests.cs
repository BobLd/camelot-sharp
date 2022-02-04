using System;
using System.Collections.Generic;
using UglyToad.PdfPig.Core;
using Xunit;
using static Camelot.Core;

namespace Camelot.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void Translate()
        {
            Assert.Equal(1, Utils.Translate(0, 1), 4);
            Assert.Equal(15, Utils.Translate(10, 5), 4);
            Assert.Equal(73, Utils.Translate(-5, 78), 4);
            Assert.Equal(88.52f, Utils.Translate(105, -16.48f), 4);
            Assert.Equal(-41.23f, Utils.Translate(-26.97f, -14.26f), 4);
            Assert.Equal(-8.55f, Utils.Translate(5.71f, -14.26f), 4);

            Assert.True(float.IsNaN(Utils.Scale(float.NaN, 1.25f)));
            Assert.True(float.IsNaN(Utils.Scale(48, float.NaN)));

            Assert.True(float.IsPositiveInfinity(Utils.Translate(48, float.PositiveInfinity)));
            Assert.True(float.IsPositiveInfinity(Utils.Translate(-48, float.PositiveInfinity)));
            Assert.True(float.IsNegativeInfinity(Utils.Translate(48, float.NegativeInfinity)));
            Assert.True(float.IsNegativeInfinity(Utils.Translate(-48, float.NegativeInfinity)));
            Assert.True(float.IsPositiveInfinity(Utils.Translate(float.PositiveInfinity, 1.25f)));
            Assert.True(float.IsPositiveInfinity(Utils.Translate(float.PositiveInfinity, -1.25f)));
            Assert.True(float.IsNegativeInfinity(Utils.Translate(float.NegativeInfinity, 1.25f)));
            Assert.True(float.IsNegativeInfinity(Utils.Translate(float.NegativeInfinity, -1.25f)));
        }

        [Fact]
        public void Scale()
        {
            Assert.Equal(0, Utils.Scale(0, 1), 3);
            Assert.Equal(50, Utils.Scale(10, 5), 3);
            Assert.Equal(-390, Utils.Scale(-5, 78), 3);
            Assert.Equal(-1730.4f, Utils.Scale(105, -16.48f), 3);
            Assert.Equal(384.5922f, Utils.Scale(-26.97f, -14.26f), 3);
            Assert.Equal(-81.4246f, Utils.Scale(5.71f, -14.26f), 3);

            Assert.True(float.IsNaN(Utils.Scale(float.NaN, 1.25f)));
            Assert.True(float.IsNaN(Utils.Scale(48, float.NaN)));

            Assert.True(float.IsPositiveInfinity(Utils.Scale(48, float.PositiveInfinity)));
            Assert.True(float.IsNegativeInfinity(Utils.Scale(-48, float.PositiveInfinity)));
            Assert.True(float.IsPositiveInfinity(Utils.Scale(float.PositiveInfinity, 1.25f)));
            Assert.True(float.IsNegativeInfinity(Utils.Scale(float.PositiveInfinity, -1.25f)));
        }

        [Fact]
        public void ScalePdf()
        {
            Assert.Equal((0, 12, 9, 6), Utils.ScalePdf((0f, 1f, 2f, 3f), (4.5f, 3.2f, 5f)));
            Assert.Equal((-12, 34, -56, 19), Utils.ScalePdf((-4, 1, -18, 3), (3.14f, 7.4f, 5.7f)));
            Assert.Equal((12, -34, 56, -19), Utils.ScalePdf((-4, -1, -18, -3), (-3.14f, -7.4f, -5.7f)));
            Assert.Equal((202, 104, 3, 1592), Utils.ScalePdf((78.458f, 42.581f, 1.287f, 945.658f), (2.58f, 1.879f, 98.123f)));

            Assert.Throws<ArgumentOutOfRangeException>("factors", () => Utils.ScalePdf((-4, -1, -18, -3), (-3.14f, float.PositiveInfinity, -5.7f)));
            Assert.Throws<ArgumentOutOfRangeException>("factors", () => Utils.ScalePdf((-4, -1, -18, -3), (float.NegativeInfinity, 0.1f, -5.7f)));
            Assert.Throws<ArgumentOutOfRangeException>("factors", () => Utils.ScalePdf((-4, -1, -18, -3), (4.7f, 0.1f, float.NaN)));

            Assert.Throws<ArgumentOutOfRangeException>("k", () => Utils.ScalePdf((float.PositiveInfinity, 1, -18, 3), (3.14f, 7.4f, 5.7f)));
            Assert.Throws<ArgumentOutOfRangeException>("k", () => Utils.ScalePdf((17.8f, float.NegativeInfinity, -18, 3), (3.14f, 7.4f, 5.7f)));
            Assert.Throws<ArgumentOutOfRangeException>("k", () => Utils.ScalePdf((17.8f, 1, float.NaN, 3), (3.14f, 7.4f, 5.7f)));
            Assert.Throws<ArgumentOutOfRangeException>("k", () => Utils.ScalePdf((17.8f, 1, 43, float.NaN), (3.14f, 7.4f, 5.7f)));
        }

        [Fact]
        public void BboxIntersect()
        {
            Assert.False(Utils.BboxIntersect((71.99965000000003f, 263.8804000000001f, 540.41949376f, 275.8804000000001f), (125.88f, 181.73604f, 162.02216f, 191.75603999999998f)));
            Assert.False(Utils.BboxIntersect((71.99965000000003f, 263.8804000000001f, 540.41949376f, 275.8804000000001f), (175.1382f, 181.73604f, 197.36246000000003f, 191.75603999999998f)));
            Assert.False(Utils.BboxIntersect((145.07965000000002f, 240.2960000000001f, 469.63326095968006f, 250.3160000000001f), (274.0156f, 166.19604f, 299.595299345f, 176.21604000000002f)));
            Assert.False(Utils.BboxIntersect((145.07965000000002f, 240.2960000000001f, 469.63326095968006f, 250.3160000000001f), (391.0094f, 166.19604f, 416.588699345f, 176.21604000000002f)));
            Assert.False(Utils.BboxIntersect((323.9270190000001f, 220.5468000000001f, 427.81457900000004f, 230.5668000000001f), (332.63280000000003f, 166.19604f, 358.212099345f, 176.21604000000002f)));
            Assert.False(Utils.BboxIntersect((323.9270190000001f, 220.5468000000001f, 427.81457900000004f, 230.5668000000001f), (391.0094f, 166.19604f, 416.588699345f, 176.21604000000002f)));
            Assert.False(Utils.BboxIntersect((129.22863000000007f, 214.5546000000001f, 158.64270587028008f, 224.5746000000001f), (271.2601f, 119.69604000000001f, 302.42060876539995f, 129.71604000000002f)));
            Assert.False(Utils.BboxIntersect((129.22863000000007f, 214.5546000000001f, 158.64270587028008f, 224.5746000000001f), (330.72f, 195.2958f, 360.20556f, 205.3158f)));
            Assert.False(Utils.BboxIntersect((222.1137190000001f, 203.0316000000001f, 243.1957790000001f, 213.0516000000001f), (380.16f, 206.75580000000002f, 427.39639533222f, 216.7758f)));
            Assert.False(Utils.BboxIntersect((330.72f, 195.2958f, 360.20556f, 205.3158f), (71.99965000000003f, 263.8804000000001f, 540.41949376f, 275.8804000000001f)));
            Assert.False(Utils.BboxIntersect((452.1f, 195.2958f, 472.10546f, 205.3158f), (175.1382f, 181.73604f, 197.36246000000003f, 191.75603999999998f)));

            Assert.True(Utils.BboxIntersect(new PdfRectangle(0, 0, 10, 10), new PdfRectangle(5, 5, 15, 15)));
            Assert.True(Utils.BboxIntersect(new PdfRectangle(-5, -7, 18, 40), new PdfRectangle(0, 5, 19, 15)));
            Assert.True(Utils.BboxIntersect(new PdfRectangle(-5, -7, 18, 40), new PdfRectangle(-10, -10, 19, 45)));
            Assert.True(Utils.BboxIntersect(new PdfRectangle(-10, -10, 19, 45), new PdfRectangle(-5, -7, 18, 40)));
            Assert.False(Utils.BboxIntersect(new PdfRectangle(100, -10, 19, 45), new PdfRectangle(-5, -7, 18, 40)));
            Assert.False(Utils.BboxIntersect(new PdfRectangle(100, 100, 110, 110), new PdfRectangle(-5, -7, 18, 40)));
            Assert.False(Utils.BboxIntersect(new PdfRectangle(100, 20, 110, 110), new PdfRectangle(-5, -7, 99, 40)));
            Assert.True(Utils.BboxIntersect(new PdfRectangle(100, 20, 110, 110), new PdfRectangle(-5, -7, 100, 40)));
        }

        [Fact]
        public void ComputeAccuracy()
        {
            var errors = new float[] { 0.7938163139615321f, 0.8783598497540532f, 0.47427074687768156f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
            Assert.Equal(95.87221747962823f, Utils.ComputeAccuracy(new[] { (100f, (IReadOnlyList<float>)errors) }), 3);
        }

        [Fact]
        public void ComputeWhitespace()
        {
            var d = new string[][] // foo.pdf
            {
                new[] { "reducing the number of stops in high KI cycles, and not just the rate of accelerating out of a stop.", "", "", "", "", "", "" },
                new[] { "", "Table 2-1. Simulated fuel savings from isolated cycle improvements", "", "", "", "", "" },
                new[] { "", "", "", "", "", "Percent Fuel Savings", "" },
                new[] { "Cycle", "KI", "Distance", "", "", "", "" },
                new[] { "", "", "", "Improved", "Decreased", "Eliminate", "Decreased" },
                new[] { "Name", "(1/km)", "(mi)", "", "", "", "" },
                new[] { "", "", "", "Speed", "Accel", "Stops", "Idle" },
                new[] { "2012_2", "3.30", "1.3", "5.9%", "9.5%", "29.2%", "17.4%" },
                new[] { "2145_1", "0.68", "11.2", "2.4%", "0.1%", "9.5%", "2.7%" },
                new[] { "4234_1", "0.59", "58.7", "8.5%", "1.3%", "8.5%", "3.3%" },
                new[] { "2032_2", "0.17", "57.8", "21.7%", "0.3%", "2.7%", "1.2%" },
                new[] { "4171_1", "0.07", "173.9", "58.1%", "1.6%", "2.1%", "0.5%" }
            };
            Assert.Equal(38.095238f, Utils.ComputeWhitespace(d), 5);

            // https://r12a.github.io/app-conversion/
            var d1 = new string[][] // column_span_2.pdf
            {
                new[] { "Investigations\nNo. of\nAge/Sex/", "Preva-", "C.I*", "Relative", "Sample size" },
                new[] { "HHs\nPhysiological  Group", "lence", "", "Precision", "per State" },
                new[] { "Anthropometry", "", "", "", "" },
                new[] { "Clinical Examination\n2400", "All the available individuals", "", "", "" },
                new[] { "History of morbidity", "", "", "", "" },
                new[] { "Diet survey\n1200", "All the individuals partaking meals in the HH", "", "", "" },
                new[] { "Men (\u2265 18yrs)", "", "", "", "1728" },
                new[] { "Blood Pressure #\n2400", "10%", "95%", "20%", "" },
                new[] { "Women (\u2265 18 yrs)", "", "", "", "1728" },
                new[] { "Men (\u2265 18 yrs)", "", "", "", "1825" },
                new[] { "Fasting blood glucose\\\n2400", "5%", "95%", "20%", "" },
                new[] { "Women (\u2265 18 yrs)", "", "", "", "1825" },
                new[] { "Knowledge &\n2400\nMen (\u2265 18 yrs)", "-", "-", "-", "1728" },
                new[] { "Practices on HTN &", "", "", "", "" },
                new[] { "DM\n2400\nWomen (\u2265 18 yrs)", "-", "-", "-", "1728" },
                new[] { "*CI: Confidence Interval       # Design effect 2", "", "", "", "" }
            };
            Assert.Equal(46.25f, Utils.ComputeWhitespace(d1), 5);
        }

        [Fact]
        public void GetTableIndex()
        {
            const string direction = "horizontal";
            var table = new Table(TableTests.FooCols, TableTests.FooRows);

            var l_table0 = TestHelper.MakeTextLine(new float[] { 80, 200, 200, 250 }, "l_table0");
            var (indices, error) = Utils.GetTableIndex(table, l_table0, direction);
            Assert.Equal(0.2618318f, error, 5);
            Assert.Equal(2, indices[0].r_idx);
            Assert.Equal(0, indices[0].c_idx);
            Assert.Equal("l_table0", indices[0].text);

            var l_table1 = TestHelper.MakeTextLine(new float[] { 80, 100, 180, 150 }, "l_table1");
            var ti1 = Utils.GetTableIndex(table, l_table1, direction);
            Assert.Equal(0.1141982f, ti1.error, 5);
            Assert.Equal(11, ti1.indices[0].r_idx);
            Assert.Equal(0, ti1.indices[0].c_idx);
            Assert.Equal("l_table1", ti1.indices[0].text);

            var l_table2 = TestHelper.MakeTextLine(new float[] { 150, 100, 180, 150 }, "l_table2");
            var ti2 = Utils.GetTableIndex(table, l_table2, direction);
            Assert.Equal(0.619339f, ti2.error, 5);
            Assert.Equal(11, ti2.indices[0].r_idx);
            Assert.Equal(1, ti2.indices[0].c_idx);
            Assert.Equal("l_table2", ti2.indices[0].text);

            var l_table3 = TestHelper.MakeTextLine(new float[] { 200, 100, 250, 150 }, "l_table3");
            var ti3 = Utils.GetTableIndex(table, l_table3, direction);
            Assert.Equal(0.1611786f, ti3.error, 5);
            Assert.Equal(11, ti3.indices[0].r_idx);
            Assert.Equal(2, ti3.indices[0].c_idx);
            Assert.Equal("l_table3", ti3.indices[0].text);

            // need to add checks for 
            // - var ti = Utils.get_table_index(table, l_table3, "horizontal", true);
            // - var ti = Utils.get_table_index(table, l_table3, "horizontal", false, true);
            // - var ti = Utils.get_table_index(table, l_table3, "horizontal", true, true, " ");
        }

        [Fact]
        public void TextStrip()
        {
            Assert.Equal("this is a test", Utils.TextStrip("this is a test", ""));
            Assert.Equal("thisisatest", Utils.TextStrip("this is a test", " "));
            Assert.Equal("this is a test ", Utils.TextStrip("this is a test (pdf)", "(pdf)"));
            Assert.Equal("this is a test \n", Utils.TextStrip("this is a test \n", "n"));
            Assert.Equal("this is a test ", Utils.TextStrip("this is a test \n", "\n"));
            Assert.Equal("this is a test \\t", Utils.TextStrip("this is a test \\t", "\t"));
            Assert.Equal("this is a test \\", Utils.TextStrip("this is a test \\\t", "\t"));
            Assert.Equal("this is  test \\\t", Utils.TextStrip("this is a test \\\t", "a"));
            Assert.Equal("", Utils.TextStrip("(camelot)", "(camelot)"));

            // not sure if expected behavior, but match the python's version
            Assert.Equal("/", Utils.TextStrip("(came/lot)", "(camelot)"));
            Assert.Equal("/", Utils.TextStrip("came/lot", "camerlot"));
            Assert.Equal("", Utils.TextStrip("camelot", "camerererelot"));
            Assert.Equal("(/)", Utils.TextStrip("(camel/ot)", "camelot"));
        }
    }
}
