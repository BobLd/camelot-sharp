﻿using Camelot.ImageProcessing.OpenCvSharp4;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UglyToad.PdfPig.Content;

namespace Camelot.ImageProcessing
{
    public class OpenCvImageProcesser : IImageProcesser
    {
        private List<(int, int, int, int)> scale_areas(List<string> areas, (float, float, float) img_scalers)
        {
            var scaled_areas = new List<(int, int, int, int)>();
            foreach (var area in areas)
            {
                var coords = area.Split(',');
                float x1 = float.Parse(coords[0]); //float(x1);
                float y1 = float.Parse(coords[1]); //float(y1);
                float x2 = float.Parse(coords[2]); //float(x2);
                float y2 = float.Parse(coords[3]); //float(y2);
                (int x1_s, int y1_s, int x2_s, int y2_s) = Utils.scale_pdf((x1, y1, x2, y2), img_scalers);
                scaled_areas.Add((x1_s, y1_s, Math.Abs(x2_s - x1_s), Math.Abs(y2_s - y1_s)));
            }
            return scaled_areas;
        }

        public (Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox, List<(float, float, float, float)> vertical_segments, List<(float, float, float, float)> horizontal_segments)
            Process(Page page, ImageProcessing.IDrawingProcessor drawingProcessor,
            bool process_background, int threshold_blocksize, int threshold_constant, int line_scale, int iterations,
            List<string> table_areas, List<string> table_regions, out Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox_unscaled)
        {
            Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox;
            Mat vertical_mask;
            List<(int, int, int, int)> vertical_segments;
            Mat horizontal_mask;

            List<(int, int, int, int)> horizontal_segments;

            using (var ms = drawingProcessor.DrawPage(page, 3))
            using (var image = Mat.FromImageData(ms.ToArray()))
            {
                (Mat img, Mat threshold) = adaptive_threshold(
                    image,
                    process_background: process_background,
                    blocksize: threshold_blocksize,
                    c: threshold_constant);

                int[] img_shape = image.shape();
                (int image_width, int image_height) = (img_shape[1], img_shape[0]);
                float image_width_scaler = image_width / (float)page.Width; //this.pdf_width;
                float image_height_scaler = image_height / (float)page.Height; // this.pdf_height;
                float pdf_width_scaler = (float)page.Width / (float)image_width;
                float pdf_height_scaler = (float)page.Height / (float)image_height;
                var image_scalers = (image_width_scaler, image_height_scaler, (float)page.Height);
                var pdf_scalers = (pdf_width_scaler, pdf_height_scaler, image_height);

                if (table_areas == null || table_areas.Count == 0)
                {
                    List<(int, int, int, int)> regions = null;
                    if (table_regions != null && table_regions.Count > 0)
                    {
                        regions = scale_areas(table_regions, image_scalers);
                    }

                    (vertical_mask, vertical_segments) = find_lines(
                        threshold,
                        regions: regions,
                        direction: "vertical",
                        line_scale: line_scale,
                        iterations: iterations);

                    (horizontal_mask, horizontal_segments) = find_lines(
                        threshold,
                        regions: regions,
                        direction: "horizontal",
                        line_scale: line_scale,
                        iterations: iterations);

                    var contours = find_contours(vertical_mask, horizontal_mask);
                    table_bbox = find_joints(contours, vertical_mask, horizontal_mask);
                }
                else
                {
                    (vertical_mask, vertical_segments) = find_lines(
                        threshold,
                        direction: "vertical",
                        line_scale: line_scale,
                        iterations: iterations);

                    (horizontal_mask, horizontal_segments) = find_lines(
                        threshold,
                        direction: "horizontal",
                        line_scale: line_scale,
                        iterations: iterations);

                    var areas = scale_areas(table_areas, image_scalers);
                    table_bbox = find_joints(areas, vertical_mask, horizontal_mask);
                }

                vertical_mask.Dispose();
                horizontal_mask.Dispose();
                img.Dispose();
                threshold.Dispose();

                table_bbox_unscaled = new Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>>(table_bbox); // copy.deepcopy(table_bbox);

                return Utils.scale_image(
                    table_bbox,
                    vertical_segments,
                    horizontal_segments,
                    pdf_scalers);
            }
        }

