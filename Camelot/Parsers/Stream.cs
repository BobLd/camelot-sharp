using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Logging;
using static Camelot.Core;

namespace Camelot.Parsers
{
    //https://github.com/camelot-dev/camelot/blob/master/camelot/parsers/stream.py

    /// <summary>
    /// Stream method of parsing looks for spaces between text to parse the table.
    /// </summary>
    public class Stream : BaseParser
    {
        private readonly List<(float x1, float y1, float x2, float y2)> table_regions;
        private readonly List<(float x1, float y1, float x2, float y2)> table_areas;
        private readonly List<string> columns;
        private readonly bool split_text;
        private readonly bool flag_size;
        private readonly string strip_text;
        private readonly int edge_tol;
        private readonly int row_tol;
        private readonly int column_tol;

#if DEBUG
        [Obsolete("For debugging purpose only.")]
        public Stream(IReadOnlyList<TextLine> horizontal_text,
                      IReadOnlyList<TextLine> vertical_text,
                      bool split_text = false,
                      bool flag_size = false,
                      string strip_text = "",
                      int edge_tol = 50,
                      int row_tol = 2,
                      int column_tol = 0) : base(null)
        {
            this.horizontal_text = horizontal_text.ToList();
            this.vertical_text = vertical_text.ToList();
            this.split_text = split_text;
            this.flag_size = flag_size;
            this.strip_text = strip_text;
            this.edge_tol = edge_tol;
            this.row_tol = row_tol;
            this.column_tol = column_tol;
        }
#endif

        /// <summary>
        /// Stream method of parsing looks for spaces between text to parse the table.
        /// <para>If you want to specify columns when specifying multiple table
        /// areas, make sure that the length of both lists are equal.</para>
        /// </summary>
        /// <param name="table_regions">List of page regions that may contain tables of the form x1,y1,x2,y2
        /// where(x1, y1) -> left-top and(x2, y2) -> right-bottom
        /// in PDF coordinate space.</param>
        /// <param name="table_areas">List of table area strings of the form x1,y1,x2,y2
        /// where(x1, y1) -> left-top and(x2, y2) -> right-bottom
        /// in PDF coordinate space.</param>
        /// <param name="columns">List of column x-coordinates strings where the coordinates
        /// are comma-separated.</param>
        /// <param name="split_text">Split text that spans across multiple cells.</param>
        /// <param name="flag_size">Flag text based on font size. Useful to detect
        /// super/subscripts. Adds &lt;s&gt;&lt;/s&gt; around flagged text.</param>
        /// <param name="strip_text">Characters that should be stripped from a string before
        /// assigning it to a cell.</param>
        /// <param name="edge_tol">Tolerance parameter for extending textedges vertically.</param>
        /// <param name="row_tol">Tolerance parameter used to combine text vertically, to generate rows.</param>
        /// <param name="column_tol">Tolerance parameter used to combine text horizontally, to generate columns.</param>
        /// <param name="log"></param>
        public Stream(List<(float x1, float y1, float x2, float y2)> table_regions = null,
                      List<(float x1, float y1, float x2, float y2)> table_areas = null,
                      List<string> columns = null,
                      bool split_text = false,
                      bool flag_size = false,
                      string strip_text = "",
                      int edge_tol = 50,
                      int row_tol = 2,
                      int column_tol = 0,
                      ILog log = null) : base(log)
        {
            this.table_regions = table_regions;
            this.table_areas = table_areas;
            this.columns = columns;
            this._validate_columns();
            this.split_text = split_text;
            this.flag_size = flag_size;
            this.strip_text = strip_text;
            this.edge_tol = edge_tol;
            this.row_tol = row_tol;
            this.column_tol = column_tol;
        }

