using System;
using System.Collections.Generic;
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
            public int Intersections { get; private set; }

            /// <summary>
            /// A text edge is valid if it intersections with at least TEXTEDGE_REQUIRED_ELEMENTS horizontal text rows.
            /// </summary>
            public bool IsValid { get; set; }

            /// <summary>
            /// x-coordinate of the text edge.
            /// </summary>
            public float X { get; internal set; }

            /// <summary>
            /// y-coordinate of bottommost point.
            /// </summary>
            public float Y0 { get; internal set; }

            /// <summary>
            /// y-coordinate of topmost point.
            /// </summary>
            public float Y1 { get; internal set; }

            /// <summary>
            /// {'left', 'right', 'middle'}
            /// </summary>
            public string Align { get; internal set; }

            /// <summary>
            /// Defines a text edge coordinates relative to a left-bottom origin. (PDF coordinate space)
            /// </summary>
            /// <param name="x">x-coordinate of the text edge.</param>
            /// <param name="y0">y-coordinate of bottommost point.</param>
            /// <param name="y1">y-coordinate of topmost point.</param>
            /// <param name="align">optional (default: 'left') {'left', 'right', 'middle'}</param>
            public TextEdge(float x, float y0, float y1, string align = "left")
            {
                X = x;
                Y0 = y0;
                Y1 = y1;
                Align = align;
                Intersections = 0;
                IsValid = false;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"<TextEdge x={Math.Round(X, 2)} y0={Math.Round(Y0, 2)} y1={Math.Round(Y1, 2)} align={Align} valid={IsValid}>";
            }

            /// <summary>
            /// Updates the text edge's x and bottom y coordinates and sets the is_valid attribute.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y0"></param>
            /// <param name="edge_tol"></param>
            public void UpdateCoords(float x, float y0, float edge_tol = 50)
            {
                if (MathExtensions.AlmostEquals(this.Y0, y0, edge_tol))
                {
                    this.X = ((this.Intersections * this.X) + x) / (this.Intersections + 1);
                    this.Y0 = y0;
                    this.Intersections++;
                    // a textedge is valid only if it extends uninterrupted
                    // over a required number of textlines
                    if (this.Intersections > TEXTEDGE_REQUIRED_ELEMENTS)
                    {
                        this.IsValid = true;
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
            public float EdgeTol { get; }

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
                this.EdgeTol = edge_tol;
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
            public static float GetXCoord(TextLine textline, string align)
            {
                var x_left = textline.X0();
                var x_right = textline.X1();

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
            public int? Find(float x_coord, string align)
            {
                var edges = this._textedges[align];

                for (int i = 0; i < edges.Count; i++)
                {
                    TextEdge te = edges[i];
                    if (MathExtensions.AlmostEquals(te.X, x_coord, 0.5))
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
            public void Add(TextLine textline, string align)
            {
                var x = GetXCoord(textline, align);
                float y0 = textline.Y0();
                float y1 = textline.Y1();
                var te = new TextEdge(x, y0, y1, align);
                this._textedges[align].Add(te);
            }

            /// <summary>
            /// Updates an existing text edge in the current dict.
            /// </summary>
            /// <param name="textline"></param>
            public void Update(TextLine textline)
            {
                foreach (string align in new[] { "left", "right", "middle" })
                {
                    var x_coord = GetXCoord(textline, align);
                    var idx = this.Find(x_coord, align);
                    if (!idx.HasValue)
                    {
                        this.Add(textline, align);
                    }
                    else
                    {
                        this._textedges[align][idx.Value].UpdateCoords(x_coord, textline.Y0(), this.EdgeTol);
                    }
                }
            }

            /// <summary>
            /// Generates the text edges dict based on horizontal text rows.
            /// </summary>
            /// <param name="textlines"></param>
            public void Generate(IReadOnlyList<TextLine> textlines)
            {
                foreach (var tl in textlines)
                {
                    if (tl.Text.Trim().Length > 1) // TODO: hacky
                    {
                        this.Update(tl);
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
            public List<TextEdge> GetRelevant()
            {
                var intersections_sum = new Dictionary<string, float>()
                {
                    { "left", this._textedges["left"].Where(te => te.IsValid).Sum(te => te.Intersections) },
                    { "right", this._textedges["right"].Where(te => te.IsValid).Sum(te => te.Intersections) },
                    { "middle", this._textedges["middle"].Where(te => te.IsValid).Sum(te => te.Intersections) },
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
            public Dictionary<(float, float, float, float), object> GetTableAreas(IReadOnlyList<TextLine> textlines, IEnumerable<TextEdge> relevant_textedges)
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
                relevant_textedges = relevant_textedges.OrderBy(te => -te.Y0).ThenBy(te => te.X); //.sort(key = lambda te: (-te.y0, te.x))

                var table_areas = new Dictionary<(float, float, float, float), object>();
                foreach (var te in relevant_textedges)
                {
                    if (te.IsValid)
                    {
                        if (table_areas.Count == 0)
                        {
                            table_areas[(te.X, te.Y0, te.X, te.Y1)] = null;
                        }
                        else
                        {
                            (float, float, float, float)? found = null;
                            foreach (var area in table_areas.Keys)
                            {
                                // check for overlap
                                if (te.Y1 >= area.Item2 && te.Y0 <= area.Item4)// [1] && te.y0 <= area[3])
                                {
                                    found = area;
                                    break;
                                }
                            }

                            if (!found.HasValue)
                            {
                                table_areas[(te.X, te.Y0, te.X, te.Y1)] = null;
                            }
                            else
                            {
                                table_areas.Remove(found.Value); //table_areas.pop(found)
                                var updated_area = (found.Value.Item1, //[0],
                                                    Math.Min(te.Y0, found.Value.Item2), //[1]),
                                                    Math.Max(found.Value.Item3, te.X), //[2], te.x),
                                                    Math.Max(found.Value.Item4, te.Y1)); //[3], te.y1));
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
                    sum_textline_height += tl.Y1() - tl.Y0();
                    (float, float, float, float)? found = null;
                    foreach (var area in table_areas)
                    {
                        // check for overlap
                        if (tl.Y0() >= area.Key.Item2 && tl.Y1() <= area.Key.Item4) // [1] and tl.y1 <= area[3]:
                        {
                            found = area.Key;
                            break;
                        }
                    }

                    if (found.HasValue)
                    {
                        table_areas.Remove(found.Value);
                        var updated_area = (Math.Min(tl.X0(), found.Value.Item1),   //[0]),
                                            Math.Min(tl.Y0(), found.Value.Item2),   //[1]),
                                            Math.Max(found.Value.Item3, tl.X1()),   //[2], tl.x1),
                                            Math.Max(found.Value.Item4, tl.Y1()));  //[3], tl.y1));
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
            public float X1 { get; }

            /// <summary>
            /// y-coordinate of left-bottom point.
            /// </summary>
            public float Y1 { get; }

            /// <summary>
            /// x-coordinate of right-top point.
            /// </summary>
            public float X2 { get; }

            /// <summary>
            /// y-coordinate of right-top point.
            /// </summary>
            public float Y2 { get; }

            /// <summary>
            /// Tuple representing left-bottom coordinates.
            /// </summary>
            public (float x1, float y1) Lb { get; }

            /// <summary>
            /// Tuple representing left-top coordinates.
            /// </summary>
            public (float x1, float y1) Lt { get; }

            /// <summary>
            /// Tuple representing right-bottom coordinates.
            /// </summary>
            public (float x1, float y1) Rb { get; }

            /// <summary>
            /// Tuple representing right-top coordinates.
            /// </summary>
            public (float x1, float y1) Rt { get; }

            /// <summary>
            /// Whether or not cell is bounded on the left.
            /// </summary>
            public bool Left { get; internal set; }

            /// <summary>
            /// Whether or not cell is bounded on the right.
            /// </summary>
            public bool Right { get; internal set; }

            /// <summary>
            /// Whether or not cell is bounded on the top.
            /// </summary>
            public bool Top { get; internal set; }

            /// <summary>
            /// Whether or not cell is bounded on the bottom.
            /// </summary>
            public bool Bottom { get; internal set; }

            /// <summary>
            /// Whether or not cell spans horizontally.
            /// </summary>
            public bool HSpan { get; internal set; }

            /// <summary>
            /// Whether or not cell spans vertically.
            /// </summary>
            public bool VSpan { get; internal set; }

            /// <summary>
            /// Defines a cell in a table with coordinates relative to a left-bottom origin. (PDF coordinate space)
            /// </summary>
            /// <param name="x1">x-coordinate of left-bottom point.</param>
            /// <param name="y1">y-coordinate of left-bottom point.</param>
            /// <param name="x2">x-coordinate of right-top point.</param>
            /// <param name="y2">y-coordinate of right-top point.</param>
            public Cell(float x1, float y1, float x2, float y2)
            {
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
                Lb = (x1, y1);
                Lt = (x1, y2);
                Rb = (x2, y1);
                Rt = (x2, y2);
                Left = false;
                Right = false;
                Top = false;
                Bottom = false;
                HSpan = false;
                VSpan = false;
                _text = "";
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"<Cell x1={Math.Round(X1, 2)} y1={Math.Round(Y1, 2)} x2={Math.Round(X2, 2)} y2={Math.Round(Y2, 2)}>";
            }

            private string _text;

            /// <summary>
            /// Text assigned to cell.
            /// </summary>
            public string Text
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
            public int Bound => (this.Top ? 1 : 0) + (this.Bottom ? 1 : 0) + (this.Left ? 1 : 0) + (this.Right ? 1 : 0);
        }

        /// <summary>
        /// Defines a table with coordinates relative to a left-bottom origin. (PDF coordinate space)
        /// </summary>
        public class Table : IComparable<Table>
        {
            /// <summary>
            /// List of tuples representing column x-coordinates in increasing order.
            /// </summary>
            public List<(float, float)> Cols { get; }

            /// <summary>
            /// List of tuples representing row y-coordinates in decreasing order.
            /// </summary>
            public List<(float, float)> Rows { get; }

            ///// <summary>
            ///// class:`pandas.DataFrame`
            ///// </summary>
            //public object df { get; }

            /// <summary>
            /// tuple - Shape of the table.
            /// </summary>
            public (int, int) Shape { get; internal set; }

            /// <summary>
            /// Accuracy with which text was assigned to the cell.
            /// </summary>
            public float Accuracy { get; internal set; }

            /// <summary>
            /// Percentage of whitespace in the table.
            /// </summary>
            public float Whitespace { get; internal set; }

            /// <summary>
            /// Table number on PDF page.
            /// </summary>
            public int? Order { get; set; }

            /// <summary>
            /// PDF page number.
            /// </summary>
            public int? Page { get; set; }

            public List<List<Cell>> Cells { get; }

            public (float x1, float y1, float x2, float y2) _bbox { get; internal set; }

            public string Flavor { get; internal set; }

            public List<(float, float, float, float)> _text { get; internal set; }

            internal (List<(float, float, float, float)>, List<(float, float, float, float)>) segments { get; set; }

            internal List<TextEdge> textedges { get; set; }

            /// <summary>
            /// Defines a table with coordinates relative to a left-bottom origin. (PDF coordinate space)
            /// </summary>
            /// <param name="cols">List of tuples representing column x-coordinates in increasing order.</param>
            /// <param name="rows">List of tuples representing row y-coordinates in decreasing order.</param>
            public Table(List<(float, float)> cols, List<(float, float)> rows)
            {
                this.Cols = cols;
                this.Rows = rows;

                this.Cells = new List<List<Cell>>();
                for (int r = 0; r < rows.Count; r++)
                {
                    var row = rows[r];
                    this.Cells.Add(new List<Cell>());
                    foreach (var c in cols)
                    {
                        this.Cells[r].Add(new Cell(c.Item1, row.Item2, c.Item2, row.Item1));
                    }
                }

                this.Shape = (0, 0);
                this.Accuracy = 0;
                this.Whitespace = 0;
                this.Order = null;
                this.Page = null;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return $"<{this.GetType().Name} shape={this.Shape}>";
            }

            public int CompareTo(Table other)
            {
                if (this.Page.HasValue && other.Page.HasValue)
                {
                    if (this.Page.Value == other.Page.Value)
                    {
                        if (this.Order.HasValue && other.Order.HasValue)
                        {
                            return this.Order.Value.CompareTo(other.Order.Value);
                        }
                        else
                        {
                            throw new ArgumentException($"Cannot do CompareTo() bewteen {this.Order?.GetType().Name ?? "null"} and {other.Order?.GetType().Name ?? "null"}.");
                        }
                    }

                    return this.Page.Value.CompareTo(other.Page.Value);
                }
                throw new ArgumentException($"Cannot do CompareTo() bewteen {this.Page?.GetType().Name ?? "null"} and {other.Page?.GetType().Name ?? "null"}.");
            }

            /// <summary>
            /// Returns two-dimensional list of strings in table.
            /// </summary>
            /// <returns>Returns two-dimensional list of strings in table.</returns>
            public List<List<string>> Data()
            {
                List<List<string>> d = new List<List<string>>();
                foreach (var row in Cells)
                {
                    var lr = new List<string>();
                    foreach (var cell in row)
                    {
                        lr.Add(cell.Text.Trim());
                    }
                    d.Add(lr);
                }
                return d;
            }

            /// <summary>
            /// Returns a parsing report with %accuracy, %whitespace, table number on page and page number.
            /// </summary>
            /// <returns>Returns a parsing report with %accuracy, %whitespace, table number on page and page number.</returns>
            public Dictionary<string, object> ParsingReport()
            {
                return new Dictionary<string, object>()
                {
                    {  "accuracy", Math.Round(Accuracy, 2) },
                    { "whitespace", Math.Round(Whitespace, 2) },
                    { "order", Order },
                    { "page", Page }
                };
            }

            /// <summary>
            /// Sets all table edges to True.
            /// </summary>
            public Table SetAllEdges()
            {
                for (int r = 0; r < this.Rows.Count; r++)
                {
                    for (int c = 0; c < this.Cols.Count; c++)
                    {
                        this.Cells[r][c].Left = true;
                        this.Cells[r][c].Right = true;
                        this.Cells[r][c].Top = true;
                        this.Cells[r][c].Bottom = true;
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
            public Table SetEdges(List<(float, float, float, float)> vertical, List<(float, float, float, float)> horizontal, int joint_tol = 2)
            {
                foreach (var v in vertical)
                {
                    // find closest x coord
                    // iterate over y coords and find closest start and end points
                    var i = this.Cols.Select((t, _i) => (t, _i)).Where(u => MathExtensions.AlmostEquals(v.Item1, u.t.Item1, joint_tol)).Select(u => u._i).ToArray(); //i = [i for i, t in enumerate(self.cols) if np.isclose(v[0], t[0], atol = joint_tol)]
                    var j = this.Rows.Select((t, _j) => (t, _j)).Where(u => MathExtensions.AlmostEquals(v.Item4, u.t.Item1, joint_tol)).Select(u => u._j).ToArray(); //j = [j for j, t in enumerate(self.rows) if np.isclose(v[3], t[0], atol = joint_tol)]
                    var k = this.Rows.Select((t, _k) => (t, _k)).Where(u => MathExtensions.AlmostEquals(v.Item2, u.t.Item1, joint_tol)).Select(u => u._k).ToArray(); //k = [k for k, t in enumerate(self.rows) if np.isclose(v[1], t[0], atol = joint_tol)]

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
                                this.Cells[J][L].Left = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.Rows.Count;
                            while (J < K)
                            {
                                this.Cells[J][L].Left = true;
                                J++;
                            }
                        }
                    }
                    else if (i.Length == 0) // only right edge
                    {
                        L = this.Cols.Count - 1;
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.Cells[J][L].Right = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = Rows.Count;
                            while (J < K)
                            {
                                Cells[J][L].Right = true;
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
                                this.Cells[J][L].Left = true;
                                this.Cells[J][L - 1].Right = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.Rows.Count;
                            while (J < K)
                            {
                                this.Cells[J][L].Left = true;
                                this.Cells[J][L - 1].Right = true;
                                J++;
                            }
                        }
                    }
                }

                foreach (var h in horizontal)
                {
                    // find closest y coord
                    // iterate over x coords and find closest start and end points
                    var i = this.Rows.Select((t, _i) => (t, _i)).Where(u => MathExtensions.AlmostEquals(h.Item2, u.t.Item1, joint_tol)).Select(u => u._i).ToArray(); //i = [i for i, t in enumerate(self.rows) if np.isclose(h[1], t[0], atol = joint_tol)]
                    var j = this.Cols.Select((t, _j) => (t, _j)).Where(u => MathExtensions.AlmostEquals(h.Item1, u.t.Item1, joint_tol)).Select(u => u._j).ToArray(); //j = [j for j, t in enumerate(self.cols) if np.isclose(h[0], t[0], atol = joint_tol)]
                    var k = this.Cols.Select((t, _k) => (t, _k)).Where(u => MathExtensions.AlmostEquals(h.Item3, u.t.Item1, joint_tol)).Select(u => u._k).ToArray(); //k = [k for k, t in enumerate(self.cols) if np.isclose(h[2], t[0], atol = joint_tol)]

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
                                this.Cells[L][J].Top = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.Cols.Count;
                            while (J < K)
                            {
                                this.Cells[L][J].Top = true;
                                J++;
                            }
                        }
                    }
                    else if (i.Length == 0) // only bottom edge
                    {
                        L = this.Rows.Count - 1;
                        if (k.Length > 0)
                        {
                            K = k[0];
                            while (J < K)
                            {
                                this.Cells[L][J].Bottom = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.Cols.Count;
                            while (J < K)
                            {
                                this.Cells[L][J].Bottom = true;
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
                                this.Cells[L][J].Top = true;
                                this.Cells[L - 1][J].Bottom = true;
                                J++;
                            }
                        }
                        else
                        {
                            K = this.Cols.Count;
                            while (J < K)
                            {
                                this.Cells[L][J].Top = true;
                                this.Cells[L - 1][J].Bottom = true;
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
            public Table SetBorder()
            {
                for (int r = 0; r < this.Rows.Count; r++)
                {
                    this.Cells[r][0].Left = true;
                    this.Cells[r][this.Cols.Count - 1].Right = true;
                }

                for (int c = 0; c < this.Cols.Count; c++)
                {
                    this.Cells[0][c].Top = true;
                    this.Cells[this.Rows.Count - 1][c].Bottom = true;
                }

                return this;
            }

            /// <summary>
            /// Sets a cell's hspan or vspan attribute to True depending
            /// on whether the cell spans horizontally or vertically.
            /// </summary>
            public Table SetSpan()
            {
                foreach (var row in Cells)
                {
                    foreach (var cell in row)
                    {
                        var left = cell.Left;
                        var right = cell.Right;
                        var top = cell.Top;
                        var bottom = cell.Bottom;

                        if (cell.Bound == 4)
                        {
                            continue;
                        }
                        else if (cell.Bound == 3)
                        {
                            if (!left && (right && top && bottom))
                            {
                                cell.HSpan = true;
                            }
                            else if (!right && (left && top && bottom))
                            {
                                cell.HSpan = true;
                            }
                            else if (!top && (left && right && bottom))
                            {
                                cell.VSpan = true;
                            }
                            else if (!bottom && (left && right && top))
                            {
                                cell.VSpan = true;
                            }
                        }
                        else if (cell.Bound == 2)
                        {
                            if (left && right && (!top && !bottom))
                            {
                                cell.VSpan = true;
                            }
                            else if (top && bottom && (!left && !right))
                            {
                                cell.HSpan = true;
                            }
                        }
                        else if (cell.Bound == 1 || cell.Bound == 0)
                        {
                            cell.VSpan = true;
                            cell.HSpan = true;
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
            public void ToCsv(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to a JSON file.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_json`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void ToJson(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to an Excel file.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_excel`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void ToExcel(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to an HTML file.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_html`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void ToHtml(string path, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Writes Table to sqlite database.
            /// <para>For kwargs, check :meth:`pandas.DataFrame.to_sql`.</para>
            /// </summary>
            /// <param name="path"></param>
            /// <param name="kwargs"></param>
            public void ToSqlite(string path, params string[] kwargs)
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
                return $"<{this.GetType().Name} N={this.N}>";
            }

            /// <summary>
            /// Number of tables in the list.
            /// </summary>
            public int N => this.Count;

            internal void _write_file(object f = null, params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            internal void _compress_dir(params string[] kwargs)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Exports the list of tables to specified file format.
            /// </summary>
            /// <param name="path"> Output filepath.</param>
            /// <param name="f">File format. Can be csv, json, excel, html and sqlite.</param>
            /// <param name="compress">Whether or not to add files to a ZIP archive.</param>
            public void Export(string path, string f = "csv", bool compress = false)
            {
                throw new NotImplementedException();
            }
        }
    }
}
