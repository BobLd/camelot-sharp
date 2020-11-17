using Camelot.ImageProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Logging;
using static Camelot.Core;

namespace Camelot.Parsers
{
    // https://github.com/camelot-dev/camelot/blob/master/camelot/parsers/lattice.py

    /// <summary>
    /// Lattice method of parsing looks for lines between text to parse the table.
    /// </summary>
    public class Lattice : BaseParser
    {
        /// <summary>
        /// List of page regions that may contain tables of the form x1,y1,x2,y2 where(x1, y1) -> left-top and(x2, y2) -> right-bottom in PDF coordinate space.
        /// </summary>
        public List<(float x1, float y1, float x2, float y2)> TableRegions { get; }

        /// <summary>
        /// List of table area strings of the form x1,y1,x2,y2 where(x1, y1) -> left-top and(x2, y2) -> right-bottom in PDF coordinate space.
        /// </summary>
        public List<(float x1, float y1, float x2, float y2)> TableAreas { get; }

        /// <summary>
        /// Process background lines.
        /// </summary>
        public bool ProcessBackground { get; }

        /// <summary>
        /// Line size scaling factor. The larger the value the smaller the detected lines. Making it very large will lead to text being detected as lines.
        /// </summary>
        public int LineScale { get; }

        /// <summary>
        /// Direction in which text in a spanning cell will be copied over.
        /// <para>{'h', 'v'}</para>
        /// </summary>
        public List<string> CopyText { get; }

        /// <summary>
        /// Direction in which text in a spanning cell will flow.
        /// <para>{'l', 'r', 't', 'b'}</para>
        /// </summary>
        public string[] ShiftText { get; }

        /// <summary>
        /// Split text that spans across multiple cells.
        /// </summary>
        public bool SplitText { get; }

        /// <summary>
        /// Flag text based on font size. Useful to detect super/subscripts. Adds &lt;s&gt;&lt;/s&gt; around flagged text.
        /// </summary>
        public bool FlagSize { get; }

        /// <summary>
        /// Characters that should be stripped from a string before assigning it to a cell.
        /// </summary>
        public string StripText { get; }

        /// <summary>
        /// Tolerance parameter used to merge close vertical and horizontal lines.
        /// </summary>
        public int LineTol { get; }

        /// <summary>
        /// Tolerance parameter used to decide whether the detected lines and points lie close to each other.
        /// </summary>
        public int JointTol { get; }

        /// <summary>
        /// Size of a pixel neighborhood that is used to calculate a threshold value for the pixel: 3, 5, 7, and so on.
        /// </summary>
        public int ThresholdBlocksize { get; }

        /// <summary>
        /// Constant subtracted from the mean or weighted mean. Normally, it is positive but may be zero or negative as well.
        /// </summary>
        public int ThresholdConstant { get; }

        /// <summary>
        /// Number of times for erosion/dilation is applied.
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Resolution used for PDF to PNG conversion.
        /// </summary>
        public int Resolution { get; }

        /// <summary>
        /// Image Processer.
        /// </summary>
        public IImageProcesser ImageProcesser { get; }

        /// <summary>
        /// Drawing Processor.
        /// </summary>
        public IDrawingProcessor DrawingProcessor { get; }

        private Dictionary<string, List<TextLine>> tBbox;
        private List<(float, float, float, float)> verticalSegments;
        private List<(float, float, float, float)> horizontalSegments;
        private Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> tableBbox;

