using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace Camelot
{
    //https://github.com/camelot-dev/camelot/blob/master/camelot/core.py
    public class Core
    {
        /// <summary>
        /// minimum number of vertical textline intersections for a textedge to be considered valid
        /// </summary>
        public const int TEXTEDGE_REQUIRED_ELEMENTS = 4;

        /// <summary>
        /// padding added to table area on the left, right and bottom
        /// </summary>
        public const int TABLE_AREA_PADDING = 10;

        /// <summary>
        /// Defines a text edge coordinates relative to a left-bottom origin. (PDF coordinate space)
        /// </summary>
        public class TextEdge
        {
            /// <summary>
            /// Number of intersections with horizontal text rows.
            /// </summary>
            public int intersections { get; private set; }

            /// <summary>
            /// A text edge is valid if it intersections with at least TEXTEDGE_REQUIRED_ELEMENTS horizontal text rows.
            /// </summary>
            public bool is_valid { get; set; }

            /// <summary>
            /// x-coordinate of the text edge.
            /// </summary>
            public float x { get; internal set; }

            /// <summary>
            /// y-coordinate of bottommost point.
            /// </summary>
            public float y0 { get; internal set; }

            /// <summary>
            /// y-coordinate of topmost point.
            /// </summary>
            public float y1 { get; internal set; }

            /// <summary>
            /// {'left', 'right', 'middle'}
            /// </summary>
            public string align { get; internal set; }

            /// <summary>
            /// Defines a text edge coordinates relative to a left-bottom origin. (PDF coordinate space)
            /// </summary>
            /// <param name="x">x-coordinate of the text edge.</param>
            /// <param name="y0">y-coordinate of bottommost point.</param>
            /// <param name="y1">y-coordinate of topmost point.</param>
            /// <param name="align">optional (default: 'left') {'left', 'right', 'middle'}</param>
            public TextEdge(float x, float y0, float y1, string align = "left")
            {
                this.x = x;
                this.y0 = y0;
                this.y1 = y1;
                this.align = align;
                this.intersections = 0;
                this.is_valid = false;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"<TextEdge x={Math.Round(x, 2)} y0={Math.Round(y0, 2)} y1={Math.Round(y1, 2)} align={align} valid={is_valid}>";
            }

            /// <summary>
            /// Updates the text edge's x and bottom y coordinates and sets the is_valid attribute.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y0"></param>
            /// <param name="edge_tol"></param>
            public void update_coords(float x, float y0, float edge_tol = 50)
            {
                if (MathExtensions.AlmostEquals(this.y0, y0, edge_tol))
                {
                    this.x = ((this.intersections * this.x) + x) / (this.intersections + 1);
                    this.y0 = y0;
                    this.intersections++;
                    // a textedge is valid only if it extends uninterrupted
                    // over a required number of textlines
                    if (this.intersections > TEXTEDGE_REQUIRED_ELEMENTS)
                    {
                        this.is_valid = true;
                    }
                }
            }
        }

        /// <summary>
        /// Defines a dict of left, right and middle text edges found on
        /// the PDF page.The dict has three keys based on the alignments,
        /// and each key's value is a list of camelot.core.TextEdge objects.
        /// </summary>
        public class TextEdges
        {
            public float edge_tol { get; }

            private readonly Dictionary<string, List<TextEdge>> _textedges;
#if DEBUG
            [Obsolete("For debugging purpose only.")]
            public Dictionary<string, List<TextEdge>> TextedgesForTest => _textedges;
#endif

            /// <summary>
            /// Defines a dict of left, right and middle text edges found on
            /// the PDF page.The dict has three keys based on the alignments,
            /// and each key's value is a list of camelot.core.TextEdge objects.
            /// </summary>
            public TextEdges(float edge_tol = 50)
            {
                this.edge_tol = edge_tol;
                _textedges = new Dictionary<string, List<TextEdge>>()
                {
                    { "left" , new List<TextEdge>() },
                    { "right" , new List<TextEdge>() },
                    { "middle" , new List<TextEdge>() },
                };
            }

            /// <summary>
            /// Returns the x coordinate of a text row based on the specified alignment.
            /// </summary>
            /// <param name="textline"></param>
            /// <param name="align"></param>
            /// <returns>Returns the x coordinate of a text row based on the specified alignment.</returns>
            public static float get_x_coord(TextLine textline, string align)
            {
                var x_left = textline.x0();
                var x_right = textline.x1();

#pragma warning disable IDE0066 // Convert switch statement to expression
                switch (align)
#pragma warning restore IDE0066 // Convert switch statement to expression
                {
                    case "left":
                        return x_left;
                    case "middle":
                        return x_left + ((x_right - x_left) / 2.0f);
                    case "right":
                        return x_right;
                    default:
                        throw new ArgumentOutOfRangeException(align, $"Unkown value '{align}'. Valid values are: left, middle and right.");
                }
            }

            /// <summary>
            /// Returns the index of an existing text edge using the specified x coordinate and alignment.
            /// </summary>
            /// <param name="x_coord"></param>
            /// <param name="align"></param>
            /// <returns>Returns the index of an existing text edge using the specified x coordinate and alignment. Null if not found.</returns>
            public int? find(float x_coord, string align)
            {
                var edges = this._textedges[align];

                for (int i = 0; i < edges.Count; i++)
                {
                    TextEdge te = edges[i];
                    if (MathExtensions.AlmostEquals(te.x, x_coord, 0.5))
                    {
                        return i;
                    }
                }
                return null;
            }

            /// <summary>
            /// Adds a new text edge to the current dict.
            /// </summary>
            /// <param name="textline"></param>
            /// <param name="align"></param>
            public void add(TextLine textline, string align)
            {
                var x = get_x_coord(textline, align);
                float y0 = textline.y0();
                float y1 = textline.y1();
                var te = new TextEdge(x, y0, y1, align);
                this._textedges[align].Add(te);
            }

            /// <summary>
            /// Updates an existing text edge in the current dict.
            /// </summary>
            /// <param name="textline"></param>
            public void update(TextLine textline)
            {
                foreach (string align in new[] { "left", "right", "middle" })
                {
                    var x_coord = get_x_coord(textline, align);
                    var idx = this.find(x_coord, align);
                    if (!idx.HasValue)
                    {
                        this.add(textline, align);
                    }
                    else
                    {
                        this._textedges[align][idx.Value].update_coords(x_coord, textline.y0(), this.edge_tol);
                    }
                }
            }

            /// <summary>
            /// Generates the text edges dict based on horizontal text rows.
            /// </summary>
            /// <param name="textlines"></param>
            public void generate(IReadOnlyList<TextLine> textlines)
            {
                foreach (var tl in textlines)
                {
                    if (tl.Text.Trim().Length > 1) // TODO: hacky
                    {
                        this.update(tl);
                    }
                }
            }

            /// <summary>
            /// Returns the list of relevant text edges (all share the same
            /// alignment) based on which list intersects horizontal text rows
            /// the most.
            /// </summary>
            /// <returns>
            /// Returns the list of relevant text edges (all share the same
            /// alignment) based on which list intersects horizontal text rows
            /// the most.
            /// </returns>
            public List<TextEdge> get_relevant()
            {
                var intersections_sum = new Dictionary<string, float>()
                {
                    { "left", this._textedges["left"].Where(te => te.is_valid).Sum(te => te.intersections) },
                    { "right", this._textedges["right"].Where(te => te.is_valid).Sum(te => te.intersections) },
                    { "middle", this._textedges["middle"].Where(te => te.is_valid).Sum(te => te.intersections) },
                };

                // TODO: naive
                // get vertical textedges that intersect maximum number of
                // times with horizontal textlines
                var relevant_align = intersections_sum.First(kvp => kvp.Value == intersections_sum.Values.Max()).Key; // max(intersections_sum.items(), key = itemgetter(1))[0];
                return this._textedges[relevant_align];
            }

            /// <summary>
            /// Returns a dict of interesting table areas on the PDF page calculated using relevant text edges.
            /// </summary>
            /// <param name="textlines"></param>
            /// <param name="relevant_textedges"></param>
            /// <returns>Returns a dict of interesting table areas on the PDF page calculated using relevant text edges.</returns>
            public Dictionary<(float, float, float, float), object> get_table_areas(IReadOnlyList<TextLine> textlines, IEnumerable<TextEdge> relevant_textedges)
            {
#pragma warning disable IDE0062 // Make local function 'static'
                (float, float, float, float) pad((float, float, float, float) area, float average_row_height)
#pragma warning restore IDE0062 // Make local function 'static'
                {
                    var x0 = area.Item1 - TABLE_AREA_PADDING;
                    var y0 = area.Item2 - TABLE_AREA_PADDING;
                    var x1 = area.Item3 + TABLE_AREA_PADDING;
                    // add a constant since table headers can be relatively up
                    var y1 = area.Item4 + (average_row_height * 5);
                    return (x0, y0, x1, y1);
                }

                // sort relevant textedges in reading order
                relevant_textedges = relevant_textedges.OrderBy(te => -te.y0).ThenBy(te => te.x); //.sort(key = lambda te: (-te.y0, te.x))

                var table_areas = new Dictionary<(float, float, float, float), object>();
                foreach (var te in relevant_textedges)
                {
                    if (te.is_valid)
                    {
                        if (table_areas.Count == 0)
                        {
                            table_areas[(te.x, te.y0, te.x, te.y1)] = null;
                        }
                        else
                        {
                            (float, float, float, float)? found = null;
                            foreach (var area in table_areas.Keys)
                            {
                                // check for overlap
                                if (te.y1 >= area.Item2 && te.y0 <= area.Item4)// [1] && te.y0 <= area[3])
                                {
                                    found = area;
                                    break;
                                }
                            }

                            if (!found.HasValue)
                            {
                                table_areas[(te.x, te.y0, te.x, te.y1)] = null;
                            }
                            else
                            {
                                table_areas.Remove(found.Value); //table_areas.pop(found)
                                var updated_area = (found.Value.Item1, //[0],
                                                    Math.Min(te.y0, found.Value.Item2), //[1]),
                                                    Math.Max(found.Value.Item3, te.x), //[2], te.x),
                                                    Math.Max(found.Value.Item4, te.y1)); //[3], te.y1));
                                table_areas[updated_area] = null;
                            }
                        }
                    }
                }

                // extend table areas based on textlines that overlap
                // vertically. it's possible that these textlines were
                // eliminated during textedges generation since numbers and
                // chars/words/sentences are often aligned differently.
                // drawback: table areas that have paragraphs on their sides
                // will include the paragraphs too.
                float sum_textline_height = 0;
                foreach (var tl in textlines)
                {
                    sum_textline_height += tl.y1() - tl.y0();
                    (float, float, float, float)? found = null;
                    foreach (var area in table_areas)
                    {
                        // check for overlap
                        if (tl.y0() >= area.Key.Item2 && tl.y1() <= area.Key.Item4) // [1] and tl.y1 <= area[3]:
                        {
                            found = area.Key;
                            break;
                        }
                    }

                    if (found.HasValue)
                    {
                        table_areas.Remove(found.Value);
                        var updated_area = (Math.Min(tl.x0(), found.Value.Item1),   //[0]),
                                            Math.Min(tl.y0(), found.Value.Item2),   //[1]),
                                            Math.Max(found.Value.Item3, tl.x1()),   //[2], tl.x1),
                                            Math.Max(found.Value.Item4, tl.y1()));  //[3], tl.y1));
                        table_areas[updated_area] = null;
                    }
                }
                float average_textline_height = sum_textline_height / textlines.Count;

                // add some padding to table areas
                var table_areas_padded = new Dictionary<(float, float, float, float), object>();
                foreach (var area in table_areas)
                {
                    table_areas_padded[pad(area.Key, average_textline_height)] = null;
                }
                return table_areas_padded;
            }
        }

        /// <summary>
        /// Defines a cell in a table with coordinates relative to a left-bottom origin. (PDF coordinate space)
        /// </summary>
        public class Cell
        {
            /// <summary>
            /// x-coordinate of left-bottom point.
            /// </summary>
            public float x1 { get; }

            /// <summary>
            /// y-coordinate of left-bottom point.
            /// </summary>
            public float y1 { get; }

            /// <summary>
            /// x-coordinate of right-top point.
            /// </summary>
            public float x2 { get; }

            /// <summary>
            /// y-coordinate of right-top point.
            /// </summary>
            public float y2 { get; }

            /// <summary>
            /// Tuple representing left-bottom coordinates.
            /// </summary>
            public (float x1, float y1) lb { get; }

            /// <summary>
            /// Tuple representing left-top coordinates.
            /// </summary>
            public (float x1, float y1) lt { get; }

            /// <summary>
            /// Tuple representing right-bottom coordinates.
            /// </summary>
            public (float x1, float y1) rb { get; }

            /// <summary>
            /// Tuple representing right-top coordinates.
            /// </summary>
            public (float x1, float y1) rt { get; }

            /// <summary>
            /// Whether or not cell is bounded on the left.
            /// </summary>
            public bool left { get; internal set; }

            /// <summary>
            /// Whether or not cell is bounded on the right.
            /// </summary>
            public bool right { get; internal set; }

            /// <summary>
            /// Whether or not cell is bounded on the top.
            /// </summary>
            public bool top { get; internal set; }

            /// <summary>
            /// Whether or not cell is bounded on the bottom.
            /// </summary>
            public bool bottom { get; internal set; }

            /// <summary>
            /// Whether or not cell spans horizontally.
            /// </summary>
            public bool hspan { get; internal set; }

            /// <summary>
            /// Whether or not cell spans vertically.
            /// </summary>
            public bool vspan { get; internal set; }

            /// <summary>
            /// Defines a cell in a table with coordinates relative to a left-bottom origin. (PDF coordinate space)
            /// </summary>
            /// <param name="x1">x-coordinate of left-bottom point.</param>
            /// <param name="y1">y-coordinate of left-bottom point.</param>
            /// <param name="x2">x-coordinate of right-top point.</param>
            /// <param name="y2">y-coordinate of right-top point.</param>
            public Cell(float x1, float y1, float x2, float y2)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
                this.lb = (x1, y1);
                this.lt = (x1, y2);
                this.rb = (x2, y1);
                this.rt = (x2, y2);
                this.left = false;
                this.right = false;
                this.top = false;
                this.bottom = false;
                this.hspan = false;
                this.vspan = false;
                this._text = "";
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"<Cell x1={Math.Round(this.x1, 2)} y1={Math.Round(this.y1, 2)} x2={Math.Round(this.x2, 2)} y2={Math.Round(this.y2, 2)}>";
            }

            private string _text;

            /// <summary>
            /// Text assigned to cell.
            /// </summary>
            public string text
            {
                get
                {
                    return _text;
                }

                set
                {
                    this._text = string.Concat(this._text, value);
                }
            }

            /// <summary>
            /// The number of sides on which the cell is bounded.
            /// </summary>
            public int bound => (this.top ? 1 : 0) + (this.bottom ? 1 : 0) + (this.left ? 1 : 0) + (this.right ? 1 : 0);
        }

        /// <summary>
        /// Defines a table with coordinates relative to a left-bottom origin. (PDF coordinate space)
        /// </summary>
        public class Table : IComparable<Table>
        {
            /// <summary>
            /// List of tuples representing column x-coordinates in increasing order.
            /// </summary>
            public List<(float, float)> cols { get; }

            /// <summary>
            /// List of tuples representing row y-coordinates in decreasing order.
            /// </summary>
            public List<(float, float)> rows { get; }

            /// <summary>
            /// class:`pandas.DataFrame`
            /// </summary>
            public object df { get; }

            /// <summary>
            /// tuple - Shape of the table.
            /// </summary>
            public (int, int) shape { get; internal set; }

            /// <summary>
            /// Accuracy with which text was assigned to the cell.
            /// </summary>
            public float accuracy { get; internal set; }

            /// <summary>
            /// Percentage of whitespace in the table.
            /// </summary>
            public float whitespace { get; internal set; }

            /// <summary>
            /// Table number on PDF page.
            /// </summary>
            public int? order { get; set; }

            /// <summary>
            /// PDF page number.
            /// </summary>
            public int? page { get; set; }

            public List<List<Cell>> cells { get; }

            public (float x1, float y1, float x2, float y2) _bbox { get; internal set; }

            public string flavor { get; internal set; }

            public List<(float, float, float, float)> _text { get; internal set; }

            internal (List<(float, float, float, float)>, List<(float, float, float, float)>) _segments { get; set; }

            internal List<TextEdge> _textedges { get; set; }

            /// <summary>
            /// Defines a table with coordinates relative to a left-bottom origin. (PDF coordinate space)
            /// </summary>
            /// <param name="cols">List of tuples representing column x-coordinates in increasing order.</param>
            /// <param name="rows">List of tuples representing row y-coordinates in decreasing order.</param>
            public Table(List<(float, float)> cols, List<(float, float)> rows)
            {
                this.cols = cols;
                this.rows = rows;

                this.cells = new List<List<Cell>>();
                for (int r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];
                    this.cells.Add(new List<Cell>());
                    foreach (var c in cols)
                    {
                        this.cells[r].Add(new Cell(c.Item1, row.Item2, c.Item2, row.Item1));
                    }
                }

                this.df = null;
                this.shape = (0, 0);
                this.accuracy = 0;
                this.whitespace = 0;
                this.order = null;
                this.page = null;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"<{this.GetType().Name} shape={this.shape}>";
            }

            public int CompareTo(Table other)
            {
                if (this.page.HasValue && other.page.HasValue)
                {
                    if (this.page.Value == other.page.Value)
                    {
                        if (this.order.HasValue && other.order.HasValue)
                        {
                            return this.order.Value.CompareTo(other.order.Value);
                        }
                        else
                        {
                            throw new ArgumentException($"Cannot do CompareTo() bewteen {this.order?.GetType().Name ?? "null"} and {other.order?.GetType().Name ?? "null"}.");
                        }
                    }

                    return this.page.Value.CompareTo(other.page.Value);
                }
                throw new ArgumentException($"Cannot do CompareTo() bewteen {this.page?.GetType().Name ?? "null"} and {other.page?.GetType().Name ?? "null"}.");
            }

            /// <summary>
            /// Returns two-dimensional list of strings in table.
            /// </summary>
            /// <returns>Returns two-dimensional list of strings in table.</returns>
            public List<List<string>> data()
            {
                List<List<string>> d = new List<List<string>>();
                foreach (var row in cells)
                {
                    var lr = new List<string>();
                    foreach (var cell in row)
                    {
                        lr.Add(cell.text.Trim());
                    }
                    d.Add(lr);
                }
                return d;
            }

            /// <summary>
            /// Returns a parsing report with %accuracy, %whitespace, table number on page and page number.
            /// </summary>
            /// <returns>Returns a parsing report with %accuracy, %whitespace, table number on page and page number.</returns>
            public Dictionary<string, object> parsing_report()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Sets all table edges to True.
            /// </summary>
            public Table set_all_edges()
            {
                for (int r = 0; r < this.rows.Count; r++)
                {
                    for (int c = 0; c < this.cols.Count; c++)
                    {
                        this.cells[r][c].left = true;
                        this.cells[r][c].right = true;
                        this.cells[r][c].top = true;
                        this.cells[r][c].bottom = true;
                    }
                }
                return this;
            }

            /// <summary>
            /// Sets a cell's edges to True depending on whether the cell's
            /// coordinates overlap with the line's coordinates within a
            /// tolerance.
            /// </summary>
            /// <param name="vertical">List of detected vertical lines.</param>
            /// <param name="horizontal">List of detected horizontal lines.</param>
            /// <param name="joint_tol"></param>
            public Table set_edges(List<(float, float, float, float)> vertical, List<(float, float, float, float)> horizontal, int joint_tol = 2)
            {
                foreach (var v in vertical)
                {
                    // find closest x coord
                    // iterate over y coords and find closest start and end points
                    var i = this.cols.Select((t, _i) => (t, _i)).Where(u => MathExtensions.AlmostEquals(v.Item1, u.t.Item1, joint_tol)).Select(u => u._i).ToArray(); //i = [i for i, t in enumerate(self.cols) if np.isclose(v[0], t[0], atol = joint_tol)]
                    var j = this.rows.Select((t, _j) => (t, _j)).Where(u => MathExtensions.AlmostEquals(v.Item4, u.t.Item1, joint_tol)).Select(u => u._j).ToArray(); //j = [j for j, t in enumerate(self.rows) if np.isclose(v[3], t[0], atol = joint_tol)]
                    var k = this.rows.Select((t, _k) => (t, _k)).Where(u => MathExtensions.AlmostEquals(v.Item2, u.t.Item1, joint_tol)).Select(u => u._k).ToArray(); //k = [k for k, t in enumerate(self.rows) if np.isclose(v[1], t[0], atol = joint_tol)]

                    if (j.Length == 0) continue; // if not j // why not before k???
                    int J = j[0];
                    int L = -1; // set L before
                    int K = -1; // set K before

                    if (i.Length == 1 && i[0] == 0) //if i == [0]: // only left edge
                    {
                        L = i[0];
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.cells[J][L].left = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.rows.Count;
                            while (J < K)
                            {
                                this.cells[J][L].left = true;
                                J++;
                            }
                        }
                    }
                    else if (i.Length == 0) // only right edge
                    {
                        L = this.cols.Count - 1;
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.cells[J][L].right = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = rows.Count;
                            while (J < K)
                            {
                                cells[J][L].right = true;
                                J++;
                            }
                        }
                    }
                    else // both left and right edges
                    {
                        L = i[0];
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.cells[J][L].left = true;
                                this.cells[J][L - 1].right = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.rows.Count;
                            while (J < K)
                            {
                                this.cells[J][L].left = true;
                                this.cells[J][L - 1].right = true;
                                J++;
                            }
                        }
                    }
                }

                foreach (var h in horizontal)
                {
                    // find closest y coord
                    // iterate over x coords and find closest start and end points
                    var i = this.rows.Select((t, _i) => (t, _i)).Where(u => MathExtensions.AlmostEquals(h.Item2, u.t.Item1, joint_tol)).Select(u => u._i).ToArray(); //i = [i for i, t in enumerate(self.rows) if np.isclose(h[1], t[0], atol = joint_tol)]
                    var j = this.cols.Select((t, _j) => (t, _j)).Where(u => MathExtensions.AlmostEquals(h.Item1, u.t.Item1, joint_tol)).Select(u => u._j).ToArray(); //j = [j for j, t in enumerate(self.cols) if np.isclose(h[0], t[0], atol = joint_tol)]
                    var k = this.cols.Select((t, _k) => (t, _k)).Where(u => MathExtensions.AlmostEquals(h.Item3, u.t.Item1, joint_tol)).Select(u => u._k).ToArray(); //k = [k for k, t in enumerate(self.cols) if np.isclose(h[2], t[0], atol = joint_tol)]

                    if (j.Length == 0) continue; // if not j // why not before k???
                    int J = j[0];
                    int L = -1; // set L before
                    int K = -1; // set K before

                    if (i.Length == 1 && i[0] == 0) // only top edge
                    {
                        L = i[0];
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.cells[L][J].top = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.cols.Count;
                            while (J < K)
                            {
                                this.cells[L][J].top = true;
                                J++;
                            }
                        }
                    }
                    else if (i.Length == 0) // only bottom edge
                    {
                        L = this.rows.Count - 1;
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.cells[L][J].bottom = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.cols.Count;
                            while (J < K)
                            {
                                this.cells[L][J].bottom = true;
                                J++;
                            }
                        }
                    }
                    else // both top and bottom edges
                    {
                        L = i[0];
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.cells[L][J].top = true;
                                this.cells[L - 1][J].bottom = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.cols.Count;
                            while (J < K)
                            {
                                this.cells[L][J].top = true;
                                this.cells[L - 1][J].bottom = true;
                                J++;
                            }
                        }
                    }
                }

                return this;
            }

            /// <summary>
            /// Sets table border edges to True.
            /// </summary>
            public Table set_border()
            {
                for (int r = 0; r < this.rows.Count; r++)
                {
                    this.cells[r][0].left = true;
                    this.cells[r][this.cols.Count - 1].right = true;
                }

                for (int c = 0; c < this.cols.Count; c++)
                {
                    this.cells[0][c].top = true;
                    this.cells[this.rows.Count - 1][c].bottom = true;
                }

                return this;
            }

            /// <summary>
            /// Sets a cell's hspan or vspan attribute to True depending
            /// on whether the cell spans horizontally or vertically.
            /// </summary>
            public Table set_span()
            {
                foreach (var row in cells)
                {
                    foreach (var cell in row)
                    {
                        var left = cell.left;
                        var right = cell.right;
                        var top = cell.top;
                        var bottom = cell.bottom;

                        if (cell.bound == 4)
                        {
                            continue;
                        }
                        else if (cell.bound == 3)
                        {
                            if (!left && (right && top && bottom))
                            {
                                cell.hspan = true;
                            }
                            else if (!right && (left && top && bottom))
                            {
                                cell.hspan = true;
                            }
                            else if (!top && (left && right && bottom))
                            {
                                cell.vspan = true;
                            }
                            else if (!bottom && (left && right && top))
                            {
                                cell.vspan = true;
                            }
                        }
                        else if (cell.bound == 2)
                        {
                            if (left && right && (!top && !bottom))
                            {
                                cell.vspan = true;
                            }
                            else if (top && bottom && (!left && !right))
                            {
                                cell.hspan = true;
                            }
                        }
                        else if (cell.bound == 1 || cell.bound == 0)
                        {
                            cell.vspan = true;
                            cell.hspan = true;
                        }
                    }
                }
                return this;
            }

            /// <summary>
            /// Writes Table to a comma-separated values (csv) file.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_csv`.</para>
            /// </summary>
            /// <param name="path">Output filepath.</param>
            /// <param name="kwargs"></param>
            public void to_csv(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to a JSON file.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_json`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void to_json(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to an Excel file.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_excel`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void to_excel(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to an HTML file.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_html`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void to_html(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to sqlite database.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_sql`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void to_sqlite(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Defines a list of camelot.core.Table objects. Each table can
        /// be accessed using its index.
        /// </summary>
        public class TableList : List<Table>
        {
            /// <summary>
            /// Defines a list of camelot.core.Table objects. Each table can
            /// be accessed using its index.
            /// </summary>
            public TableList(IEnumerable<Table> tables)
            {
                this.AddRange(tables);
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"<{this.GetType().Name} n={this.n}>";
            }

            public int len => this.Count;

            //def _format_func(table, f):
            //    return getattr(table, f"to_{f}")

            /// <summary>
            /// Number of tables in the list.
            /// </summary>
            public int n => this.Count;

            public void _write_file(object f = null, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            public void _compress_dir(params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Exports the list of tables to specified file format.
            /// </summary>
            /// <param name="path"> Output filepath.</param>
            /// <param name="f">File format. Can be csv, json, excel, html and sqlite.</param>
            /// <param name="compress">Whether or not to add files to a ZIP archive.</param>
            public void export(string path, string f = "csv", bool compress = false)
            {
                throw new NotImplementedException();
            }
        }
    }
}
