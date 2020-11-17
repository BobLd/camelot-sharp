using Camelot.Parsers;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using Xunit;

namespace Camelot.Tests
{
    public class StreamTests
    {
        private readonly TextLine[] column_span_2_text = new TextLine[]
        {
            TestHelper.MakeTextLine(336.144,687.721,366.278,697.221, "Preva-\n"),
            TestHelper.MakeTextLine(379.768,687.721,396.260,697.221, "C.I*\n"),
            TestHelper.MakeTextLine(415.650,687.721,453.612,697.221, "Relative\n"),
            TestHelper.MakeTextLine(339.029,676.326,364.119,685.826, "lence\n"),
            TestHelper.MakeTextLine(412.759,676.326,457.124,685.826, "Precision\n"),
            TestHelper.MakeTextLine(341.661,559.179,361.107,568.679, "10%\n"),
            TestHelper.MakeTextLine(378.511,559.179,397.958,568.679, "95%\n"),
            TestHelper.MakeTextLine(425.251,559.179,444.698,568.679, "20%\n"),
            TestHelper.MakeTextLine(344.410,514.844,358.356,524.344, "5%\n"),
            TestHelper.MakeTextLine(378.515,514.844,397.962,524.344, "95%\n"),
            TestHelper.MakeTextLine(425.255,514.844,444.702,524.344, "20%\n"),
            TestHelper.MakeTextLine(349.779,484.683,352.943,494.183, "-\n"),
            TestHelper.MakeTextLine(386.630,484.683,389.793,494.183, "-\n"),
            TestHelper.MakeTextLine(433.370,484.683,436.533,494.183, "-\n"),
            TestHelper.MakeTextLine(349.796,461.892,352.959,471.392, "-\n"),
            TestHelper.MakeTextLine(386.646,461.892,389.810,471.392, "-\n"),
            TestHelper.MakeTextLine(433.386,461.892,436.550,471.392, "-\n"),
            TestHelper.MakeTextLine(95.414,687.721,162.133,697.221, "Investigations\n"),
            TestHelper.MakeTextLine(189.683,687.721,217.955,697.221, "No. of\n"),
            TestHelper.MakeTextLine(194.202,676.326,213.601,685.826, "HHs\n"),
            TestHelper.MakeTextLine(84.898,653.517,149.935,663.017, "Anthropometry\n"),
            TestHelper.MakeTextLine(84.898,632.767,174.834,642.267, "Clinical Examination\n"),
            TestHelper.MakeTextLine(193.093,632.767,214.848,642.267, "2400\n"),
            TestHelper.MakeTextLine(84.898,612.017,170.711,621.517, "History of morbidity\n"),
            TestHelper.MakeTextLine(84.898,591.267,134.839,600.767, "Diet survey\n"),
            TestHelper.MakeTextLine(193.122,591.267,214.877,600.767, "1200\n"),
            TestHelper.MakeTextLine(84.898,559.179,161.116,568.679, "Blood Pressure #\n"),
            TestHelper.MakeTextLine(193.008,559.179,214.734,568.679, "2400\n"),
            TestHelper.MakeTextLine(84.898,514.844,181.883,524.344, "Fasting blood glucose\n"),
            TestHelper.MakeTextLine(193.027,514.844,214.782,524.344 ,"2400\n"),
            TestHelper.MakeTextLine(84.898,484.683,143.066,494.183, "Knowledge &\n"),
            TestHelper.MakeTextLine(193.074,484.683,214.801,494.183, "2400\n"),
            TestHelper.MakeTextLine(84.897,473.287,172.325,482.787, "Practices on HTN &\n"),
            TestHelper.MakeTextLine(84.897,461.892,99.869,471.392, "DM\n"),
            TestHelper.MakeTextLine(193.016,461.892,214.771,471.392, "2400\n")
        };