        /// <summary>
        /// Returns bounding box for the text present on a page.
        /// </summary>
        /// <param name="t_bbox">Dict with two keys 'horizontal' and 'vertical' with lists of
        /// LTTextLineHorizontals and LTTextLineVerticals respectively.</param>
        /// <returns>Tuple (x0, y0, x1, y1) in pdf coordinate space.</returns>
        public static (float x0, float y0, float x1, float y1) _text_bbox(Dictionary<string, List<TextLine>> t_bbox)
        {
            var xmin = t_bbox.SelectMany(dir => dir.Value.Select(t => t.X0())).Min();
            var ymin = t_bbox.SelectMany(dir => dir.Value.Select(t => t.Y0())).Min();
            var xmax = t_bbox.SelectMany(dir => dir.Value.Select(t => t.X1())).Max();
            var ymax = t_bbox.SelectMany(dir => dir.Value.Select(t => t.Y1())).Max();
            return (xmin, ymin, xmax, ymax);
        }

        /// <summary>
        /// Groups PDFMiner text objects into rows vertically
        /// within a tolerance.
        /// </summary>
        /// <param name="text">List of PDFMiner text objects.</param>
        /// <param name="row_tol">int, optional (default: 2)</param>
        /// <returns>rows : list - Two-dimensional list of text objects grouped into rows.</returns>
        public static IReadOnlyList<IReadOnlyList<TextLine>> _group_rows(IReadOnlyList<TextLine> text, int row_tol = 2)
        {
            float row_y = 0;
            var rows = new List<List<TextLine>>();
            var temp = new List<TextLine>();

            foreach (var t in text)
            {
                // is checking for upright necessary?
                // if t.get_text().strip() and all([obj.upright for obj in t._objs if
                // type(obj) is LTChar]):
                if (!string.IsNullOrEmpty(t.Text?.Trim()))
                {
                    if (!MathExtensions.AlmostEquals(row_y, t.Y0(), row_tol))
                    {
                        rows.Add(temp.OrderBy(_t => _t.X0()).ToList());
                        temp = new List<TextLine>();
                        row_y = t.Y0();
                    }
                    temp.Add(t);
                }
            }

            rows.Add(temp.OrderBy(t => t.X0()).ToList());

            if (rows.Count > 1)
            {
                rows.RemoveAt(0); // TODO: hacky
            }

            return rows;
        }

        /// <summary>
        /// Merges column boundaries horizontally if they overlap
        /// or lie within a tolerance.
        /// </summary>
        /// <param name="l">List of column x-coordinate tuples.</param>
        /// <param name="column_tol">int, optional (default: 0)</param>
        /// <returns>merged : list - List of merged column x-coordinate tuples.</returns>
        private static List<(float, float)> _merge_columns(IEnumerable<(float, float)> l, int column_tol = 0)
        {
            var merged = new List<(float, float)>();
            foreach (var higher in l)
            {
                if (merged.Count == 0)
                {
                    merged.Add(higher);
                }
                else
                {
                    var lower = merged.Last();
                    if (column_tol >= 0)
                    {
                        if (higher.Item1 <= lower.Item2 || MathExtensions.AlmostEquals(higher.Item1, lower.Item2, column_tol))
                        {
                            var upper_bound = Math.Max(lower.Item2, higher.Item2);
                            var lower_bound = Math.Min(lower.Item1, higher.Item1);
#pragma warning disable IDE0056 // Use index operator
                            merged[merged.Count - 1] = (lower_bound, upper_bound);
#pragma warning restore IDE0056 // Use index operator
                        }
                        else
                        {
                            merged.Add(higher);
                        }
                    }
                    else if (column_tol < 0)
                    {
                        if (higher.Item1 <= lower.Item2)
                        {
                            if (MathExtensions.AlmostEquals(higher.Item1, lower.Item2, Math.Abs(column_tol)))
                            {
                                merged.Add(higher);
                            }
                            else
                            {
                                var upper_bound = Math.Max(lower.Item2, higher.Item2);
                                var lower_bound = Math.Min(lower.Item1, higher.Item1);
#pragma warning disable IDE0056 // Use index operator
                                merged[merged.Count - 1] = (lower_bound, upper_bound);
#pragma warning restore IDE0056 // Use index operator
                            }
                        }
                        else
                        {
                            merged.Add(higher);
                        }
                    }
                }
            }
            return merged;
        }

