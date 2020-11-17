using Camelot.ImageProcessing.OpenCvSharp4;
using Camelot.Parsers;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using Xunit;

namespace Camelot.ImageProcessing.Tests
{
    public class LatticeTests
    {
        [Fact]
        public void test_repr()
        {
            Lattice lattice = new Lattice(new OpenCvImageProcesser(), new BasicSystemDrawingProcessor());
            var tables = lattice.extract_tables(@"Files\foo.pdf", layout_kwargs: null);
            Assert.Single(tables);
            Assert.Equal((7, 7), tables[0].Shape);
            Assert.Equal("<Cell x1=120.33 y1=218.33 x2=164.67 y2=234.01>", tables[0].Cells[0][0].ToString()); // "<Cell x1=120.48 y1=218.43 x2=164.64 y2=233.77>" in Python
        }

        // https://github.com/camelot-dev/camelot/blob/master/tests/data.py
        public string[][] data_lattice_shift_text_left_top = new string[][]
        {
            new string[]
            {
                "Investigations",
                "No. of\nHHs",
                "Age/Sex/\nPhysiological Group", // space removed
                "Preva-\nlence",
                "C.I*",
                "Relative\nPrecision",
                "Sample size\nper State",
            },
            new string[] { "Anthropometry", "2400", "All the available individuals", "", "", "", "" },
            new string[] { "Clinical Examination", "", "", "", "", "", "" },
            new string[] { "History of morbidity", "", "", "", "", "", "" },
            new string[]
            {
                "Diet survey",
                "1200",
                "All the individuals partaking meals in the HH",
                "",
                "",
                "",
                "",
            },
            new string[] { "Blood Pressure #", "2400", "Men (≥ 18yrs)", "10%", "95%", "20%", "1728" },
            new string[] { "", "", "Women (≥ 18 yrs)", "", "", "", "1728" },
            new string[] { "Fasting blood glucose", "2400", "Men (≥ 18 yrs)", "5%", "95%", "20%", "1825" },
            new string[] { "", "", "Women (≥ 18 yrs)", "", "", "", "1825" },
            new string[]
            {
                "Knowledge &\nPractices on HTN &\nDM",
                "2400",
                "Men (≥ 18 yrs)",
                "-",
                "-",
                "-",
                "1728",
            },
            new string[] { "", "2400", "Women (≥ 18 yrs)", "-", "-", "-", "1728" },
        };

        public string[][] data_lattice_shift_text_disable = new string[][]
        {
            new string[]
            {
                "Investigations",
                "No. of\nHHs",
                "Age/Sex/\nPhysiological Group", // space removed
                "Preva-\nlence",
                "C.I*",
                "Relative\nPrecision",
                "Sample size\nper State"
            },
            new string[] {"Anthropometry", "", "", "", "", "", ""},
            new string[] {"Clinical Examination", "2400", "", "All the available individuals", "", "", ""},
            new string[] {"History of morbidity", "", "", "", "", "", ""},
            new string[]
            {
                "Diet survey",
                "1200",
                "",
                "All the individuals partaking meals in the HH",
                "",
                "",
                ""
            },
            new string[] {"", "", "Men (≥ 18yrs)", "", "", "", "1728"},
            new string[] {"Blood Pressure #", "2400", "Women (≥ 18 yrs)", "10%", "95%", "20%", "1728"},
            new string[] {"", "", "Men (≥ 18 yrs)", "", "", "", "1825"},
            new string[] {"Fasting blood glucose", "2400", "Women (≥ 18 yrs)", "5%", "95%", "20%", "1825"},
            new string[] {
                "Knowledge &\nPractices on HTN &",
                "2400",
                "Men (≥ 18 yrs)",
                "-",
                "-",
                "-",
                "1728",
            },
            new string[] {"DM", "2400", "Women (≥ 18 yrs)", "-", "-", "-", "1728"},
        };

        public string[][] data_lattice_shift_text_right_bottom = new string[][]
        {
            new string[]
            {
                "Investigations",
                "No. of\nHHs",
                "Age/Sex/\nPhysiological Group", // space removed
                "Preva-\nlence",
                "C.I*",
                "Relative\nPrecision",
                "Sample size\nper State",
            },
            new string[] {"Anthropometry", "", "", "", "", "", ""},
            new string[] {"Clinical Examination", "", "", "", "", "", ""},
            new string[] {"History of morbidity", "2400", "", "", "", "", "All the available individuals"},
            new string[]
            {
                "Diet survey",
                "1200",
                "",
                "",
                "",
                "",
                "All the individuals partaking meals in the HH",
            },
            new string[] {"", "", "Men (≥ 18yrs)", "", "", "", "1728"},
            new string[] {"Blood Pressure #", "2400", "Women (≥ 18 yrs)", "10%", "95%", "20%", "1728"},
            new string[] {"", "", "Men (≥ 18 yrs)", "", "", "", "1825"},
            new string[] {"Fasting blood glucose", "2400", "Women (≥ 18 yrs)", "5%", "95%", "20%", "1825"},
            new string[] {"", "2400", "Men (≥ 18 yrs)", "-", "-", "-", "1728"},
            new string[]
            {
                "Knowledge &\nPractices on HTN &\nDM",
                "2400",
                "Women (≥ 18 yrs)",
                "-",
                "-",
                "-",
                "1728",
            },
        };

        [Fact]
        public void test_lattice_shift_text()
        {
            Lattice lattice = new Lattice(new OpenCvImageProcesser(), new BasicSystemDrawingProcessor(), line_scale: 40);
            var tables = lattice.extract_tables(@"Files\column_span_2.pdf",
                layout_kwargs: new DlaOptions[]
                {
                    new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
                    {
                         WithinLineMultiplier = 2
                    }
                });

            Assert.Single(tables);
            Assert.Equal(data_lattice_shift_text_left_top.Length, tables[0].Cells.Count);
            Assert.Equal(data_lattice_shift_text_left_top, tables[0].Data().Select(r => r.Select(c => c).ToArray()).ToArray());

            lattice = new Lattice(new OpenCvImageProcesser(), new BasicSystemDrawingProcessor(), line_scale: 40, shift_text: new[] { "" });
            tables = lattice.extract_tables(@"Files\column_span_2.pdf",
                layout_kwargs: new DlaOptions[]
                {
                    new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
                    {
                         WithinLineMultiplier = 2
                    }
                });
            Assert.Single(tables);
            Assert.Equal(data_lattice_shift_text_disable.Length, tables[0].Cells.Count);
            Assert.Equal(data_lattice_shift_text_disable, tables[0].Data().Select(r => r.Select(c => c).ToArray()).ToArray());

            lattice = new Lattice(new OpenCvImageProcesser(), new BasicSystemDrawingProcessor(), line_scale: 40, shift_text: new[] { "r", "b" });
            tables = lattice.extract_tables(@"Files\column_span_2.pdf",
                layout_kwargs: new DlaOptions[]
                {
                    new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
                    {
                         WithinLineMultiplier = 2
                    }
                });
            Assert.Single(tables);
            Assert.Equal(data_lattice_shift_text_right_bottom.Length, tables[0].Cells.Count);
            Assert.Equal(data_lattice_shift_text_right_bottom, tables[0].Data().Select(r => r.Select(c => c).ToArray()).ToArray());
        }
    }
}