        private readonly TextLine[] column_span_2_horizontal_text = new TextLine[] // column_span_2_
        {
            TestHelper.MakeTextLine(107.603,726.010,532.528,737.510, "The sample size required for each State for various investigations among different\n"),
            TestHelper.MakeTextLine(79.257,711.610,314.029,723.110, "target groups of individuals are given below:\n"),
            TestHelper.MakeTextLine(95.414,687.721,162.133,697.221, "Investigations\n"),
            TestHelper.MakeTextLine(189.683,687.721,217.955,697.221, "No. of\n"),
            TestHelper.MakeTextLine(258.149,687.721,299.807,697.221, "Age/Sex/\n"),
            TestHelper.MakeTextLine(336.144,687.721,366.278,697.221, "Preva-\n"),
            TestHelper.MakeTextLine(379.768,687.721,396.260,697.221, "C.I*\n"),
            TestHelper.MakeTextLine(415.650,687.721,453.612,697.221, "Relative\n"),
            TestHelper.MakeTextLine(468.603,687.721,525.546,697.221, "Sample size\n"),
            TestHelper.MakeTextLine(194.202,676.326,213.601,685.826, "HHs\n"),
            TestHelper.MakeTextLine(230.691,676.326,327.658,685.826, "Physiological  Group\n"),
            TestHelper.MakeTextLine(339.029,676.326,364.119,685.826, "lence\n"),
            TestHelper.MakeTextLine(412.759,676.326,457.124,685.826, "Precision\n"),
            TestHelper.MakeTextLine(476.143,676.326,518.304,685.826, "per State\n"),
            TestHelper.MakeTextLine(84.898,653.517,149.935,663.017, "Anthropometry\n"),
            TestHelper.MakeTextLine(84.898,632.767,174.834,642.267, "Clinical Examination\n"),
            TestHelper.MakeTextLine(193.093,632.767,214.848,642.267, "2400\n"),
            TestHelper.MakeTextLine(232.423,632.767,430.014,642.267, "                           All the available individuals\n"),
            TestHelper.MakeTextLine(84.898,612.017,170.711,621.517, "History of morbidity\n"),
            TestHelper.MakeTextLine(84.898,591.267,134.839,600.767, "Diet survey\n"),
            TestHelper.MakeTextLine(193.122,591.267,214.877,600.767, "1200\n"),
            TestHelper.MakeTextLine(232.395,591.267,427.753,600.767, "All the individuals partaking meals in the HH\n"),
            TestHelper.MakeTextLine(232.300,570.517,293.626,581.803, "Men (> 18yrs)\n"),
            TestHelper.MakeTextLine(486.454,570.517,508.181,580.017, "1728\n"),
            TestHelper.MakeTextLine(84.898,559.179,161.116,568.679, "Blood Pressure #\n"),
            TestHelper.MakeTextLine(193.008,559.179,214.734,568.679, "2400\n"),
            TestHelper.MakeTextLine(341.661,559.179,361.107,568.679, "10%\n"),
            TestHelper.MakeTextLine(378.511,559.179,397.958,568.679, "95%\n"),
            TestHelper.MakeTextLine(425.251,559.179,444.698,568.679, "20%\n"),
            TestHelper.MakeTextLine(232.300,549.767,311.103,561.053, "Women (> 18 yrs)\n"),
            TestHelper.MakeTextLine(486.454,549.767,508.181,559.267, "1728\n"),
            TestHelper.MakeTextLine(232.300,526.183,296.514,537.469, "Men (> 18 yrs)\n"),
            TestHelper.MakeTextLine(486.454,526.183,508.181,535.683, "1825\n"),
            TestHelper.MakeTextLine(84.898,514.844,181.883,524.344, "Fasting blood glucose\n"),
            TestHelper.MakeTextLine(193.027,514.844,214.782,524.344, "2400\n"),
            TestHelper.MakeTextLine(344.410,514.844,358.356,524.344, "5%\n"),
            TestHelper.MakeTextLine(378.515,514.844,397.962,524.344, "95%\n"),
            TestHelper.MakeTextLine(425.255,514.844,444.702,524.344, "20%\n"),
            TestHelper.MakeTextLine(232.300,505.433,311.103,516.719, "Women (> 18 yrs)\n"),
            TestHelper.MakeTextLine(486.454,505.433,508.181,514.933, "1825\n"),
            TestHelper.MakeTextLine(84.898,484.683,143.066,494.183, "Knowledge &\n"),
            TestHelper.MakeTextLine(193.074,484.683,214.801,494.183, "2400\n"),
            TestHelper.MakeTextLine(232.319,484.683,296.513,495.969, "Men (> 18 yrs)\n"),
            TestHelper.MakeTextLine(349.779,484.683,352.943,494.183, "-\n"),
            TestHelper.MakeTextLine(386.630,484.683,389.793,494.183, "-\n"),
            TestHelper.MakeTextLine(433.370,484.683,436.533,494.183, "-\n"),
            TestHelper.MakeTextLine(486.437,484.683,508.163,494.183, "1728\n"),
            TestHelper.MakeTextLine(84.897,473.287,172.325,482.787, "Practices on HTN &\n"),
            TestHelper.MakeTextLine(84.897,461.892,99.869,471.392, "DM\n"),
            TestHelper.MakeTextLine(193.016,461.892,214.771,471.392, "2400\n"),
            TestHelper.MakeTextLine(232.289,461.892,311.102,473.178, "Women (> 18 yrs)\n"),
            TestHelper.MakeTextLine(349.796,461.892,352.959,471.392, "-\n"),
            TestHelper.MakeTextLine(386.646,461.892,389.810,471.392, "-\n"),
            TestHelper.MakeTextLine(433.386,461.892,436.550,471.392, "-\n"),
            TestHelper.MakeTextLine(486.453,461.892,508.180,471.392, "1728\n"),
            TestHelper.MakeTextLine(84.897,436.324,285.822,445.824, "*CI: Confidence Interval       # Design effect 2\n"),
            TestHelper.MakeTextLine(79.257,408.121,294.629,419.621, "3.4.2. Measurement of blood pressure\n"),
            TestHelper.MakeTextLine(107.603,388.063,532.804,399.563, "Earlier studies have revealed that the prevalence of hypertension among the rural\n"),
            TestHelper.MakeTextLine(79.257,373.663,532.770,387.325, "adults  of  >18  years  in  the  State  of  Gujarat  was  10%3 which  was  lowest  compared  to\n"),
            TestHelper.MakeTextLine(79.257,359.263,533.001,370.763, "other  NNMB  States.  Assuming  10%  prevalence  of  hypertension  among  rural  adults,\n"),
            TestHelper.MakeTextLine(79.257,344.863,532.322,356.363, "with 95% confidence interval, 20% relative precision and design effect of 2, the sample\n"),
            TestHelper.MakeTextLine(79.257,330.463,413.481,341.963, "size computed for each State was 1,728 adults in each gender.\n"),
            TestHelper.MakeTextLine(79.257,301.652,316.697,313.152, "3.4.3. Estimation of fasting blood glucose\n"),
            TestHelper.MakeTextLine(107.603,281.622,532.356,293.122, "Earlier studies have revealed that the overall prevalence of diabetes among the\n"),
            TestHelper.MakeTextLine(79.257,267.222,532.930,280.884, "rural adults of >18 years was about 5%3. Assuming 5% prevalence of diabetes among\n"),
            TestHelper.MakeTextLine(79.257,252.822,532.737,264.322, "rural adults, with 95% confidence interval and 20% relative precision, the sample size\n"),
            TestHelper.MakeTextLine(79.257,238.422,310.890,249.922, "computed was 1,825 adults in each gender.\n"),
            TestHelper.MakeTextLine(79.257,209.611,183.539,221.111, "3.5.  Investigations\n"),
            TestHelper.MakeTextLine(107.603,189.582,456.134,201.082, "The following investigations were carried out in the selected HHs:\n"),
            TestHelper.MakeTextLine(79.257,160.771,458.377,172.271, "3.5.1. Socio-economic and demographic particulars of households\n"),
            TestHelper.MakeTextLine(107.603,140.712,532.988,152.212, "Socioeconomic and demographic particulars, such as age, sex, occupation, annual\n"),
            TestHelper.MakeTextLine(79.257,126.312,532.909,137.812, "family income, family size, type of family, literacy level of individuals, information about\n"),
            TestHelper.MakeTextLine(79.257,111.912,532.909,123.412, "possession of agricultural land, types of crops raised, their yield during previous year,\n"),
            TestHelper.MakeTextLine(79.257,97.511,532.714,109.011, "live stock, type of dwelling, environmental sanitation etc. from all the selected 20 HHs\n"),
            TestHelper.MakeTextLine(79.257,83.111,330.889,94.611, "were collected using pre-tested questionnaires.\n"),
            TestHelper.MakeTextLine(83.069,53.886,115.234,63.886, "NNMB\n"),
            TestHelper.MakeTextLine(303.052,53.886,308.612,63.886, "3\n"),
            TestHelper.MakeTextLine(362.249,53.886,530.164,63.886, "Rural-Third Repeat Survey 2011-12\n'>")
        };

