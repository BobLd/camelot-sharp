using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Logging;
using static Camelot.Core;

namespace Camelot
{
    // https://github.com/camelot-dev/camelot/blob/master/camelot/utils.py

    public static class Utils
    {
        /// <summary>
        /// Check to see if a URL has a valid protocol.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>If url has a valid protocol return True otherwise False.</returns>
        public static bool IsUrl(string url)
        {
            throw new NotImplementedException();
        }

        public static string RandomString(int length)
        {
            throw new NotImplementedException();
        }

        public static string DownloadUrl(string url)
        {
            throw new NotImplementedException();
        }

        internal static bool ValidateInput(string[] kwargs, string flavor = "lattice")
        {
            throw new NotImplementedException();
        }

        internal static string[] RemoveExtra(string[] kwargs, string flavor = "lattice")
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Translates x2 by x1.
        /// </summary>
        public static float Translate(float x1, float x2)
        {
            x2 += x1;
            return x2;
        }

        /// <summary>
        /// Scales x by scaling factor s.
        /// </summary>
        public static float Scale(float x, float s)
        {
            x *= s;
            return x;
        }

        /// <summary>
        /// Translates and scales pdf coordinate space to image coordinate space.
        /// </summary>
        /// <param name="k">Tuple (x1, y1, x2, y2) representing table bounding box where
        /// (x1, y1) -> lt and(x2, y2) -> rb in PDFMiner coordinate space.</param>
        /// <param name="factors">Tuple (scaling_factor_x, scaling_factor_y, pdf_y) where the
        /// first two elements are scaling factors and pdf_y is height of pdf.</param>
        /// <returns>Tuple (x1, y1, x2, y2) representing table bounding box where
        /// (x1, y1) -> lt and(x2, y2) -> rb in OpenCV coordinate space.</returns>
        public static (int x1, int y1, int x2, int y2) ScalePdf((float x1, float y1, float x2, float y2) k, (float scaling_factor_x, float scaling_factor_y, float pdf_y) factors)
        {
            (float x1, float y1, float x2, float y2) = k;
            if (float.IsInfinity(x1) || float.IsInfinity(y1) || float.IsInfinity(x2) || float.IsInfinity(y2) ||
                float.IsNaN(x1) || float.IsNaN(y1) || float.IsNaN(x2) || float.IsNaN(y2))
            {
                throw new ArgumentOutOfRangeException(nameof(k), "Infinity or NaN.");
            }

            (float scaling_factor_x, float scaling_factor_y, float pdf_y) = factors;
            if (float.IsInfinity(scaling_factor_x) || float.IsInfinity(scaling_factor_y) || float.IsInfinity(pdf_y) ||
                float.IsNaN(scaling_factor_x) || float.IsNaN(scaling_factor_y) || float.IsNaN(pdf_y))
            {
                throw new ArgumentOutOfRangeException(nameof(factors), "Infinity or NaN.");
            }

            x1 = Scale(x1, scaling_factor_x);
            y1 = Scale(Math.Abs(Translate(-pdf_y, y1)), scaling_factor_y);
            x2 = Scale(x2, scaling_factor_x);
            y2 = Scale(Math.Abs(Translate(-pdf_y, y2)), scaling_factor_y);
            return ((int)x1, (int)y1, (int)x2, (int)y2);
        }