        /// <summary>
        /// Makes row coordinates continuous.
        /// </summary>
        /// <param name="rows_grouped">Two-dimensional list of text objects grouped into rows.</param>
        /// <param name="text_y_max"></param>
        /// <param name="text_y_min"></param>
        /// <returns>rows : list - List of continuous row y-coordinate tuples.</returns>
        public static List<(float, float)> _join_rows(IReadOnlyList<IReadOnlyList<TextLine>> rows_grouped, float text_y_max, float text_y_min)
        {
            var row_mids = rows_grouped.Select(r => r.Count > 0 ? r.Sum(t => (t.Y0() + t.Y1()) / 2f) / r.Count : 0).ToList();
            var rows = Enumerable.Range(1, row_mids.Count - 1).Select(i => (row_mids[i] + row_mids[i - 1]) / 2f).ToList();
            rows.Insert(0, text_y_max);
            rows.Add(text_y_min);
            return Enumerable.Range(0, rows.Count - 1).Select(i => (rows[i], rows[i + 1])).ToList();
        }

        /// <summary>
        /// Adds columns to existing list by taking into account
        /// the text that lies outside the current column x-coordinates.
        /// </summary>
        /// <param name="cols">List of column x-coordinate tuples.</param>
        /// <param name="text">List of PDFMiner text objects.</param>
        /// <param name="row_tol"></param>
        /// <returns>cols : list - Updated list of column x-coordinate tuples.</returns>
        public static List<(float, float)> _add_columns(List<(float, float)> cols, IReadOnlyList<TextLine> text, int row_tol)
        {
            if (text?.Count > 0)
            {
                var new_text = _group_rows(text, row_tol).ToList();
                var elementsMax = new_text.Max(r => r.Count);
                var new_cols = new_text.Where(r => r.Count == elementsMax).SelectMany(r => r.Select(t => (t.X0(), t.X1())));
                cols.AddRange(_merge_columns(new_cols.OrderBy(x => x)));
            }
            return cols;
        }

        /// <summary>
        /// Makes column coordinates continuous.
        /// </summary>
        /// <param name="cols">List of column x-coordinate tuples.</param>
        /// <param name="text_x_min"></param>
        /// <param name="text_x_max"></param>
        /// <returns>cols : list - Updated list of column x-coordinate tuples.</returns>
        private static List<(float, float)> _join_columns(List<(float, float)> cols, float text_x_min, float text_x_max)
        {
            cols.Sort();
            var col = Enumerable.Range(1, cols.Count - 1).Select(i => (cols[i].Item1 + cols[i - 1].Item2) / 2f).ToList();
            col.Insert(0, text_x_min);
            col.Add(text_x_max);
            return Enumerable.Range(0, col.Count - 1).Select(i => (col[i], col[i + 1])).ToList();
        }

        private void _validate_columns()
        {
            if (this.table_areas != null && this.columns != null)
            {
                if (table_areas.Count != this.columns.Count)
                {
                    throw new ArgumentException($"Length of {nameof(table_areas)} and {nameof(columns)} should be equal");
                }
            }
        }

        /// <summary>
        /// A general implementation of the table detection algorithm
        /// described by Anssi Nurminen's master's thesis.
        /// <para>Link: https://dspace.cc.tut.fi/dpub/bitstream/handle/123456789/21520/Nurminen.pdf?sequence=3</para>
        /// Assumes that tables are situated relatively far apart vertically.
        /// </summary>
        /// <param name="textlines"></param>
        /// <returns></returns>
        private Dictionary<(float, float, float, float), object> _nurminen_table_detection(List<TextLine> textlines)
        {
            // TODO: add support for arabic text #141
            // sort textlines in reading order
            textlines = textlines.OrderBy(x => -x.Y0()).ThenBy(x => x.X0()).ToList();
            var textedges = new TextEdges(this.edge_tol);
            // generate left, middle and right textedges
            textedges.Generate(textlines);
            // select relevant edges
            var relevant_textedges = textedges.GetRelevant();
            this.textedges.AddRange(relevant_textedges); // extend
            // guess table areas using textlines and relevant edges
            var table_bbox = textedges.GetTableAreas(textlines, relevant_textedges);
            // treat whole page as table area if no table areas found
            if (table_bbox.Count == 0)
            {
                table_bbox = new Dictionary<(float, float, float, float), object>()
                {
                    {(0, 0, this.pdf_width, this.pdf_height), null }
                };
            }

            return table_bbox;
        }