        /// <summary>
        /// Lattice method of parsing looks for lines between text to parse the table.
        /// </summary>
        /// <param name="imageProcesser"></param>
        /// <param name="drawingProcessor"></param>
        /// <param name="table_regions">List of page regions that may contain tables of the form x1,y1,x2,y2 where(x1, y1) -> left-top and(x2, y2) -> right-bottom in PDF coordinate space.</param>
        /// <param name="table_areas">List of table area strings of the form x1,y1,x2,y2 where(x1, y1) -> left-top and(x2, y2) -> right-bottom in PDF coordinate space.</param>
        /// <param name="process_background">Process background lines.</param>
        /// <param name="line_scale">Line size scaling factor. The larger the value the smaller the detected lines. Making it very large will lead to text being detected as lines.</param>
        /// <param name="copy_text">{'h', 'v'} Direction in which text in a spanning cell will be copied over.</param>
        /// <param name="shift_text">{'l', 'r', 't', 'b'} Direction in which text in a spanning cell will flow. (default: ['l', 't'])</param>
        /// <param name="split_text">Split text that spans across multiple cells.</param>
        /// <param name="flag_size">Flag text based on font size. Useful to detect super/subscripts. Adds &lt;s&gt;&lt;/s&gt; around flagged text.</param>
        /// <param name="strip_text">Characters that should be stripped from a string before assigning it to a cell.</param>
        /// <param name="line_tol">Tolerance parameter used to merge close vertical and horizontal lines.</param>
        /// <param name="joint_tol">Tolerance parameter used to decide whether the detected lines and points lie close to each other.</param>
        /// <param name="threshold_blocksize">Size of a pixel neighborhood that is used to calculate a threshold value for the pixel: 3, 5, 7, and so on.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <param name="threshold_constant">Constant subtracted from the mean or weighted mean. Normally, it is positive but may be zero or negative as well.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <param name="iterations">Number of times for erosion/dilation is applied.
        /// <para>For more information, refer `OpenCV's dilate https://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html#dilate`.</para></param>
        /// <param name="resolution">Resolution used for PDF to PNG conversion.</param>
        /// <param name="log"></param>
        /// <param name="kwargs"></param>
        public Lattice(IImageProcesser imageProcesser,
                       IDrawingProcessor drawingProcessor,
                       List<(float x1, float y1, float x2, float y2)> table_regions = null,
                       List<(float x1, float y1, float x2, float y2)> table_areas = null,
                       bool process_background = false,
                       int line_scale = 15,
                       List<string> copy_text = null,
                       string[] shift_text = null,
                       bool split_text = false,
                       bool flag_size = false,
                       string strip_text = "",
                       int line_tol = 2,
                       int joint_tol = 2,
                       int threshold_blocksize = 15,
                       int threshold_constant = -2,
                       int iterations = 0,
                       int resolution = 300,
                       ILog log = null,
                       params string[] kwargs) : base(log)
        {
            ImageProcesser = imageProcesser;
            DrawingProcessor = drawingProcessor;

            TableRegions = table_regions;
            TableAreas = table_areas;
            ProcessBackground = process_background;
            LineScale = line_scale;
            CopyText = copy_text;
            ShiftText = shift_text ?? (new[] { "l", "t" });
            SplitText = split_text;
            FlagSize = flag_size;
            StripText = strip_text;
            LineTol = line_tol;
            JointTol = joint_tol;
            ThresholdBlocksize = threshold_blocksize;
            ThresholdConstant = threshold_constant;
            Iterations = iterations;
            Resolution = resolution;
        }

        /// <summary>
        /// Reduces index of a text object if it lies within a spanning cell.
        /// </summary>
        /// <param name="t">table : camelot.core.Table</param>
        /// <param name="idx">list - List of tuples of the form(r_idx, c_idx, text).</param>
        /// <param name="shift_text">list - {'l', 'r', 't', 'b'}
        /// Select one or more strings from above and pass them as a list to specify where the text in a spanning cell should flow.</param>
        /// <returns>List of tuples of the form (r_idx, c_idx, text) where r_idx and c_idx are new row and column indices for text.</returns>
        private static List<(int r_idx, int c_idx, string text)> ReduceIndex(Table t, List<(int r_idx, int c_idx, string text)> idx, string[] shift_text)
        {
            List<(int r_idx, int c_idx, string text)> indices = new List<(int r_idx, int c_idx, string text)>();

            foreach ((int r_idx, int c_idx, string text) in idx)
            {
                int c_idx_local = c_idx;
                int r_idx_local = r_idx;

                foreach (var d in shift_text)
                {
                    switch (d)
                    {
                        case "l":
                            if (t.Cells[r_idx][c_idx].HSpan)
                            {
                                while (!t.Cells[r_idx][c_idx_local].Left)
                                {
                                    c_idx_local--;
                                }
                            }
                            break;

                        case "r":
                            if (t.Cells[r_idx][c_idx].HSpan)
                            {
                                while (!t.Cells[r_idx][c_idx_local].Right)
                                {
                                    c_idx_local++;
                                }
                            }
                            break;

                        case "t":
                            if (t.Cells[r_idx][c_idx].VSpan)
                            {
                                while (!t.Cells[r_idx_local][c_idx].Top)
                                {
                                    r_idx_local--;
                                }
                            }
                            break;

                        case "b":
                            if (t.Cells[r_idx][c_idx].VSpan)
                            {
                                while (!t.Cells[r_idx_local][c_idx].Bottom)
                                {
                                    r_idx_local++;
                                }
                            }
                            break;

                        case "":
                            break;

                        default:
                            throw new ArgumentException($"Unknown value '{d}'. Valid values are 'l', 'r', 't' and 'b'.", nameof(shift_text));
                    }
                }

                indices.Add((r_idx_local, c_idx_local, text));
            }
            return indices;
        }