        /// <summary>
        /// Translates and scales image coordinate space to pdf coordinate space.
        /// </summary>
        /// <param name="tables">Dict with table boundaries as keys and list of intersections in that boundary as value.</param>
        /// <param name="v_segments">List of vertical line segments.</param>
        /// <param name="h_segments">List of horizontal line segments.</param>
        /// <param name="factors">Tuple (scaling_factor_x, scaling_factor_y, img_y) where the
        /// first two elements are scaling factors and img_y is height of image.</param>
        /// <returns>tables_new : dict, v_segments_new : dict, h_segments_new : dict</returns>
        public static (Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> tables_new, List<(float, float, float, float)> v_segments_new, List<(float, float, float, float)> h_segments_new)
            ScaleImage(Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> tables,
            List<(int, int, int, int)> v_segments,
            List<(int, int, int, int)> h_segments,
            (float scaling_factor_x, float scaling_factor_y, float img_y) factors)
        {
            (float scaling_factor_x, float scaling_factor_y, float img_y) = factors;
            var tables_new = new Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>>();
            foreach (var k in tables.Keys)
            {
                (float x1, float y1, float x2, float y2) = k;
                x1 = Scale(x1, scaling_factor_x);
                y1 = Scale(Math.Abs(Translate(-img_y, y1)), scaling_factor_y);
                x2 = Scale(x2, scaling_factor_x);
                y2 = Scale(Math.Abs(Translate(-img_y, y2)), scaling_factor_y);

                List<float> j_x = tables[k].ConvertAll(x => x.Item1);
                List<float> j_y = tables[k].ConvertAll(x => x.Item2);
                j_x = j_x.ConvertAll(j => Scale(j, scaling_factor_x));
                j_y = j_y.ConvertAll(j => Scale(Math.Abs(Translate(-img_y, j)), scaling_factor_y));

                tables_new[(x1, y1, x2, y2)] = j_x.Zip(j_y, (x, y) => (x, y)).ToList();
            }

            List<(float, float, float, float)> v_segments_new = new List<(float, float, float, float)>();
            foreach (var v in v_segments)
            {
                (var x1, var x2) = (Scale(v.Item1, scaling_factor_x), Scale(v.Item3, scaling_factor_x));
                (var y1, var y2) = (Scale(Math.Abs(Translate(-img_y, v.Item2)), scaling_factor_y), Scale(Math.Abs(Translate(-img_y, v.Item4)), scaling_factor_y));
                v_segments_new.Add((x1, y1, x2, y2));
            }

            List<(float, float, float, float)> h_segments_new = new List<(float, float, float, float)>();
            foreach (var h in h_segments)
            {
                (var x1, var x2) = (Scale(h.Item1, scaling_factor_x), Scale(h.Item3, scaling_factor_x));
                (var y1, var y2) = (Scale(Math.Abs(Translate(-img_y, h.Item2)), scaling_factor_y), Scale(Math.Abs(Translate(-img_y, h.Item4)), scaling_factor_y));
                h_segments_new.Add((x1, y1, x2, y2));
            }
            return (tables_new, v_segments_new, h_segments_new);
        }