        private List<TextEdge> textedges;
        private Dictionary<(float, float, float, float), object> table_bbox;

        private void _generate_table_bbox()
        {
            this.textedges = new List<TextEdge>();
            Dictionary<(float, float, float, float), object> table_bbox;
            if (this.table_areas == null || this.table_areas.Count == 0)
            {
                var hor_text = this.horizontal_text;
                if (this.table_regions?.Count > 0)
                {
                    // filter horizontal text
                    hor_text = new List<TextLine>();
                    foreach (var region in this.table_regions)
                    {
                        var region_text = Utils.TextInBbox(region, this.horizontal_text);
                        hor_text.AddRange(region_text);
                    }
                }
                // find tables based on nurminen's detection algorithm
                table_bbox = this._nurminen_table_detection(hor_text);
            }
            else
            {
                table_bbox = new Dictionary<(float, float, float, float), object>();
                foreach (var area in this.table_areas)
                {
                    table_bbox[area] = null;
                }
            }
            this.table_bbox = table_bbox;
        }

        public (List<(float, float)> cols, List<(float, float)> rows) _generate_columns_and_rows(int table_idx, (float, float, float, float) tk)
        {
            // select elements which lie within table_bbox            
            this.t_bbox = new Dictionary<string, List<TextLine>>()
            {
                {
                    "horizontal",
                    Utils.TextInBbox(tk, this.horizontal_text).OrderBy(x => -x.Y0()).ThenBy(x => x.X0()).ToList()
                },
                {
                    "vertical",
                    Utils.TextInBbox(tk, this.vertical_text).OrderBy(x => x.X0()).ThenBy(x => -x.Y0()).ToList()
                }
            };

            (float text_x_min, float text_y_min, float text_x_max, float text_y_max) = _text_bbox(this.t_bbox);
            var rows_grouped = _group_rows(this.t_bbox["horizontal"], this.row_tol);
            var rows = _join_rows(rows_grouped, text_y_max, text_y_min);
            var elements = rows_grouped.Select(r => r.Count).ToList();

            List<(float, float)> cols;
            if (this.columns?.Count > 0 && columns[table_idx] != "")
            {
                // user has to input boundary columns too
                // take (0, pdf_width) by default
                // similar to else condition
                // len can't be 1
                var cols_temp = this.columns[table_idx].Split(',').Select(c => float.Parse(c)).ToList();
                cols_temp.Insert(0, text_x_min);
                cols_temp.Add(text_x_max);
                cols = Enumerable.Range(0, cols_temp.Count - 1).Select(i => (cols_temp[i], cols_temp[i + 1])).ToList();
            }
            else
            {
                // calculate mode of the list of number of elements in
                // each row to guess the number of columns
                if (elements.Count == 0)
                {
                    cols = new List<(float, float)>() { (text_x_min, text_x_max) };
                }
                else
                {
                    // ncols = max(set(elements), key = elements.count)
                    int ncols = elements.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Key; // get mode
                    if (ncols == 1)
                    {
                        // if mode is 1, the page usually contains not tables
                        // but there can be cases where the list can be skewed,
                        // try to remove all 1s from list in this case and
                        // see if the list contains elements, if yes, then use
                        // the mode after removing 1s
                        elements = elements.Where(x => x != 1).ToList();
                        if (elements.Count > 0)
                        {
                            ncols = elements.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Key; // get mode
                        }
                        else
                        {
                            log?.Debug($"No tables found in table area {table_idx + 1}");
                        }
                    }

                    cols = rows_grouped.Where(r => r.Count == ncols).SelectMany(r => r.Select(t => (t.X0(), t.X1()))).ToList();
                    cols = _merge_columns(cols.OrderBy(c => c), column_tol);

                    var inner_text = new List<TextLine>();
                    for (int i = 1; i < cols.Count; i++)
                    {
                        var left = cols[i - 1].Item2;
                        var right = cols[i].Item1;
                        inner_text.AddRange(this.t_bbox.SelectMany(dir => dir.Value.Where(t => t.X0() > left && t.X1() < right)).ToList());
                    }

                    var outer_text = this.t_bbox.SelectMany(dir => dir.Value.Where(t => t.X0() > cols.Last().Item2 || t.X1() < cols[0].Item1)).ToList();
                    inner_text.AddRange(outer_text);
                    cols = _add_columns(cols, inner_text, this.row_tol);
                    cols = _join_columns(cols, text_x_min, text_x_max);
                }
            }

            return (cols, rows);
        }