        /// <summary>
        /// Thresholds an image using OpenCV's adaptiveThreshold.
        /// </summary>
        /// <param name="imagename">Path to image file.</param>
        /// <param name="process_background">Whether or not to process lines that are in background.</param>
        /// <param name="blocksize">Size of a pixel neighborhood that is used to calculate a threshold value for the pixel: 3, 5, 7, and so on.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <param name="c">Constant subtracted from the mean or weighted mean. Normally, it is positive but may be zero or negative as well.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <returns>img : object - numpy.ndarray representing the original image.
        /// <para>threshold : object - numpy.ndarray representing the thresholded image.</para></returns>
        public (Mat img, Mat threshold) adaptive_threshold(string imagename, bool process_background = false, int blocksize = 15, int c = -2)
        {
            return adaptive_threshold(Cv2.ImRead(imagename), process_background, blocksize, c);
        }

        /// <summary>
        /// Thresholds an image using OpenCV's adaptiveThreshold.
        /// </summary>
        /// <param name="image">Image bytes array.</param>
        /// <param name="process_background">Whether or not to process lines that are in background.</param>
        /// <param name="blocksize">Size of a pixel neighborhood that is used to calculate a threshold value for the pixel: 3, 5, 7, and so on.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <param name="c">Constant subtracted from the mean or weighted mean. Normally, it is positive but may be zero or negative as well.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <returns>img : object - numpy.ndarray representing the original image.
        /// <para>threshold : object - numpy.ndarray representing the thresholded image.</para></returns>
        public (byte[] img, byte[] threshold) adaptive_threshold(byte[] image, bool process_background = false, int blocksize = 15, int c = -2)
        {
            (var img, var thr) = adaptive_threshold(Mat.FromImageData(image), process_background, blocksize, c);
            return (img.ToBytes(), thr.ToBytes());
        }

        /// <summary>
        /// Thresholds an image using OpenCV's adaptiveThreshold.
        /// </summary>
        /// <param name="image">Image bytes array.</param>
        /// <param name="process_background">Whether or not to process lines that are in background.</param>
        /// <param name="blocksize">Size of a pixel neighborhood that is used to calculate a threshold value for the pixel: 3, 5, 7, and so on.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <param name="c">Constant subtracted from the mean or weighted mean. Normally, it is positive but may be zero or negative as well.
        /// <para>For more information, refer `OpenCV's adaptiveThreshold https://docs.opencv.org/2.4/modules/imgproc/doc/miscellaneous_transformations.html#adaptivethreshold`.</para></param>
        /// <returns>img : object - numpy.ndarray representing the original image.
        /// <para>threshold : object - numpy.ndarray representing the thresholded image.</para></returns>
        public (Mat img, Mat threshold) adaptive_threshold(Mat image, bool process_background = false, int blocksize = 15, int c = -2)
        {
            using (var gray = new Mat())
            {
                var threshold = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                if (process_background)
                {
                    Cv2.AdaptiveThreshold(gray, threshold, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, blocksize, c);
                }
                else
                {
                    Cv2.BitwiseNot(gray, gray); // np.invert(...)
                    Cv2.AdaptiveThreshold(gray, threshold, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, blocksize, c);
                }
                return (image, threshold);
            }
        }

        /// <summary>
        /// Finds horizontal and vertical lines by applying morphological
        /// transformations on an image.
        /// </summary>
        /// <param name="threshold">numpy.ndarray representing the thresholded image.</param>
        /// <param name="regions">List of page regions that may contain tables of the form x1,y1,x2,y2
        /// where(x1, y1) -> left-top and(x2, y2) -> right-bottom in image coordinate space.</param>
        /// <param name="direction">Specifies whether to find vertical or horizontal lines.</param>
        /// <param name="line_scale">Factor by which the page dimensions will be divided to get
        /// smallest length of lines that should be detected.
        /// <para>The larger this value, smaller the detected lines. Making it
        /// too large will lead to text being detected as lines.</para></param>
        /// <param name="iterations">Number of times for erosion/dilation is applied.
        /// <para>For more information, refer `OpenCV's dilate https://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html#dilate`_.</para></param>
        /// <returns>dmask : object - numpy.ndarray representing pixels where vertical/horizontal lines lie.
        /// <para>lines : list - List of tuples representing vertical/horizontal lines with
        /// coordinates relative to a left-top origin in
        /// image coordinate space.</para></returns>
        public (Mat dmask, List<(int, int, int, int)> lines) find_lines(Mat threshold, List<(int x1, int y1, int x2, int y2)> regions = null,
            string direction = "horizontal", int line_scale = 15, int iterations = 0)
        {
            List<(int, int, int, int)> lines = new List<(int, int, int, int)>();
            using (Mat threshold_local = threshold.Clone())
            {
                Mat el = null;
                if (direction == "vertical")
                {
                    var size = threshold_local.shape()[0].FloorDiv(line_scale);
                    el = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(1, size));
                }
                else if (direction == "horizontal")
                {
                    var size = threshold_local.shape()[1].FloorDiv(line_scale);
                    el = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(size, 1));
                }
                else
                {
                    throw new ArgumentException("Specify direction as either 'vertical' or 'horizontal'", nameof(direction));
                }