        /// <summary>
        /// Detects if text in table is rotated or not using the current
        /// transformation matrix(CTM) and returns its orientation.
        /// </summary>
        /// <param name="chars">List of PDFMiner LTChar objects.</param>
        /// <param name="horizontal_text">List of PDFMiner LTTextLineHorizontal objects.</param>
        /// <param name="vertical_text">List of PDFMiner LTTextLineVertical objects.</param>
        /// <returns>if text in table is upright, 'anticlockwise' if
        /// rotated 90 degree anticlockwise and 'clockwise' if
        /// rotated 90 degree clockwise.</returns>
        public static string GetRotation(List<object> chars, List<object> horizontal_text, List<object> vertical_text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all line segments present inside a bounding box.
        /// </summary>
        /// <param name="bbox">Tuple (x1, y1, x2, y2) representing a bounding box where
        /// (x1, y1) -> lb and(x2, y2) -> rt in PDFMiner coordinate space.</param>
        /// <param name="v_segments">List of vertical line segments.</param>
        /// <param name="h_segments">List of vertical horizontal segments.</param>
        /// <returns>v_s : list - List of vertical line segments that lie inside table.
        /// h_s : list - List of horizontal line segments that lie inside table.</returns>
        public static (List<(float, float, float, float)> v_s, List<(float, float, float, float)> h_s) SegmentsInBbox((float, float, float, float) bbox, List<(float, float, float, float)> v_segments, List<(float, float, float, float)> h_segments)
        {
            var lb = (bbox.Item1, bbox.Item2);
            var rt = (bbox.Item3, bbox.Item4);

            var v_s = v_segments.Where(v => v.Item2 > lb.Item2 - 2 && v.Item4 < rt.Item2 + 2 && lb.Item1 - 2 <= v.Item1 && v.Item1 <= rt.Item1 + 2).ToList();
            var h_s = h_segments.Where(h => h.Item1 > lb.Item1 - 2 && h.Item3 < rt.Item1 + 2 && lb.Item2 - 2 <= h.Item2 && h.Item2 <= rt.Item2 + 2).ToList();
            return (v_s, h_s);
        }

        /// <summary>
        /// Returns all text objects present inside a bounding box.
        /// </summary>
        /// <param name="bbox">Tuple (x1, y1, x2, y2) representing a bounding box where
        /// (x1, y1) -> lb and(x2, y2) -> rt in the PDF coordinate space.</param>
        /// <param name="text">List of PDFMiner text objects.</param>
        /// <returns>List of PDFMiner text objects that lie inside table.</returns>
        public static List<TextLine> TextInBbox((float x1, float y1, float x2, float y2) bbox, List<TextLine> text)
        {
            if (text.Count == 0) return text;

            var t_bbox = text.Where(t => bbox.x1 - 2f <= (t.X0() + t.X1()) / 2f && (t.X0() + t.X1()) / 2f <= bbox.x2 + 2 && bbox.y1 - 2f <= (t.Y0() + t.Y1()) / 2f && (t.Y0() + t.Y1()) / 2f <= bbox.y2 + 2f).ToList();

            // Avoid duplicate text by discarding overlapping boxes
            var rest = t_bbox.Distinct().ToList();
            foreach (var ba in t_bbox)
            {
                foreach (var bb in rest.ToList())
                {
                    if (ba.Equals(bb))
                    {
                        continue;
                    }

                    if (BboxIntersect(ba, bb))
                    {
                        // if the intersection is larger than 80% of ba's size, we keep the longest
                        if ((BboxIntersectionArea(ba, bb) / BboxArea(ba)) > 0.8f)
                        {
                            if (BboxLonger(bb, ba))
                            {
                                rest.Remove(ba);
                            }
                        }
                    }
                }
            }

            return rest;
        }

        /// <summary>
        /// Returns area of the intersection of the bounding boxes of two PDFMiner objects.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>Area of the intersection of the bounding boxes of both objects</returns>
        public static float BboxIntersectionArea(TextLine ba, TextLine bb)
        {
            var x_left = Math.Max(ba.X0(), bb.X0());
            var y_top = Math.Min(ba.Y1(), bb.Y1());
            var x_right = Math.Min(ba.X1(), bb.X1());
            var y_bottom = Math.Max(ba.Y0(), bb.Y0());

            if (x_right < x_left || y_bottom > y_top)
            {
                return 0f;
            }

            return (x_right - x_left) * (y_top - y_bottom);
        }

        /// <summary>
        /// Returns area of the bounding box of a PDFMiner object.
        /// </summary>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>Area of the bounding box of the object</returns>
        public static float BboxArea(TextLine bb)
        {
            return (float)bb.BoundingBox.Area;
        }

        /// <summary>
        /// Returns True if the bounding boxes of two PDFMiner objects intersect.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding boxes intersect</returns>
        public static bool BboxIntersect(TextLine ba, TextLine bb)
        {
            return BboxIntersect(ba.BoundingBox, bb.BoundingBox);
        }

        /// <summary>
        /// Returns True if the bounding boxes of two PDFMiner objects intersect.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding boxes intersect</returns>
        public static bool BboxIntersect(PdfRectangle ba, PdfRectangle bb)
        {
            return BboxIntersect((ba.X0(), ba.Y0(), ba.X1(), ba.Y1()), (bb.X0(), bb.Y0(), bb.X1(), bb.Y1()));
        }

        /// <summary>
        /// Returns True if the bounding boxes of two PDFMiner objects intersect.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding boxes intersect</returns>
        public static bool BboxIntersect((float x0, float y0, float x1, float y1) ba, (float x0, float y0, float x1, float y1) bb)
        {
            return ba.x1 >= bb.x0 && bb.x1 >= ba.x0 && ba.y1 >= bb.y0 && bb.y1 >= ba.y0;
        }

        /// <summary>
        /// Returns True if the bounding box of the first PDFMiner object is longer or equal to the second.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding box of the first object is longer or equal</returns>
        public static bool BboxLonger(PdfRectangle ba, PdfRectangle bb)
        {
            return (ba.X1() - ba.X0()) >= (bb.X1() - bb.X0());
        }

        /// <summary>
        /// Returns True if the bounding box of the first PDFMiner object is longer or equal to the second.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding box of the first object is longer or equal</returns>
        public static bool BboxLonger(TextLine ba, TextLine bb)
        {
            return BboxLonger(ba.BoundingBox, bb.BoundingBox);
        }

        /// <summary>
        /// Merges lines which are within a tolerance by calculating a
        /// moving mean, based on their x or y axis projections.
        /// </summary>
        /// <param name="ar">list</param>
        /// <param name="line_tol">line_tol : int, optional (default: 2)</param>
        /// <returns>list</returns>
        public static List<float> MergeCloseLines(IEnumerable<float> ar, int line_tol = 2)
        {
            List<float> ret = new List<float>();
            foreach (var a in ar)
            {
                if (ret.Count == 0)
                {
                    ret.Add(a);
                }
                else
                {
                    float temp = ret.Last(); // temp = ret[-1]
                    if (MathExtensions.AlmostEquals(temp, a, line_tol))
                    {
                        temp = (temp + a) / 2f;
#pragma warning disable IDE0056 // Use index operator
                        ret[ret.Count - 1] = temp;
#pragma warning restore IDE0056 // Use index operator
                    }
                    else
                    {
                        ret.Add(a);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Strips any characters in `strip` that are present in `text`.
        /// </summary>
        /// <param name="text">Text to process and strip.</param>
        /// <param name="strip">Characters that should be stripped from `text`. optional (default: '')</param>
        /// <returns>str</returns>
        public static string TextStrip(string text, string strip = "")
        {
            if (string.IsNullOrEmpty(strip))
            {
                return text;
            }

            return Regex.Replace(text, $"[{Regex.Escape(strip)}]", "");
        }

        /*
         * TODO: combine the following functions into a TextProcessor class which
         * applies corresponding transformations sequentially
         * (inspired from sklearn.pipeline.Pipeline)
         */

        /// <summary>
        /// Flags super/subscripts in text by enclosing them with &lt;s&gt;&lt;/s&gt;.
        /// May give false positives.
        /// </summary>
        /// <param name="textline">List of PDFMiner LTChar objects.</param>
        /// <param name="direction">Direction of the PDFMiner LTTextLine object.</param>
        /// <param name="strip_text">Characters that should be stripped from a string before
        /// assigning it to a cell.</param>
        /// <returns>string</returns>
        public static string FlagFontSize(List<Letter> textline, string direction, string strip_text = "")
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Splits PDFMiner LTTextLine into substrings if it spans across
        /// multiple rows/columns.
        /// </summary>
        /// <param name="table">camelot.core.Table</param>
        /// <param name="textline">PDFMiner LTTextLine object.</param>
        /// <param name="direction">Direction of the PDFMiner LTTextLine object.</param>
        /// <param name="flag_size">Whether or not to highlight a substring using &lt;s&gt;&lt;/s&gt;
        /// if its size is different from rest of the string. (Useful for
        /// super and subscripts.)</param>
        /// <param name="strip_text">Characters that should be stripped from a string before
        /// assigning it to a cell.</param>
        /// <returns>List of tuples of the form (idx, text) where idx is the index
        /// of row/column and text is the an lttextline substring.</returns>
        public static List<(int, int, string)> SplitTextline(Table table, TextLine textline, string direction, bool flag_size = false, string strip_text = "")
        {
            var cut_text = new List<(int, int, object)>();
            var bbox = textline.Bbox();

            try
            {
                if (direction == "horizontal" && !textline.IsEmpty())
                {
                    var x_overlap = table.Cols.Select((x, i) => (x, i)).Where(k => k.x.Item1 <= bbox[2] && bbox[0] <= k.x.Item2).Select(k => k.i).ToArray();
                    var r_idx = table.Rows.Select((_r, j) => (_r, j)).Where(k => k._r.Item2 <= (bbox[1] + bbox[3]) / 2 && (bbox[1] + bbox[3]) / 2 <= k._r.Item1).Select(k => k.j).ToArray();
                    var r = r_idx[0];
                    var x_cuts = x_overlap.Where(c => table.Cells[r][c].Right).Select(c => (c, table.Cells[r][c].X2)).ToList();
                    if (x_cuts.Count == 0)
                    {
                        x_cuts = new List<(int c, float x2)>() { (x_overlap[0], table.Cells[r].Last().X2) };
                    }

                    foreach (object obj in textline.Objs())
                    {
                        var row = table.Rows[r];
                        foreach (var cut in x_cuts)
                        {
                            if (obj is Letter l)
                            {
                                if (row.Item2 <= (l.Y0() + l.Y1()) / 2f && (l.Y0() + l.Y1()) / 2f <= row.Item1 &&
                                    (l.X0() + l.X1()) / 2f <= cut.X2)
                                {
                                    cut_text.Add((r, cut.c, obj));
                                    break;
                                }
                                else
                                {
                                    // TODO: add test
                                    if (cut == x_cuts.Last())
                                    {
                                        cut_text.Add((r, cut.c + 1, obj));
                                    }
                                }
                            }
                            else if (obj.GetType().Equals(typeof(LTAnno)))
                            {
                                cut_text.Add((r, cut.c, obj));
                            }
                        }
                    }
                }
                else if (direction == "vertical" && !textline.IsEmpty())
                {
                    var y_overlap = table.Rows.Select((y, j) => (y, j)).Where(k => k.y.Item2 <= bbox[3] && bbox[1] <= k.y.Item1).Select(k => k.j).ToArray();
                    var c_idx = table.Cols.Select((_c, i) => (_c, i)).Where(k => k._c.Item1 <= (bbox[0] + bbox[2]) / 2 && (bbox[0] + bbox[2]) / 2 <= k._c.Item2).Select(k => k.i).ToArray();
                    var c = c_idx[0];
                    var y_cuts = y_overlap.Where(r => table.Cells[r][c].Bottom).Select(r => (r, table.Cells[r][c].Y1)).ToList();
                    if (y_cuts.Count == 0)
                    {
                        y_cuts = new List<(int r, float y1)>() { (y_overlap[0], table.Cells[-1][c].Y1) };
                    }

                    foreach (object obj in textline.Objs())
                    {
                        var col = table.Cols[c];
                        foreach (var cut in y_cuts)
                        {
                            if (obj is Letter l)
                            {
                                if (col.Item1 <= (l.X0() + l.X1()) / 2f && (l.X0() + l.X1()) / 2f <= col.Item2 &&
                                    (l.Y0() + l.Y1()) / 2f >= cut.Y1)
                                {
                                    cut_text.Add((cut.r, c, obj));
                                    break;
                                }
                                else
                                {
                                    // TODO: add test
                                    if (cut == y_cuts.Last())
                                    {
                                        cut_text.Add((cut.r - 1, c, obj));
                                    }
                                }
                            }
                            else if (obj.GetType().Equals(typeof(LTAnno)))
                            {
                                cut_text.Add((cut.r, c, obj));
                            }
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                return new List<(int, int, string)>() { (-1, -1, textline.Text) };
            }

            var grouped_chars = new List<(int, int, string)>();
            foreach (var chars in cut_text.GroupBy(ct => (ct.Item1, ct.Item2)))
            {
                if (flag_size)
                {
                    grouped_chars.Add((chars.Key.Item1, chars.Key.Item2, FlagFontSize(chars.Select(t => (Letter)t.Item3).ToList(), direction, strip_text)));
                }
                else
                {
                    var gchars = chars.Select(t => ((Letter)t.Item3).Value);
                    grouped_chars.Add((chars.Key.Item1, chars.Key.Item2, TextStrip(string.Concat(gchars), strip_text)));
                }
            }
            return grouped_chars;
        }

        /// <summary>
        /// Gets indices of the table cell where given text object lies by comparing their y and x-coordinates.
        /// </summary>
        /// <param name="table">camelot.core.Table</param>
        /// <param name="t">PDFMiner LTTextLine object.</param>
        /// <param name="direction">Direction of the PDFMiner LTTextLine object.</param>
        /// <param name="split_text">Whether or not to split a text line if it spans across multiple cells.</param>
        /// <param name="flag_size">Whether or not to highlight a substring using &lt;s&gt;&lt;/s&gt;
        /// if its size is different from rest of the string. (Useful for super and subscripts)</param>
        /// <param name="strip_text">Characters that should be stripped from a string before
        /// assigning it to a cell.</param>
        /// <returns>indices : list - List of tuples of the form (r_idx, c_idx, text) where r_idx and c_idx are row and column indices.
        /// <para>float - Assignment error, percentage of text area that lies outside a cell.</para>
        /// <para>+-------+</para>
        /// <para>|       |</para>
        /// <para>|   [Text bounding box]</para>
        /// <para>|       |</para>
        /// <para>+-------+</para></returns>
        public static (List<(int r_idx, int c_idx, string text)> indices, float error) GetTableIndex(Table table, TextLine t, string direction, bool split_text = false, bool flag_size = false, string strip_text = "", ILog log = null)
        {
            int r_idx = -1;
            int c_idx = -1;
            for (int r = 0; r < table.Rows.Count; r++)
            {
                if ((t.Y0() + t.Y1()) / 2.0f < table.Rows[r].Item1 && (t.Y0() + t.Y1()) / 2.0f > table.Rows[r].Item2)
                {
                    var lt_col_overlap = new List<float>();
                    foreach (var c in table.Cols)
                    {
                        if (c.Item1 <= t.X1() && c.Item2 >= t.X0())
                        {
                            var left = c.Item1 <= t.X0() ? t.X0() : c.Item1;
                            var right = c.Item2 >= t.X1() ? t.X1() : c.Item2;
                            lt_col_overlap.Add(Math.Abs(left - right) / Math.Abs(c.Item1 - c.Item2));
                        }
                        else
                        {
                            lt_col_overlap.Add(-1);
                        }
                    }

                    if (lt_col_overlap.Count(x => x != -1) == 0)
                    {
                        var text = t.Text.Trim('\n');
                        var text_range = (t.X0(), t.X1());
                        var col_range = (table.Cols[0].Item1, table.Cols.Last().Item2);
                        log?.Warn($"{text} {text_range} does not lie in column range {col_range}");
                    }
                    r_idx = r;
                    c_idx = lt_col_overlap.IndexOf(lt_col_overlap.Max());
                    break;
                }
            }

            // error calculation
            float y0_offset = 0f;
            float y1_offset = 0f;
            float x0_offset = 0f;
            float x1_offset = 0f;
            if (t.Y0() > table.Rows[r_idx].Item1)
            {
                y0_offset = Math.Abs(t.Y0() - table.Rows[r_idx].Item1);
            }

            if (t.Y1() < table.Rows[r_idx].Item2)
            {
                y1_offset = Math.Abs(t.Y1() - table.Rows[r_idx].Item2);
            }

            if (t.X0() < table.Cols[c_idx].Item1)
            {
                x0_offset = Math.Abs(t.X0() - table.Cols[c_idx].Item1);
            }

            if (t.X1() > table.Cols[c_idx].Item2)
            {
                x1_offset = Math.Abs(t.X1() - table.Cols[c_idx].Item2);
            }

            float dX = Math.Abs(t.X0() - t.X1());
            var X = dX == 0.0 ? 1.0f : dX;
            float dY = Math.Abs(t.Y0() - t.Y1());
            var Y = dY == 0.0 ? 1.0f : dY;
            float charea = X * Y;
            float error = ((X * (y0_offset + y1_offset)) + (Y * (x0_offset + x1_offset))) / charea;

            if (split_text)
            {
                return (SplitTextline(table, t, direction, flag_size: flag_size, strip_text: strip_text), error);
            }
            else
            {
                if (flag_size)
                {
                    return (new List<(int r_idx, int c_idx, string text)>()
                    {
                        (r_idx,
                        c_idx,
                        FlagFontSize(t.Objs().ToList(), direction, strip_text:strip_text))
                    },
                    error);
                }
                else
                {
                    return (new List<(int r_idx, int c_idx, string text)>()
                    {
                        (r_idx,
                        c_idx,
                        TextStrip(t.Text, strip_text))
                    },
                    error);
                }
            }
        }

        /// <summary>
        /// Calculates a score based on weights assigned to various parameters and their error percentages.
        /// </summary>
        /// <param name="error_weights">Two-dimensional list of the form [[p1, e1], [p2, e2], ...]
        /// where pn is the weight assigned to list of errors en.
        /// Sum of pn should be equal to 100.</param>
        /// <returns>score : float</returns>
        public static float ComputeAccuracy((float, IReadOnlyList<float>)[] error_weights)
        {
            //float SCORE_VAL = 100;
            float score = 0;

            try
            {
                if (error_weights.Sum(ew => ew.Item1) != 100) // SCORE_VAL)
                {
                    throw new ArgumentException("Sum of weights should be equal to 100.");
                }

                foreach (var ew in error_weights)
                {
                    var weight = ew.Item1 / ew.Item2.Count;
                    foreach (var error_percentage in ew.Item2)
                    {
                        score += weight * (1f - error_percentage);
                    }
                }
            }
            catch (DivideByZeroException)
            {
                score = 0;
            }
            return score;
        }

        /// <summary>
        /// Calculates the percentage of empty strings in a
        /// two-dimensional list.
        /// </summary>
        /// <param name="d">list</param>
        /// <returns>Percentage of empty cells.</returns>
        public static float ComputeWhitespace(IReadOnlyList<IReadOnlyList<string>> d)
        {
            float whitespace = 0;

            foreach (var i in d)
            {
                foreach (var j in i)
                {
                    if (j.Trim()?.Length == 0)
                    {
                        whitespace++;
                    }
                }
            }

            return 100f * (whitespace / (d.Count * d[0].Count));
        }

        /// <summary>
        /// Returns a PDFMiner LTPage object and page dimension of a single
        /// page pdf. See https://euske.github.io/pdfminer/ to get definitions
        /// of kwargs.
        /// </summary>
        /// <param name="filename">Path to pdf file.</param>
        /// <param name="char_margin"></param>
        /// <param name="line_margin"></param>
        /// <param name="word_margin"></param>
        /// <param name="detect_vertical"></param>
        /// <param name="all_texts"></param>
        /// <returns>layout : object - PDFMiner LTPage object.
        /// <para>dim : tuple - Dimension of pdf page in the form (width, height).</para></returns>
        public static (Page layout, (int width, int height) dim) GetPageLayout(string filename, float char_margin = 1.0f, float line_margin = 0.5f, float word_margin = 0.1f, bool detect_vertical = true, bool all_texts = true)
        {
            Page page;
            using (PdfDocument document = PdfDocument.Open(filename))
            {
                page = document.GetPage(1);
            }

            //return (page, (page.Width, page.Height));
            throw new NotImplementedException();
        }

        /// <summary>
        /// Recursively parses pdf layout to get a list of PDFMiner text objects.
        /// </summary>
        /// <param name="layout">PDFMiner LTPage object.</param>
        /// <param name="ltype">Specify 'char', 'lh', 'lv' to get LTChar, LTTextLineHorizontal, and LTTextLineVertical objects respectively.</param>
        /// <param name="t">list</param>
        /// <returns>List of PDFMiner text objects.</returns>
        public static List<object> GetTextObjects(object layout, string ltype = "char", List<object> t = null)
        {
            throw new NotImplementedException();
        }
    }
}