        private Dictionary<string, List<TextLine>> t_bbox;

        /// <summary>
        /// TODO: BobLD - have a single function for stream and lattice
        /// </summary>
        public Table _generate_table(int table_idx, List<(float, float)> cols, List<(float, float)> rows, params string[] kwargs)
        {
            Table table = new Table(cols, rows);
            table.SetAllEdges();

            var pos_errors = new List<float>();
            // TODO: have a single list in place of two directional ones?
            // sorted on x-coordinate based on reading order i.e. LTR or RTL
            foreach (var direction in new[] { "vertical", "horizontal" })
            {
                foreach (var t in this.t_bbox[direction])
                {
                    (var indices, var error) = Utils.GetTableIndex(
                        table,
                        t,
                        direction,
                        split_text: this.split_text,
                        flag_size: this.flag_size,
                        strip_text: this.strip_text,
                        log: log);

                    //if indices[:2] != (-1, -1):
                    //    pos_errors.append(error)
                    //    for r_idx, c_idx, text in indices:
                    //        table.cells[r_idx][c_idx].text = text

                    // CAREFUL HERE - Not sure the Python version is correct
                    // indices[:2] != (-1, -1) should always return true 
                    // as indices is [[]] and indices[:2] is 
                    // [[x, y, text0], [x', y', text1]]
                    // SOLUTION: We take the first element to do the check
                    if (indices[0].r_idx != -1 && indices[0].c_idx != -1)
                    {
                        pos_errors.Add(error);
                        foreach ((var r_idx, var c_idx, var text) in indices)
                        {
                            table.Cells[r_idx][c_idx].Text = text + "\n";
                        }
                    }
                }
            }
            var accuracy = Utils.ComputeAccuracy(new[] { (100f, (IReadOnlyList<float>)pos_errors) }); //[[100, pos_errors]]);

            var data = table.Data();
            //table.df = pd.DataFrame(data)
            table.Shape = (data.Count, data.Max(r => r.Count)); //table.df.shape;

            var whitespace = Utils.ComputeWhitespace(data);
            table.Flavor = "stream";
            table.Accuracy = accuracy;
            table.Whitespace = whitespace;
            table.Order = table_idx + 1;
            table.Page = -99; //int(os.path.basename(self.rootname).replace("page-", ""));

            // for plotting
            var _text = new List<(float, float, float, float)>();
            _text.AddRange(this.horizontal_text.Select(t => (t.X0(), t.Y0(), t.X1(), t.Y1())));
            _text.AddRange(this.vertical_text.Select(t => (t.X0(), t.Y0(), t.X1(), t.Y1())));
            table._text = _text;
            table.segments = (null, null);
            table.textedges = this.textedges;

            return table;
        }

        public override List<Table> extract_tables(string filename, bool suppress_stdout = false, params DlaOptions[] layout_kwargs)
        {
            this._generate_layout(filename, layout_kwargs);
            var base_filename = Path.GetFileName(this.rootname); //os.path.basename(self.rootname)
            if (!suppress_stdout)
            {
                log?.Debug($"Processing {base_filename}");
            }

            if (this.horizontal_text == null || this.horizontal_text.Count == 0)
            {
                if (this.images?.Count > 0)
                {
                    log?.Warn($"{base_filename} is image-based, camelot only works on text-based pages.");
                }
                else
                {
                    log?.Warn($"No tables found on {base_filename}");
                }
                return new List<Table>();
            }

            this._generate_table_bbox();

            var _tables = new List<Table>();
            // sort tables based on y-coord
            int table_idx = 0;
            foreach (var tk in this.table_bbox.Keys.OrderByDescending(kvp => kvp.Item2))
            {
                (var cols, var rows) = this._generate_columns_and_rows(table_idx, tk);
                var table = this._generate_table(table_idx, cols, rows);
                table._bbox = tk;
                _tables.Add(table);
                table_idx++;
            }

            return _tables;
        }
    }
}