        /// <summary>
        /// Copies over text in empty spanning cells.
        /// </summary>
        /// <param name="t">camelot.core.Table</param>
        /// <param name="copy_text">{'h', 'v'}
        /// Select one or more strings from above and pass them as a list
        /// to specify the direction in which text should be copied over
        /// when a cell spans multiple rows or columns.</param>
        /// <returns>camelot.core.Table</returns>
        private static Table CopySpanningText(Table t, List<string> copy_text = null)
        {
            foreach (var f in copy_text)
            {
                if (f == "h")
                {
                    for (int i = 0; i < t.Cells.Count; i++)
                    {
                        for (int j = 0; j < t.Cells[i].Count; j++)
                        {
                            if (t.Cells[i][j].Text.Trim()?.Length == 0)
                            {
                                if (t.Cells[i][j].HSpan && !t.Cells[i][j].Left)
                                {
                                    t.Cells[i][j].Text = t.Cells[i][j - 1].Text;
                                }
                            }
                        }
                    }
                }
                else if (f == "v")
                {
                    for (int i = 0; i < t.Cells.Count; i++)
                    {
                        for (int j = 0; j < t.Cells[i].Count; j++)
                        {
                            if (t.Cells[i][j].Text.Trim()?.Length == 0)
                            {
                                if (t.Cells[i][j].VSpan && !t.Cells[i][j].Top)
                                {
                                    t.Cells[i][j].Text = t.Cells[i - 1][j].Text;
                                }
                            }
                        }
                    }
                }
            }

            return t;
        }

        private void GenerateTableBbox()
        {
            (this.tableBbox, this.verticalSegments, this.horizontalSegments) = ImageProcesser.Process(
                this.Layout,
                this.DrawingProcessor,
                this.ProcessBackground,
                this.ThresholdBlocksize,
                this.ThresholdConstant,
                this.LineScale,
                this.Iterations,
                this.TableAreas,
                this.TableRegions);
        }

        public (List<(float, float)> cols, List<(float, float)> rows, List<(float, float, float, float)> v_s, List<(float, float, float, float)> h_s) GenerateColumnsAndRows(int table_idx, (float, float, float, float) tk)
        {
            // select elements which lie within table_bbox
            Dictionary<string, List<TextLine>> t_bbox = new Dictionary<string, List<TextLine>>();
            (var v_s, var h_s) = Utils.SegmentsInBbox(tk, this.verticalSegments, this.horizontalSegments);
            t_bbox["horizontal"] = Utils.TextInBbox(tk, this.HorizontalText);
            t_bbox["vertical"] = Utils.TextInBbox(tk, this.VerticalText);

            t_bbox["horizontal"] = t_bbox["horizontal"].OrderBy(x => -x.Y0()).ThenBy(x => x.X0()).ToList();
            t_bbox["vertical"] = t_bbox["vertical"].OrderBy(x => x.X0()).ThenBy(x => -x.Y0()).ToList();

            this.tBbox = t_bbox;
            var cols = this.tableBbox[tk].ConvertAll(x => x.Item1);
            var rows = this.tableBbox[tk].ConvertAll(x => x.Item2);

            cols.AddRange(new[] { tk.Item1, tk.Item3 });
            rows.AddRange(new[] { tk.Item2, tk.Item4 });

            // sort horizontal and vertical segments
            cols = Utils.MergeCloseLines(cols.OrderBy(r => r), line_tol: this.LineTol);
            rows = Utils.MergeCloseLines(rows.OrderByDescending(c => c), line_tol: this.LineTol);

            // make grid using x and y coord of shortlisted rows and cols
            var colsT = Enumerable.Range(0, cols.Count - 1).Select(i => (cols[i], cols[i + 1])).ToList();
            var rowsT = Enumerable.Range(0, rows.Count - 1).Select(i => (rows[i], rows[i + 1])).ToList();
            return (colsT, rowsT, v_s, h_s);
        }