        [Fact]
        public void AddColumns()
        {
            // column_span_2.pdf
            var cols = new List<(float, float)>() { (232.2996050000001f, 311.1031500000001f), (486.4540550000001f, 508.1806500000001f) };
            var actual = Stream.AddColumns(cols, column_span_2_text, 2);

            var expected = new List<(float, float)>()
            {
                (232.2996050000001f, 311.1031500000001f),
                (486.4540550000001f, 508.1806500000001f),
                (336.1442f, 366.2782f),
                (378.51135f, 397.9616500000001f),
                (415.64969999999994f, 453.61170000000004f)
            };

            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected, actual, new TestHelper.ListTuple2EqualityComparer(3));
        }

        [Fact]
        public void _group_rows()
        {
            Assert.Equal(17, Stream.GroupRows(column_span_2_text, 2).Count);
        }

        [Fact]
        public void _join_rows()
        {
            var rows_grouped = new List<List<TextLine>>
            {
                new List<TextLine>()
                {
                    column_span_2_text[30-13],
                    column_span_2_text[31-13],
                    TestHelper.MakeTextLine(258.149,687.721,299.807,697.221, "Age/Sex/\n"),
                    column_span_2_text[0],
                    column_span_2_text[1],
                    column_span_2_text[2],
                    TestHelper.MakeTextLine(468.603,687.721,525.546,697.221 , "Sample size\n")
                },
                new List<TextLine>()
                {
                    column_span_2_text[32-13],
                    TestHelper.MakeTextLine(230.691,676.326,327.658,685.826, "Physiological  Group\n"),
                    column_span_2_text[3],
                    column_span_2_text[4],
                    TestHelper.MakeTextLine(476.143,676.326,518.304,685.826, "per State\n")
                },
                new List<TextLine>()
                {
                    column_span_2_text[33-13]
                },
                new List<TextLine>()
                {
                    column_span_2_text[34-13],
                    column_span_2_text[35-13],
                    TestHelper.MakeTextLine(232.423,632.767,430.014,642.267, "                           All the available individuals\n")
                },
                new List<TextLine>()
                {
                    column_span_2_text[36-13]
                },
                new List<TextLine>()
                {
                    column_span_2_text[37-13],
                    column_span_2_text[38-13],
                    TestHelper.MakeTextLine(232.395,591.267,427.753,600.767, "All the individuals partaking meals in the HH\n")
                },
                new List<TextLine>()
                {
                    TestHelper.MakeTextLine(232.300,570.517,293.626,581.803, "Men (> 18yrs)\n"),
                    TestHelper.MakeTextLine(486.454,570.517,508.181,580.017, "1728\n")
                },
                new List<TextLine>()
                {
                    column_span_2_text[39-13],
                    column_span_2_text[40-13],
                    column_span_2_text[18-13],
                    column_span_2_text[19-13],
                    column_span_2_text[20-13]
                },
                new List<TextLine>()
                {
                    TestHelper.MakeTextLine(232.300,549.767,311.103,561.053, "Women (> 18 yrs)\n"),
                    TestHelper.MakeTextLine(486.454,549.767,508.181,559.267, "1728\n")
                },
                new List<TextLine>()
                {
                    TestHelper.MakeTextLine(232.300,526.183,296.514,537.469, "Men (> 18 yrs)\n"),
                    TestHelper.MakeTextLine(486.454,526.183,508.181,535.683, "1825\n")
                },
                new List<TextLine>()
                {
                    column_span_2_text[41-13],
                    column_span_2_text[42-13],
                    column_span_2_text[21-13],
                    column_span_2_text[22-13],
                    column_span_2_text[23-13],
                },
                new List<TextLine>()
                {
                    TestHelper.MakeTextLine(232.300,505.433,311.103,516.719, "Women (> 18 yrs)\n"),
                    TestHelper.MakeTextLine(486.454,505.433,508.181,514.933, "1825\n")
                },
                new List<TextLine>()
                {
                    column_span_2_text[43-13],
                    column_span_2_text[44-13],
                    TestHelper.MakeTextLine(232.319,484.683,296.513,495.969,"Men (> 18 yrs)\n"),
                    column_span_2_text[24-13],
                    column_span_2_text[25-13],
                    column_span_2_text[26-13],
                    TestHelper.MakeTextLine(486.437,484.683,508.163,494.183, "1728\n")
                },
                new List<TextLine>()
                {
                    column_span_2_text[45-13]
                },
                new List<TextLine>()
                {
                    column_span_2_text[46-13],
                    column_span_2_text[47-13],
                    TestHelper.MakeTextLine(232.289,461.892,311.102,473.178, "Women (> 18 yrs)\n"),
                    column_span_2_text[27-13],
                    column_span_2_text[28-13],
                    column_span_2_text[29-13],
                    TestHelper.MakeTextLine(486.453,461.892,508.180,471.392, "1728\n")
                },
                new List<TextLine>()
                {
                    TestHelper.MakeTextLine(84.897,436.324,285.822,445.824, "*CI: Confidence Interval       # Design effect 2\n")
                }
            };

            var actual = Stream.JoinRows(rows_grouped, 697.2215f, 436.32394999999985f);

            List<(float, float)> expected = new List<(float, float)>()
            {
                (697.2215f, 686.7735f),
                (686.7735f, 669.6715f),
                (669.6715f, 647.8918f),
                (647.8918f, 627.1419f),
                (627.1419f, 606.3920f),
                (606.3920f, 585.86535f),
                (585.86535f, 569.821275f),
                (569.821275f, 559.446325f),
                (559.446325f, 543.1715f),
                (543.1715f, 525.486675f),
                (525.486675f, 515.111725f),
                (515.111725f, 500.09473571428555f),
                (500.09473571428555f, 483.79891071428557f),
                (483.79891071428557f, 472.4033f),
                (472.4033f, 453.9218857142856f),
                (453.9218857142856f, 436.32395f)
            };

            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected, actual, new TestHelper.ListTuple2EqualityComparer(3));
        }

        [Fact]
        public void _text_bbox()
        {
            Dictionary<string, List<TextLine>> t_bbox = new Dictionary<string, List<TextLine>>()
            {
                {
                    "horizontal",
                    new List<TextLine>()
                    {
                        column_span_2_text[30-13],
                        column_span_2_text[31-13],
                        TestHelper.MakeTextLine(258.149,687.721,299.807,697.221, "Age/Sex/\n"),
                        column_span_2_text[0],
                        column_span_2_text[1],
                        column_span_2_text[2],
                        TestHelper.MakeTextLine(468.603,687.721,525.546,697.221 , "Sample size\n"),
                        column_span_2_text[32-13],
                        TestHelper.MakeTextLine(230.691,676.326,327.658,685.826, "Physiological  Group\n"),
                        column_span_2_text[3],
                        column_span_2_text[4],
                        TestHelper.MakeTextLine(476.143,676.326,518.304,685.826, "per State\n"),
                        column_span_2_text[33-13],
                        column_span_2_text[34-13],
                        column_span_2_text[35-13],
                        TestHelper.MakeTextLine(232.423,632.767,430.014,642.267, "                           All the available individuals\n"),
                        column_span_2_text[36-13],
                        column_span_2_text[37-13],
                        column_span_2_text[38-13],
                        TestHelper.MakeTextLine(232.395,591.267,427.753,600.767, "All the individuals partaking meals in the HH\n"),
                        TestHelper.MakeTextLine(232.300,570.517,293.626,581.803, "Men (> 18yrs)\n"),
                        TestHelper.MakeTextLine(486.454,570.517,508.181,580.017, "1728\n"),
                        column_span_2_text[39-13],
                        column_span_2_text[40-13],
                        column_span_2_text[18-13],
                        column_span_2_text[19-13],
                        column_span_2_text[20-13],
                        TestHelper.MakeTextLine(232.300,549.767,311.103,561.053, "Women (> 18 yrs)\n"),
                        TestHelper.MakeTextLine(486.454,549.767,508.181,559.267, "1728\n"),
                        TestHelper.MakeTextLine(232.300,526.183,296.514,537.469, "Men (> 18 yrs)\n"),
                        TestHelper.MakeTextLine(486.454,526.183,508.181,535.683, "1825\n"),
                        column_span_2_text[41-13],
                        column_span_2_text[42-13],
                        column_span_2_text[21-13],
                        column_span_2_text[22-13],
                        column_span_2_text[23-13],
                        TestHelper.MakeTextLine(232.300,505.433,311.103,516.719, "Women (> 18 yrs)\n"),
                        TestHelper.MakeTextLine(486.454,505.433,508.181,514.933, "1825\n"),
                        column_span_2_text[43-13],
                        column_span_2_text[44-13],
                        TestHelper.MakeTextLine(232.319,484.683,296.513,495.969,"Men (> 18 yrs)\n"),
                        column_span_2_text[24-13],
                        column_span_2_text[25-13],
                        column_span_2_text[26-13],
                        TestHelper.MakeTextLine(486.437,484.683,508.163,494.183, "1728\n"),
                        column_span_2_text[45-13],
                        column_span_2_text[46-13],
                        column_span_2_text[47-13],
                        TestHelper.MakeTextLine(232.289,461.892,311.102,473.178, "Women (> 18 yrs)\n"),
                        column_span_2_text[27-13],
                        column_span_2_text[28-13],
                        column_span_2_text[29-13],
                        TestHelper.MakeTextLine(486.453,461.892,508.180,471.392, "1728\n"),
                        TestHelper.MakeTextLine(84.897,436.324,285.822,445.824, "*CI: Confidence Interval       # Design effect 2\n")
                    }
                },
                {
                    "vertical",
                    new List<TextLine>()
                },
            };

            var (x0, y0, x1, y1) = Stream.TextBbox(t_bbox);
            Assert.Equal(84.89665500000015f, x0, 3);
            Assert.Equal(436.32394999999985f, y0, 3);
            Assert.Equal(525.5457000000002f, x1, 3);
            Assert.Equal(697.2215f, y1, 3);
        }

