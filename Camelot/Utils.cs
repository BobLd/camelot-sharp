﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
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
        public static bool is_url(string url)
        {
            throw new NotImplementedException();
        }

        public static string random_string(int length)
        {
            throw new NotImplementedException();
        }

        public static string download_url(string url)
        {
            throw new NotImplementedException();
        }

        public static bool validate_input(string[] kwargs, string flavor = "lattice")
        {
            throw new NotImplementedException();
        }

        public static string[] remove_extra(string[] kwargs, string flavor = "lattice")
        {
            throw new NotImplementedException();
        }

        // class TemporaryDirectory(object)

        /// <summary>
        /// Translates x2 by x1.
        /// </summary>
        public static float translate(float x1, float x2)
        {
            x2 += x1;
            return x2;
        }

        /// <summary>
        /// Scales x by scaling factor s.
        /// </summary>
        public static float scale(float x, float s)
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
        public static (int x1, int y1, int x2, int y2) scale_pdf((float x1, float y1, float x2, float y2) k, (float scaling_factor_x, float scaling_factor_y, float pdf_y) factors)
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

            x1 = scale(x1, scaling_factor_x);
            y1 = scale(Math.Abs(translate(-pdf_y, y1)), scaling_factor_y);
            x2 = scale(x2, scaling_factor_x);
            y2 = scale(Math.Abs(translate(-pdf_y, y2)), scaling_factor_y);
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
            scale_image(Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> tables,
            List<(int, int, int, int)> v_segments,
            List<(int, int, int, int)> h_segments,
            (float scaling_factor_x, float scaling_factor_y, float img_y) factors)
        {
            (float scaling_factor_x, float scaling_factor_y, float img_y) = factors;
            var tables_new = new Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>>();
            foreach (var k in tables.Keys)
            {
                (float x1, float y1, float x2, float y2) = k;
                x1 = scale(x1, scaling_factor_x);
                y1 = scale(Math.Abs(translate(-img_y, y1)), scaling_factor_y);
                x2 = scale(x2, scaling_factor_x);
                y2 = scale(Math.Abs(translate(-img_y, y2)), scaling_factor_y);

                var j_x = tables[k].Select(x => (float)x.Item1).ToList();
                var j_y = tables[k].Select(x => (float)x.Item2).ToList();
                j_x = j_x.Select(j => scale(j, scaling_factor_x)).ToList();
                j_y = j_y.Select(j => scale(Math.Abs(translate(-img_y, j)), scaling_factor_y)).ToList();

                tables_new[(x1, y1, x2, y2)] = j_x.Zip(j_y, (x, y) => (x, y)).ToList();
            }

            List<(float, float, float, float)> v_segments_new = new List<(float, float, float, float)>();
            foreach (var v in v_segments)
            {
                (var x1, var x2) = (scale(v.Item1, scaling_factor_x), scale(v.Item3, scaling_factor_x));
                (var y1, var y2) = (scale(Math.Abs(translate(-img_y, v.Item2)), scaling_factor_y), scale(Math.Abs(translate(-img_y, v.Item4)), scaling_factor_y));
                v_segments_new.Add((x1, y1, x2, y2));
            }

            List<(float, float, float, float)> h_segments_new = new List<(float, float, float, float)>();
            foreach (var h in h_segments)
            {
                (var x1, var x2) = (scale(h.Item1, scaling_factor_x), scale(h.Item3, scaling_factor_x));
                (var y1, var y2) = (scale(Math.Abs(translate(-img_y, h.Item2)), scaling_factor_y), scale(Math.Abs(translate(-img_y, h.Item4)), scaling_factor_y));
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
        public static string get_rotation(List<object> chars, List<object> horizontal_text, List<object> vertical_text)
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
        public static (List<float[]> v_s, List<float[]> h_s) segments_in_bbox((float, float, float, float) bbox, List<(float, float, float, float)> v_segments, List<(float, float, float, float)> h_segments)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all text objects present inside a bounding box.
        /// </summary>
        /// <param name="bbox">Tuple (x1, y1, x2, y2) representing a bounding box where
        /// (x1, y1) -> lb and(x2, y2) -> rt in the PDF coordinate space.</param>
        /// <param name="text">List of PDFMiner text objects.</param>
        /// <returns>List of PDFMiner text objects that lie inside table.</returns>
        public static List<TextLine> text_in_bbox((float x1, float y1, float x2, float y2) bbox, List<TextLine> text)
        {
            if (text.Count == 0) return text;

            //var lb = (bbox.x1, bbox.y1);
            //var rt = (bbox.x2, bbox.y2);
            //t_bbox = [t for t in text if lb[0] - 2 <= (t.x0 + t.x1) / 2.0 <= rt[0] + 2 and lb[1] -2 <= (t.y0 + t.y1) / 2.0 <= rt[1] + 2]
            var t_bbox = text.Where(t => bbox.x1 - 2f <= (t.x0() + t.x1()) / 2f && (t.x0() + t.x1()) / 2f <= bbox.x2 + 2 && bbox.y1 - 2f <= (t.y0() + t.y1()) / 2f && (t.y0() + t.y1()) / 2f <= bbox.y2 + 2f).ToList();

            // Avoid duplicate text by discarding overlapping boxes
            var rest = t_bbox.Distinct().ToList(); // rest = {t for t in t_bbox}
            foreach (var ba in t_bbox)
            {
                foreach (var bb in rest.ToList())
                {
                    if (ba.Equals(bb))
                    {
                        continue;
                    }

                    if (bbox_intersect(ba, bb))
                    {
                        // if the intersection is larger than 80% of ba's size, we keep the longest
                        if ((bbox_intersection_area(ba, bb) / bbox_area(ba)) > 0.8f)
                        {
                            if (bbox_longer(bb, ba))
                            {
                                rest.Remove(ba);
                            }
                        }
                    }
                }
            }

            //unique_boxes = list(rest)
            //return unique_boxes
            return rest;
        }

        /// <summary>
        /// Returns area of the intersection of the bounding boxes of two PDFMiner objects.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>Area of the intersection of the bounding boxes of both objects</returns>
        public static float bbox_intersection_area(TextLine ba, TextLine bb)
        {
            var x_left = Math.Max(ba.x0(), bb.x0());
            var y_top = Math.Min(ba.y1(), bb.y1());
            var x_right = Math.Min(ba.x1(), bb.x1());
            var y_bottom = Math.Max(ba.y0(), bb.y0());

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
        public static float bbox_area(object bb)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns True if the bounding boxes of two PDFMiner objects intersect.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding boxes intersect</returns>
        public static bool bbox_intersect(TextLine ba, TextLine bb)
        {
            return bbox_intersect(ba.BoundingBox, bb.BoundingBox);
        }

        /// <summary>
        /// Returns True if the bounding boxes of two PDFMiner objects intersect.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding boxes intersect</returns>
        public static bool bbox_intersect(PdfRectangle ba, PdfRectangle bb)
        {
            return bbox_intersect((ba.x0(), ba.y0(), ba.x1(), ba.y1()), (bb.x0(), bb.y0(), bb.x1(), bb.y1()));
        }

        /// <summary>
        /// Returns True if the bounding boxes of two PDFMiner objects intersect.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding boxes intersect</returns>
        public static bool bbox_intersect((float x0, float y0, float x1, float y1) ba, (float x0, float y0, float x1, float y1) bb)
        {
            return ba.x1 >= bb.x0 && bb.x1 >= ba.x0 && ba.y1 >= bb.y0 && bb.y1 >= ba.y0;
        }

        /// <summary>
        /// Returns True if the bounding box of the first PDFMiner object is longer or equal to the second.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding box of the first object is longer or equal</returns>
        public static bool bbox_longer(PdfRectangle ba, PdfRectangle bb)
        {
            return (ba.x1() - ba.x0()) >= (bb.x1() - bb.x0());
        }

        /// <summary>
        /// Returns True if the bounding box of the first PDFMiner object is longer or equal to the second.
        /// </summary>
        /// <param name="ba">PDFMiner text object</param>
        /// <param name="bb">PDFMiner text object</param>
        /// <returns>True if the bounding box of the first object is longer or equal</returns>
        public static bool bbox_longer(TextLine ba, TextLine bb)
        {
            return bbox_longer(ba.BoundingBox, bb.BoundingBox);
        }

        /// <summary>
        /// Merges lines which are within a tolerance by calculating a
        /// moving mean, based on their x or y axis projections.
        /// </summary>
        /// <param name="ar">list</param>
        /// <param name="line_tol">line_tol : int, optional (default: 2)</param>
        /// <returns>list</returns>
        public static List<float> merge_close_lines(IEnumerable<float> ar, int line_tol = 2)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Strips any characters in `strip` that are present in `text`.
        /// </summary>
        /// <param name="text">Text to process and strip.</param>
        /// <param name="strip">Characters that should be stripped from `text`. optional (default: '')</param>
        /// <returns>str</returns>
        public static string text_strip(string text, string strip = "")
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
        public static string flag_font_size(List<Letter> textline, string direction, string strip_text = "")
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
        public static List<(int, int, string)> split_textline(Table table, TextLine textline, string direction, bool flag_size = false, string strip_text = "")
        {
            //int idx = 0;
            var cut_text = new List<(int, int, object)>();
            var bbox = textline.bbox();

            try
            {
                if (direction == "horizontal" && !textline.is_empty())
                {
                    var x_overlap = table.cols.Select((x, i) => (x, i)).Where(k => k.x.Item1 <= bbox[2] && bbox[0] <= k.x.Item2).Select(k => k.i).ToArray(); //x_overlap = [i for i, x in enumerate(table.cols) if x[0] <= bbox[2] and bbox[0] <= x[1]]
                    var r_idx = table.rows.Select((_r, j) => (_r, j)).Where(k => k._r.Item2 <= (bbox[1] + bbox[3]) / 2 && (bbox[1] + bbox[3]) / 2 <= k._r.Item1).Select(k => k.j).ToArray(); //r_idx = [j for j, r in enumerate(table.rows) if r[1] <= (bbox[1] + bbox[3]) / 2 <= r[0]]
                    var r = r_idx[0];
                    var x_cuts = x_overlap.Where(c => table.cells[r][c].right).Select(c => (c, table.cells[r][c].x2)).ToList(); // x_cuts = [(c, table.cells[r][c].x2) for c in x_overlap if table.cells[r][c].right]
                    if (x_cuts.Count == 0)
                    {
                        x_cuts = new List<(int c, float x2)>() { (x_overlap[0], table.cells[r].Last().x2) }; //[(x_overlap[0], table.cells[r][-1].x2)]
                    }

                    foreach (object obj in textline._objs())
                    {
                        var row = table.rows[r];
                        foreach (var cut in x_cuts)
                        {
                            if (obj is Letter l) // if isinstance(obj, LTChar):
                            {
                                if (row.Item2 <= (l.y0() + l.y1()) / 2f && (l.y0() + l.y1()) / 2f <= row.Item1 &&
                                    (l.x0() + l.x1()) / 2f <= cut.x2)
                                {
                                    cut_text.Add((r, cut.c, obj));
                                    break;
                                }
                                else
                                {
                                    // TODO: add test
                                    if (cut == x_cuts.Last()) //[-1])
                                    {
                                        cut_text.Add((r, cut.c + 1, obj));
                                    }
                                }
                            }
                            else if (obj.GetType().Equals(typeof(LTAnno))) // elif isinstance(obj, LTAnno):
                            {
                                cut_text.Add((r, cut.c, obj));
                            }
                        }
                    }
                }
                else if (direction == "vertical" && !textline.is_empty())
                {
                    var y_overlap = table.rows.Select((y, j) => (y, j)).Where(k => k.y.Item2 <= bbox[3] && bbox[1] <= k.y.Item1).Select(k => k.j).ToArray(); //y_overlap = [j for j, y in enumerate(table.rows) if y[1] <= bbox[3] and bbox[1] <= y[0]]
                    var c_idx = table.cols.Select((_c, i) => (_c, i)).Where(k => k._c.Item1 <= (bbox[0] + bbox[2]) / 2 && (bbox[0] + bbox[2]) / 2 <= k._c.Item2).Select(k => k.i).ToArray(); //c_idx = [i for i, c in enumerate(table.cols) if c[0] <= (bbox[0] + bbox[2]) / 2 <= c[1]]
                    var c = c_idx[0];
                    var y_cuts = y_overlap.Where(r => table.cells[r][c].bottom).Select(r => (r, table.cells[r][c].y1)).ToList(); //y_cuts = [(r, table.cells[r][c].y1) for r in y_overlap if table.cells[r][c].bottom]
                    if (y_cuts.Count == 0)
                    {
                        y_cuts = new List<(int r, float y1)>() { (y_overlap[0], table.cells[-1][c].y1) };
                    }

                    foreach (object obj in textline._objs())
                    {
                        var col = table.cols[c];
                        foreach (var cut in y_cuts)
                        {
                            if (obj is Letter l) // if isinstance(obj, LTChar):
                            {
                                if (col.Item1 <= (l.x0() + l.x1()) / 2f && (l.x0() + l.x1()) / 2f <= col.Item2 &&
                                    (l.y0() + l.y1()) / 2f >= cut.y1)
                                {
                                    cut_text.Add((cut.r, c, obj));
                                    break;
                                }
                                else
                                {
                                    // TODO: add test
                                    if (cut == y_cuts.Last()) //[-1])
                                    {
                                        cut_text.Add((cut.r - 1, c, obj));
                                    }
                                }
                            }
                            else if (obj.GetType().Equals(typeof(LTAnno))) // elif isinstance(obj, LTAnno):
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
                    grouped_chars.Add((chars.Key.Item1, chars.Key.Item2, flag_font_size(chars.Select(t => (Letter)t.Item3).ToList(), direction, strip_text)));
                }
                else
                {
                    var gchars = chars.Select(t => ((Letter)t.Item3).Value);//[t[2].get_text() for t in chars]
                    grouped_chars.Add((chars.Key.Item1, chars.Key.Item2, text_strip(string.Concat(gchars), strip_text)));
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
        public static (List<(int r_idx, int c_idx, string text)> indices, float error) get_table_index(Table table, TextLine t, string direction, bool split_text = false, bool flag_size = false, string strip_text = "")
        {
            int r_idx = -1;
            int c_idx = -1;
            for (int r = 0; r < table.rows.Count; r++)
            {
                if ((t.y0() + t.y1()) / 2.0f < table.rows[r].Item1 && (t.y0() + t.y1()) / 2.0f > table.rows[r].Item2)
                {
                    var lt_col_overlap = new List<float>();
                    foreach (var c in table.cols)
                    {
                        if (c.Item1 <= t.x1() && c.Item2 >= t.x0())
                        {
                            var left = c.Item1 <= t.x0() ? t.x0() : c.Item1;
                            var right = c.Item2 >= t.x1() ? t.x1() : c.Item2;
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
                        var text_range = (t.x0(), t.x1());
                        var col_range = (table.cols[0].Item1, table.cols.Last().Item2); // [-1][1]
                        //warnings.warn(f"{text} {text_range} does not lie in column range {col_range}")
                        Debug.WriteLine($"{text} {text_range} does not lie in column range {col_range}");
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
            if (t.y0() > table.rows[r_idx].Item1)
            {
                y0_offset = Math.Abs(t.y0() - table.rows[r_idx].Item1);
            }

            if (t.y1() < table.rows[r_idx].Item2)
            {
                y1_offset = Math.Abs(t.y1() - table.rows[r_idx].Item2);
            }

            if (t.x0() < table.cols[c_idx].Item1)
            {
                x0_offset = Math.Abs(t.x0() - table.cols[c_idx].Item1);
            }

            if (t.x1() > table.cols[c_idx].Item2)
            {
                x1_offset = Math.Abs(t.x1() - table.cols[c_idx].Item2);
            }

            float dX = Math.Abs(t.x0() - t.x1());
            var X = dX == 0.0 ? 1.0f : dX;
            float dY = Math.Abs(t.y0() - t.y1());
            var Y = dY == 0.0 ? 1.0f : dY;
            float charea = X * Y;
            float error = ((X * (y0_offset + y1_offset)) + (Y * (x0_offset + x1_offset))) / charea;

            if (split_text)
            {
                return (split_textline(table, t, direction, flag_size: flag_size, strip_text: strip_text), error);
            }
            else
            {
                if (flag_size)
                {
                    return (new List<(int r_idx, int c_idx, string text)>()
                    {
                        (r_idx,
                        c_idx,
                        flag_font_size(t._objs().ToList(), direction, strip_text:strip_text))
                    },
                    error);
                }
                else
                {
                    return (new List<(int r_idx, int c_idx, string text)>()
                    {
                        (r_idx,
                        c_idx,
                        text_strip(t.Text, strip_text))
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
        public static float compute_accuracy((float, IReadOnlyList<float>)[] error_weights)
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
        public static float compute_whitespace(IReadOnlyList<IReadOnlyList<string>> d)
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
        public static (Page layout, (int width, int height) dim) get_page_layout(string filename, float char_margin = 1.0f, float line_margin = 0.5f, float word_margin = 0.1f, bool detect_vertical = true, bool all_texts = true)
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
        public static List<object> get_text_objects(object layout, string ltype = "char", List<object> t = null)
        {
            throw new NotImplementedException();
        }
    }
}