                if (regions?.Count > 0)
                {
                    var region_mask = (Mat)Mat.Zeros(threshold_local.Rows, threshold_local.Cols, threshold_local.Type()); //np.zeros(threshold.shape)
                    foreach (var region in regions)
                    {
                        (int x, int y, int w, int h) = region;
                        var part = region_mask.SubMat(y, y + h, x, x + w);
                        part.SetTo(1);
                        Cv2.Multiply(threshold_local, region_mask, threshold_local); //threshold = np.multiply(threshold, region_mask)
                    }
                }

                Cv2.Erode(threshold_local, threshold_local, el);
                Cv2.Dilate(threshold_local, threshold_local, el);
                Mat dmask = new Mat();
                Cv2.Dilate(threshold_local, dmask, el, iterations: iterations);

                Debug.Assert(threshold_local.Channels() == 1);
                Cv2.FindContours(threshold_local, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                foreach (var c in contours)
                {
                    (var x, var y, var w, var h) = Cv2.BoundingRect(c).ToTuple();
                    var x1 = x;
                    var y1 = y;
                    var x2 = x + w;
                    var y2 = y + h;

                    if (direction == "vertical")
                    {
                        lines.Add(((x1 + x2).FloorDiv(2), y2, (x1 + x2).FloorDiv(2), y1));
                    }
                    else if (direction == "horizontal")
                    {
                        lines.Add((x1, (y1 + y2).FloorDiv(2), x2, (y1 + y2).FloorDiv(2)));
                    }
                }

                return (dmask, lines);
            }
        }

        /// <summary>
        /// Finds horizontal and vertical lines by applying morphological
        /// transformations on an image.
        /// </summary>
        /// <param name="threshold">numpy.ndarray representing the thresholded image.</param>
        /// <param name="regions">List of page regions that may contain tables of the form x1,y1,x2,y2
        /// where(x1, y1) -> left-top and(x2, y2) -> right-bottom in image coordinate space.</param>
        /// <param name="direction">Specifies whether to find vertical or horizontal lines.</param>
        /// <param name="line_scale">Factor by which the page dimensions will be divided to get
        /// smallest length of lines that should be detected.
        /// <para>The larger this value, smaller the detected lines. Making it
        /// too large will lead to text being detected as lines.</para></param>
        /// <param name="iterations">Number of times for erosion/dilation is applied.
        /// <para>For more information, refer `OpenCV's dilate https://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html#dilate`_.</para></param>
        /// <returns>dmask : object - numpy.ndarray representing pixels where vertical/horizontal lines lie.
        /// <para>lines : list - List of tuples representing vertical/horizontal lines with
        /// coordinates relative to a left-top origin in
        public (byte[] dmask, List<(int, int, int, int)> lines) find_lines(byte[] threshold, List<(int x1, int y1, int x2, int y2)> regions = null, string direction = "horizontal", int line_scale = 15, int iterations = 0)
        {
            using (Mat thresholdMat = Mat.FromImageData(threshold))
            {
                (var d, var l) = find_lines(thresholdMat, regions, direction, line_scale, iterations);
                return (d.ToBytes(), l);
            }
        }

        /// <summary>
        /// Finds table boundaries using OpenCV's findContours.
        /// </summary>
        /// <param name="vertical">numpy.ndarray representing pixels where vertical lines lie.</param>
        /// <param name="horizontal">numpy.ndarray representing pixels where horizontal lines lie.</param>
        /// <returns>cont : list - List of tuples representing table boundaries. Each tuple is of
        /// the form (x, y, w, h) where (x, y) -> left-top, w -> width and
        /// h -> height in image coordinate space.</returns>
        public List<(int x, int y, int w, int h)> find_contours(Mat vertical, Mat horizontal)
        {
            using (var mask = (Mat)(vertical + horizontal))
            {
                Debug.Assert(mask.Channels() == 1);
                Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                // sort in reverse based on contour area and use first 10 contours
                contours = contours.OrderByDescending(k => Cv2.ContourArea(k)).Take(10).ToArray();

                var cont = new List<(int, int, int, int)>();
                foreach (var c in contours)
                {
                    var c_poly = Cv2.ApproxPolyDP(c, 3, true);
                    (var x, var y, var w, var h) = Cv2.BoundingRect(c_poly).ToTuple();
                    cont.Add((x, y, w, h));
                }
                return cont;
            }
        }