#if DEBUG
        [Fact]
        public void _generate_columns_and_rows()
        {
            Stream stream = new Stream(column_span_2_horizontal_text, new List<TextLine>());
            var (cols, rows) = stream.GenerateColumnsAndRows(0, (74.89665500000015f, 426.32394999999985f, 518.1806500000001f, 714.2693141025641f));

            var cols_expected = new List<(float, float)>()
            {
                (84.896655f, 323.623675f),
                (323.623675f, 372.394775f),
                (372.394775f, 406.805675f),
                (406.805675f, 470.0328775f),
                (470.0328775f, 525.5457f)
            };

            var rows_expected = new List<(float, float)>()
            {
                (697.221f, 686.7735f),
                (686.7735f, 669.6715f),
                (669.6715f, 647.8918f),
                (647.8918f, 627.1419f),
                (627.1419f, 606.392f),
                (606.392f, 585.8652f),
                (585.8652f, 569.821275f),
                (569.821275f, 559.446325f),
                (559.446325f, 543.1715f),
                (543.1715f, 525.486675f),
                (525.486675f, 515.111755f),
                (515.111755f, 500.0947357f),
                (500.0947357f, 483.7989f),
                (483.7989f, 472.40326f),
                (472.40326f, 453.9218857f),
                (453.9218857f, 436.32395f)
            };

            Assert.Equal(cols_expected.Count, cols.Count);
            Assert.Equal(rows_expected.Count, rows.Count);

            Assert.Equal(cols_expected, cols, new TestHelper.ListTuple2EqualityComparer(3));
            Assert.Equal(rows_expected, rows, new TestHelper.ListTuple2EqualityComparer(3));
        }
#endif

#if DEBUG
        [Fact]
        public void _generate_table()
        {
            Stream stream = new Stream(column_span_2_horizontal_text, new List<TextLine>());
            (var cols, var rows) = stream.GenerateColumnsAndRows(0, (74.89665500000015f, 426.32394999999985f, 518.1806500000001f, 714.2693141025641f));
            var actual = stream.GenerateTable(0, cols, rows);

            Assert.Equal((16, 5), actual.Shape);
            Assert.Equal(78, actual._text.Count);
            Assert.Equal(46.25, actual.Whitespace);
            Assert.Equal(97.09215, actual.Accuracy, 4);
            Assert.Equal("stream", actual.Flavor);
            Assert.Equal(80, actual.Cells.SelectMany(r => r.Select(c => c)).Count());
        }
#endif

        [Fact]
        public void extract_tables()
        {
            Stream stream = new Stream();
            var tables = stream.ExtractTables(@"Files\foo.pdf");

            Assert.Single(tables);
            Assert.Equal((612, 792), stream.Dimensions);
            Assert.Equal(612, stream.PdfWidth);
            Assert.Equal(792, stream.PdfHeight);
            Assert.Equal(84, stream.HorizontalText.Count);
        }
    }
}
