using Camelot.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
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
        public List<(float x1, float y1, float x2, float y2)> table_regions { get; }

        /// <summary>
        /// List of table area strings of the form x1,y1,x2,y2 where(x1, y1) -> left-top and(x2, y2) -> right-bottom in PDF coordinate space.
        /// </summary>
        public List<(float x1, float y1, float x2, float y2)> table_areas { get; }

        /// <summary>
        /// Process background lines.
        /// </summary>
        public bool process_background { get; }

        /// <summary>
        /// Line size scaling factor. The larger the value the smaller the detected lines. Making it very large will lead to text being detected as lines.
        /// </summary>
        public int line_scale { get; }

        /// <summary>
        /// Direction in which text in a spanning cell will be copied over.
        /// <para>{'h', 'v'}</para>
        /// </summary>
        public List<string> copy_text { get; }

        /// <summary>
        /// Direction in which text in a spanning cell will flow.
        /// <para>{'l', 'r', 't', 'b'}</para>
        /// </summary>
        public string[] shift_text { get; }

        /// <summary>
        /// Split text that spans across multiple cells.
        /// </summary>
        public bool split_text { get; }

        /// <summary>
        /// Flag text based on font size. Useful to detect super/subscripts. Adds &lt;s&gt;&lt;/s&gt; around flagged text.
        /// </summary>
        public bool flag_size { get; }

        /// <summary>
        /// Characters that should be stripped from a string before assigning it to a cell.
        /// </summary>
        public string strip_text { get; }

        /// <summary>
        /// Tolerance parameter used to merge close vertical and horizontal lines.
        /// </summary>
        public int line_tol { get; }

        /// <summary>
        /// Tolerance parameter used to decide whether the detected lines and points lie close to each other.
        /// </summary>
        public int joint_tol { get; }

        /// <summary>
        /// Size of a pixel neighborhood that is used to calculate a threshold value for the pixel: 3, 5, 7, and so on.
        /// </summary>
        public int threshold_blocksize { get; }

        /// <summary>
        /// Constant subtracted from the mean or weighted mean. Normally, it is positive but may be zero or negative as well.
        /// </summary>
        public int threshold_constant { get; }

        /// <summary>
        /// Number of times for erosion/dilation is applied.
        /// </summary>
        public int iterations { get; }

        /// <summary>
        /// Resolution used for PDF to PNG conversion.
        /// </summary>
        public int resolution { get; }

        /// <summary>
        /// 
        /// </summary>
        public IImageProcesser imageProcesser { get; }

        /// <summary>
        /// 
        /// </summary>
        public IDrawingProcessor drawingProcessor { get; }

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
                       params string[] kwargs)
        {
            this.imageProcesser = imageProcesser;
            this.drawingProcessor = drawingProcessor;

            this.table_regions = table_regions;
            this.table_areas = table_areas;
            this.process_background = process_background;
            this.line_scale = line_scale;
            this.copy_text = copy_text;
            this.shift_text = shift_text == null ? new[] { "l", "t" } : shift_text;
            this.split_text = split_text;
            this.flag_size = flag_size;
            this.strip_text = strip_text;
            this.line_tol = line_tol;
            this.joint_tol = joint_tol;
            this.threshold_blocksize = threshold_blocksize;
            this.threshold_constant = threshold_constant;
            this.iterations = iterations;
            this.resolution = resolution;
        }

        /// <summary>
        /// Reduces index of a text object if it lies within a spanning cell.
        /// </summary>
        /// <param name="t">table : camelot.core.Table</param>
        /// <param name="idx">list - List of tuples of the form(r_idx, c_idx, text).</param>
        /// <param name="shift_text">list - {'l', 'r', 't', 'b'}
        /// Select one or more strings from above and pass them as a list to specify where the text in a spanning cell should flow.</param>
        /// <returns>List of tuples of the form (r_idx, c_idx, text) where r_idx and c_idx are new row and column indices for text.</returns>
        public static List<(int r_idx, int c_idx, string text)> _reduce_index(Table t, List<(int r_idx, int c_idx, string text)> idx, string[] shift_text)
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
                            if (t.cells[r_idx][c_idx].hspan)
                            {

                                while (!t.cells[r_idx][c_idx_local].left)
                                {
                                    c_idx_local -= 1;
                                }
                            }
                            break;

                        case "r":
                            if (t.cells[r_idx][c_idx].hspan)
                            {
                                while (!t.cells[r_idx][c_idx_local].right)
                                {
                                    c_idx_local += 1;
                                }
                            }
                            break;

                        case "t":
                            if (t.cells[r_idx][c_idx].vspan)
                            {
                                while (!t.cells[r_idx_local][c_idx].top)
                                {
                                    r_idx_local -= 1;
                                }
                            }
                            break;

                        case "b":
                            if (t.cells[r_idx][c_idx].vspan)
                            {
                                while (!t.cells[r_idx_local][c_idx].bottom)
                                {
                                    r_idx_local += 1;
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
        public static Table _copy_spanning_text(Table t, List<string> copy_text = null)
        {
            foreach (var f in copy_text)
            {
                if (f == "h")
                {
                    for (int i = 0; i < t.cells.Count; i++)
                    {
                        for (int j = 0; j < t.cells[i].Count; j++)
                        {
                            if (t.cells[i][j].text.Trim() == "")
                            {
                                if (t.cells[i][j].hspan && !t.cells[i][j].left)
                                {
                                    t.cells[i][j].text = t.cells[i][j - 1].text;
                                }
                            }
                        }
                    }
                }
                else if (f == "v")
                {
                    for (int i = 0; i < t.cells.Count; i++)
                    {
                        for (int j = 0; j < t.cells[i].Count; j++)
                        {
                            if (t.cells[i][j].text.Trim() == "")
                            {
                                if (t.cells[i][j].vspan && !t.cells[i][j].top)
                                {
                                    t.cells[i][j].text = t.cells[i - 1][j].text;
                                }
                            }
                        }
                    }
                }
            }

            return t;
        }

        public void _generate_image()
        {
            // from ..ext.ghostscript import Ghostscript
        }

        byte[] image;
        byte[] threshold;

        Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox_unscaled;
        List<(float, float, float, float)> vertical_segments;
        List<(float, float, float, float)> horizontal_segments;

        public void _generate_table_bbox()
        {
            (this.table_bbox, this.vertical_segments, this.horizontal_segments) = imageProcesser.Process(
                this.layout,
                this.drawingProcessor,
                this.process_background,
                this.threshold_blocksize,
                this.threshold_constant,
                this.line_scale,
                this.iterations,
                this.table_areas,
                this.table_regions,
                out this.table_bbox_unscaled);
        }

        public void _generate_table_bbox_old()
        {
            List<(int, int, int, int)> scale_areas(List<(float x1, float y1, float x2, float y2)> areas, (float, float, float) img_scalers)
            {
                var scaled_areas = new List<(int, int, int, int)>();
                foreach (var area in areas)
                {
                    (int x1_s, int y1_s, int x2_s, int y2_s) = Utils.scale_pdf(area, img_scalers);
                    scaled_areas.Add((x1_s, y1_s, Math.Abs(x2_s - x1_s), Math.Abs(y2_s - y1_s)));
                }
                return scaled_areas;
            }

            (this.image, this.threshold) = imageProcesser.adaptive_threshold(
                this.image,
                process_background: this.process_background,
                blocksize: this.threshold_blocksize,
                c: this.threshold_constant);

            (int image_width, int image_height, int _) = imageProcesser.GetImageShape(this.image);
            var image_width_scaler = image_width / (float)this.pdf_width;
            var image_height_scaler = image_height / (float)this.pdf_height;
            var pdf_width_scaler = this.pdf_width / (float)image_width;
            var pdf_height_scaler = this.pdf_height / (float)image_height;
            var image_scalers = (image_width_scaler, image_height_scaler, this.pdf_height);
            var pdf_scalers = (pdf_width_scaler, pdf_height_scaler, image_height);

            List<(int, int, int, int)> vertical_segments, horizontal_segments;
            byte[] vertical_mask, horizontal_mask;
            if (this.table_areas == null || this.table_areas.Count == 0)
            {
                List<(int, int, int, int)> regions = null;
                if (this.table_regions != null && this.table_regions.Count > 0)
                {
                    regions = scale_areas(this.table_regions, image_scalers);
                }

                (vertical_mask, vertical_segments) = imageProcesser.find_lines(
                    this.threshold,
                    regions: regions,
                    direction: "vertical",
                    line_scale: this.line_scale,
                    iterations: this.iterations);

                (horizontal_mask, horizontal_segments) = imageProcesser.find_lines(
                    this.threshold,
                    regions: regions,
                    direction: "horizontal",
                    line_scale: this.line_scale,
                    iterations: this.iterations);

                var contours = imageProcesser.find_contours(vertical_mask, horizontal_mask);
                table_bbox = imageProcesser.find_joints(contours, vertical_mask, horizontal_mask);
            }
            else
            {
                (vertical_mask, vertical_segments) = imageProcesser.find_lines(
                    this.threshold,
                    direction: "vertical",
                    line_scale: this.line_scale,
                    iterations: this.iterations);

                (horizontal_mask, horizontal_segments) = imageProcesser.find_lines(
                    this.threshold,
                    direction: "horizontal",
                    line_scale: this.line_scale,
                    iterations: this.iterations);

                var areas = scale_areas(this.table_areas, image_scalers);
                table_bbox = imageProcesser.find_joints(areas, vertical_mask, horizontal_mask);
            }

            this.table_bbox_unscaled = new Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>>(table_bbox); // copy.deepcopy(table_bbox);

            (this.table_bbox, this.vertical_segments, this.horizontal_segments) = Utils.scale_image(
                table_bbox,
                vertical_segments,
                horizontal_segments,
                pdf_scalers);
        }

        Dictionary<string, List<TextLine>> t_bbox;

        public (List<(float, float)> cols, List<(float, float)> rows, List<(float, float, float, float)> v_s, List<(float, float, float, float)> h_s) _generate_columns_and_rows(int table_idx, (float, float, float, float) tk)
        {
            // select elements which lie within table_bbox
            Dictionary<string, List<TextLine>> t_bbox = new Dictionary<string, List<TextLine>>();
            (var v_s, var h_s) = Utils.segments_in_bbox(tk, this.vertical_segments, this.horizontal_segments);
            t_bbox["horizontal"] = Utils.text_in_bbox(tk, this.horizontal_text);
            t_bbox["vertical"] = Utils.text_in_bbox(tk, this.vertical_text);

            t_bbox["horizontal"] = t_bbox["horizontal"].OrderBy(x => -x.y0()).ThenBy(x => x.x0()).ToList();
            t_bbox["vertical"] = t_bbox["vertical"].OrderBy(x => x.x0()).ThenBy(x => -x.y0()).ToList();

            this.t_bbox = t_bbox;
            //cols, rows = zip(*self.table_bbox[tk])
            //cols, rows = list(cols), list(rows)
            var cols = this.table_bbox[tk].Select(x => (float)x.Item1).ToList();
            var rows = this.table_bbox[tk].Select(x => (float)x.Item2).ToList();

            cols.AddRange(new[] { tk.Item1, tk.Item3 });
            rows.AddRange(new[] { tk.Item2, tk.Item4 });
            // sort horizontal and vertical segments
            cols = Utils.merge_close_lines(cols.OrderBy(r => r), line_tol: this.line_tol);
            rows = Utils.merge_close_lines(rows.OrderByDescending(c => c), line_tol: this.line_tol);
            // make grid using x and y coord of shortlisted rows and cols
            //cols = [(cols[i], cols[i + 1]) for i in range(0, len(cols) - 1)]
            var colsT = Enumerable.Range(0, cols.Count - 1).Select(i => (cols[i], cols[i + 1])).ToList();

            //rows = [(rows[i], rows[i + 1]) for i in range(0, len(rows) - 1)]
            var rowsT = Enumerable.Range(0, rows.Count - 1).Select(i => (rows[i], rows[i + 1])).ToList();
            return (colsT, rowsT, v_s, h_s);
        }

        /// <summary>
        /// TODO: BobLD - have a single function for stream and lattice
        /// </summary>
        public Table _generate_table(int table_idx, List<(float, float)> cols, List<(float, float)> rows,
            List<(float, float, float, float)> v_s = null, List<(float, float, float, float)> h_s = null)
        {
            if (v_s == null || h_s == null)
            {
                throw new ArgumentNullException($"No segments found on {this.rootname}");
            }

            var table = new Table(cols, rows);
            // set table edges to True using ver+hor lines
            table = table.set_edges(v_s, h_s, joint_tol: this.joint_tol);
            // set table border edges to True
            table = table.set_border();
            // set spanning cells to True
            table = table.set_span();

            var pos_errors = new List<float>();
            // TODO: have a single list in place of two directional ones?
            // sorted on x-coordinate based on reading order i.e. LTR or RTL

            foreach (string direction in new[] { "vertical", "horizontal" })
            {
                foreach (var t in this.t_bbox[direction])
                {
                    (var indices, var error) = Utils.get_table_index(table,
                        t,
                        direction,
                        split_text: this.split_text,
                        flag_size: this.flag_size,
                        strip_text: this.strip_text);

                    // CAREFUL HERE - Not sure the Python version is correct
                    // indices[:2] != (-1, -1) should always return true 
                    // as indices is [[]] and indices[:2] is 
                    // [[x, y, text0], [x', y', text1]]
                    // SOLUTION: We take the first element to do the check
                    if (indices[0].r_idx != -1 && indices[0].c_idx != -1) // if indices[:2] != (-1, -1):
                    {
                        pos_errors.Add(error);
                        indices = Lattice._reduce_index(table, indices, shift_text: this.shift_text);
                        foreach ((var r_idx, var c_idx, var text) in indices)
                        {
                            table.cells[r_idx][c_idx].text = text + "\n";
                        }
                    }
                }
            }
            var accuracy = Utils.compute_accuracy(new[] { (100f, (IReadOnlyList<float>)pos_errors) });

            if (this.copy_text != null)
            {
                table = _copy_spanning_text(table, copy_text: this.copy_text);
            }

            var data = table.data();
            //table.df = pd.DataFrame(data);
            table.shape = (data.Count, data.Max(r => r.Count));

            var whitespace = Utils.compute_whitespace(data);
            table.flavor = "lattice";
            table.accuracy = accuracy;
            table.whitespace = whitespace;
            table.order = table_idx + 1;
            table.page = -99; //int(os.path.basename(self.rootname).replace("page-", ""));

            // for plotting
            var _text = new List<(float x0, float y0, float x1, float y1)>();
            _text.AddRange(this.horizontal_text.Select(t => (t.x0(), t.y0(), t.x1(), t.y1())));  // _text.extend([(t.x0, t.y0, t.x1, t.y1) for t in self.horizontal_text]) ;
            _text.AddRange(this.vertical_text.Select(t => (t.x0(), t.y0(), t.x1(), t.y1())));   // _text.extend([(t.x0, t.y0, t.x1, t.y1) for t in self.vertical_text]) ;
            table._text = _text;
            table._image = (this.image, this.table_bbox_unscaled);
            table._segments = (this.vertical_segments, this.horizontal_segments);
            table._textedges = null;

            return table;
        }

        Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox;

        public override List<Table> extract_tables(string filename, bool suppress_stdout = false, params DlaOptions[] layout_kwargs)
        {
            base._generate_layout(filename, layout_kwargs);
            var base_filename = Path.GetFileName(this.rootname); //os.path.basename(self.rootname)
            if (!suppress_stdout)
            {
                // logger.info("Processing {}".format(os.path.basename(self.rootname)))
                Debug.Print($"Processing {base_filename}");
            }

            if (base.horizontal_text == null || base.horizontal_text.Count == 0)
            {
                if (base.images.Count > 0)
                {
                    //warnings.warn("{} is image-based, camelot only works on" " text-based pages.".format(os.path.basename(self.rootname)))
                    Debug.Print($"{base_filename} is image-based, camelot only works on text-based pages.");
                }
                else
                {
                    //warnings.warn("No tables found on {}".format(os.path.basename(self.rootname)))
                    Debug.Print($"No tables found on {base_filename}");
                }
                return null;
            }

            this._generate_image();
            this._generate_table_bbox();

            var _tables = new List<Table>();
            // sort tables based on y-coord
            int table_idx = 0;
            foreach (var tk in this.table_bbox.Keys.OrderByDescending(kvp => kvp.Item2))
            {
                (var cols, var rows, var v_s, var h_s) = this._generate_columns_and_rows(table_idx, tk);
                var table = this._generate_table(table_idx, cols, rows, v_s: v_s, h_s: h_s);
                table._bbox = tk;
                _tables.Add(table);
                table_idx++;
            }

            return _tables;
        }
    }
}