        /// <summary>
        /// Finds table boundaries using OpenCV's findContours.
        /// </summary>
        /// <param name="vertical">numpy.ndarray representing pixels where vertical lines lie.</param>
        /// <param name="horizontal">numpy.ndarray representing pixels where horizontal lines lie.</param>
        /// <returns>cont : list - List of tuples representing table boundaries. Each tuple is of
        /// the form (x, y, w, h) where (x, y) -> left-top, w -> width and
        /// h -> height in image coordinate space.</returns>
        public List<(int x, int y, int w, int h)> find_contours(byte[] vertical, byte[] horizontal)
        {
            using (Mat verticalMat = Mat.FromImageData(vertical))
            using (Mat horizontalMat = Mat.FromImageData(horizontal))
            {
                return find_contours(verticalMat, horizontalMat);
            }
        }

        /// <summary>
        /// Finds joints/intersections present inside each table boundary.
        /// </summary>
        /// <param name="contours">list - List of tuples representing table boundaries.Each tuple is of
        /// the form (x, y, w, h) where (x, y) -> left-top, w -> width and
        /// h -> height in image coordinate space.</param>
        /// <param name="vertical">numpy.ndarray representing pixels where vertical lines lie.</param>
        /// <param name="horizontal">numpy.ndarray representing pixels where horizontal lines lie.</param>
        /// <returns>tables : dict - Dict with table boundaries as keys and list of intersections
        /// in that boundary as their value.
        /// Keys are of the form (x1, y1, x2, y2) where (x1, y1) -> lb and (x2, y2) -> rt in image coordinate space.</returns>
        public Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> find_joints(List<(int x, int y, int w, int h)> contours, Mat vertical, Mat horizontal)
        {
            using (Mat joints = new Mat())
            {
                Cv2.Multiply(vertical, horizontal, joints); // joints = np.multiply(vertical, horizontal)
                var tables = new Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>>();

                foreach (var c in contours)
                {
                    (int x, int y, int w, int h) = c;
                    var roi = joints.SubMat(y, y + h, x, x + w); // roi = joints[y: y + h, x: x + w]
                    Debug.Assert(roi.Channels() == 1);
                    Cv2.FindContours(roi, out Point[][] jc, out _, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

                    if (jc.Length <= 4)
                    {
                        // remove contours with less than 4 joints
                        continue;
                    }

                    var joint_coords = new List<(float, float)>();
                    foreach (var j in jc)
                    {
                        (int jx, int jy, int jw, int jh) = Cv2.BoundingRect(j).ToTuple();
                        int c1 = x + (2 * jx + jw).FloorDiv(2); // 2;
                        int c2 = y + (2 * jy + jh).FloorDiv(2); // 2;
                        joint_coords.Add((c1, c2));
                    }

                    //joint_coords = joint_coords.OrderByDescending(k => k.Item2).ThenByDescending(k => k.Item1).ToList(); // added by bobld

                    tables[(x, y + h, x + w, y)] = joint_coords;
                }
                return tables;
            }
        }

        /// <summary>
        /// Finds joints/intersections present inside each table boundary.
        /// </summary>
        /// <param name="contours">list - List of tuples representing table boundaries.Each tuple is of
        /// the form (x, y, w, h) where (x, y) -> left-top, w -> width and
        /// h -> height in image coordinate space.</param>
        /// <param name="vertical">numpy.ndarray representing pixels where vertical lines lie.</param>
        /// <param name="horizontal">numpy.ndarray representing pixels where horizontal lines lie.</param>
        /// <returns>tables : dict - Dict with table boundaries as keys and list of intersections
        /// in that boundary as their value.
        /// Keys are of the form (x1, y1, x2, y2) where (x1, y1) -> lb and (x2, y2) -> rt in image coordinate space.</returns>
        public Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> find_joints(List<(int x, int y, int w, int h)> contours, byte[] vertical, byte[] horizontal)
        {
            using (Mat verticalMat = Mat.FromImageData(vertical))
            using (Mat horizontalMat = Mat.FromImageData(horizontal))
            {
                return find_joints(contours, verticalMat, horizontalMat);
            }
        }

        public (int height, int width, int channels) GetImageShape(byte[] image)
        {
            using (Mat img = Mat.FromImageData(image))
            {
                var sh = img.shape();
                return (sh[0], sh[1], sh[2]);
            }
        }
    }
}