using System.Collections.Generic;
using UglyToad.PdfPig.Content;

namespace Camelot.ImageProcessing
{
    public interface IImageProcesser
    {
        /// <summary>
        /// full processing
        /// </summary>
        /// <param name="page"></param>
        /// <param name="drawingProcessor"></param>
        /// <param name="process_background"></param>
        /// <param name="threshold_blocksize"></param>
        /// <param name="threshold_constant"></param>
        /// <param name="line_scale"></param>
        /// <param name="iterations"></param>
        /// <param name="table_areas"></param>
        /// <param name="table_regions"></param>
        /// <param name="table_bbox_unscaled"></param>
        /// <returns></returns>
        (Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox, List<(float, float, float, float)> vertical_segments, List<(float, float, float, float)> horizontal_segments)
            Process(Page page, ImageProcessing.IDrawingProcessor drawingProcessor,
                    bool process_background, int threshold_blocksize, int threshold_constant, int line_scale, int iterations,
                    List<string> table_areas, List<string> table_regions, out Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox_unscaled);

        /// <summary>
        /// Thresholds an image using OpenCV's adaptiveThreshold.
        /// </summary>
        (byte[] img, byte[] threshold) adaptive_threshold(byte[] image, bool process_background = false, int blocksize = 15, int c = -2);

        /// <summary>
        /// Finds horizontal and vertical lines by applying morphological
        /// transformations on an image.
        /// </summary>
        (byte[] dmask, List<(int, int, int, int)> lines) find_lines(byte[] threshold, List<(int x1, int y1, int x2, int y2)> regions = null,
            string direction = "horizontal", int line_scale = 15, int iterations = 0);

        /// <summary>
        /// Finds table boundaries using OpenCV's findContours.
        /// </summary>
        List<(int x, int y, int w, int h)> find_contours(byte[] vertical, byte[] horizontal);

        /// <summary>
        /// Finds joints/intersections present inside each table boundary.
        /// </summary>
        Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> find_joints(List<(int x, int y, int w, int h)> contours, byte[] vertical, byte[] horizontal);

        /// <summary>
        /// Gets the image's shape.
        /// </summary>
        /// <returns>Height, Width, Channels</returns>
        (int height, int width, int channels) GetImageShape(byte[] image);
    }
}