        /// <summary>
        /// TODO: BobLD - have a single function for stream and lattice
        /// </summary>
        public Table GenerateTable(int table_idx, List<(float, float)> cols, List<(float, float)> rows,
            List<(float, float, float, float)> v_s = null, List<(float, float, float, float)> h_s = null)
        {
            if (v_s == null || h_s == null)
            {
                throw new ArgumentNullException($"No segments found on {this.RootName}");
            }

            var table = new Table(cols, rows);
            // set table edges to True using ver+hor lines
            table = table.SetEdges(v_s, h_s, joint_tol: this.JointTol);
            // set table border edges to True
            table = table.SetBorder();
            // set spanning cells to True
            table = table.SetSpan();

            var pos_errors = new List<float>();
            // TODO: have a single list in place of two directional ones?
            // sorted on x-coordinate based on reading order i.e. LTR or RTL

            foreach (string direction in new[] { "vertical", "horizontal" })
            {
                foreach (var t in this.tBbox[direction])
                {
                    (var indices, var error) = Utils.GetTableIndex(table,
                        t,
                        direction,
                        split_text: this.SplitText,
                        flag_size: this.FlagSize,
                        strip_text: this.StripText,
                        log: log);

                    // CAREFUL HERE - Not sure the Python version is correct
                    // indices[:2] != (-1, -1) should always return true 
                    // as indices is [[]] and indices[:2] is 
                    // [[x, y, text0], [x', y', text1]]
                    // SOLUTION: We take the first element to do the check
                    if (indices[0].r_idx != -1 && indices[0].c_idx != -1) // if indices[:2] != (-1, -1):
                    {
                        pos_errors.Add(error);
                        indices = Lattice.ReduceIndex(table, indices, shift_text: this.ShiftText);
                        foreach ((var r_idx, var c_idx, var text) in indices)
                        {
                            table.Cells[r_idx][c_idx].Text = text + "\n";
                        }
                    }
                }
            }
            var accuracy = Utils.ComputeAccuracy(new[] { (100f, (IReadOnlyList<float>)pos_errors) });

            if (this.CopyText != null)
            {
                table = CopySpanningText(table, copy_text: this.CopyText);
            }

            var data = table.Data();
            //table.df = pd.DataFrame(data);
            table.Shape = (data.Count, data.Max(r => r.Count));

            var whitespace = Utils.ComputeWhitespace(data);
            table.Flavor = "lattice";
            table.Accuracy = accuracy;
            table.Whitespace = whitespace;
            table.Order = table_idx + 1;
            table.Page = -99; //int(os.path.basename(self.rootname).replace("page-", ""));

            // for plotting
            var _text = new List<(float x0, float y0, float x1, float y1)>();
            _text.AddRange(this.HorizontalText.Select(t => (t.X0(), t.Y0(), t.X1(), t.Y1())));
            _text.AddRange(this.VerticalText.Select(t => (t.X0(), t.Y0(), t.X1(), t.Y1())));
            table.Text = _text;
            table.Segments = (this.verticalSegments, this.horizontalSegments);
            table.Textedges = null;

            return table;
        }

        public override List<Table> ExtractTables(string filename, bool suppress_stdout = false, params DlaOptions[] layout_kwargs)
        {
            base.GenerateLayout(filename, layout_kwargs);
            var base_filename = Path.GetFileName(this.RootName); //os.path.basename(self.rootname)
            if (!suppress_stdout)
            {
                log?.Debug($"Processing {base_filename}");
            }

            if (base.HorizontalText == null || base.HorizontalText.Count == 0)
            {
                if (base.Images.Count > 0)
                {
                    log?.Warn($"{base_filename} is image-based, camelot only works on text-based pages.");
                }
                else
                {
                    log?.Warn($"No tables found on {base_filename}");
                }
                return null;
            }

            this.GenerateTableBbox();

            var _tables = new List<Table>();
            // sort tables based on y-coord
            int table_idx = 0;
            foreach (var tk in this.tableBbox.Keys.OrderByDescending(kvp => kvp.y1))
            {
                (var cols, var rows, var v_s, var h_s) = this.GenerateColumnsAndRows(table_idx, tk);
                var table = this.GenerateTable(table_idx, cols, rows, v_s: v_s, h_s: h_s);
                table.Bbox = tk;
                _tables.Add(table);
                table_idx++;
            }

            return _tables;
        }
    }
}
