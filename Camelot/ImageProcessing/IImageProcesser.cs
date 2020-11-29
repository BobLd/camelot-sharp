using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Rendering;

namespace Camelot.ImageProcessing
{
    public interface IImageProcesser
    {
        /// <summary>
        /// Process the page to extract the tables.
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
        /// <returns>table_bbox - Finds joints/intersections present inside each table boundary (in PDF corrdinate).
        /// <para>vertical_segments - vertical lines (in PDF corrdinate)</para>
        /// horizontal_segments - horizontal lines (in PDF corrdinate)</returns>
        (Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox, List<(float, float, float, float)> vertical_segments, List<(float, float, float, float)> horizontal_segments)
            Process(Page page, IPageImageRenderer pageImageRenderer,
                    bool process_background, int threshold_blocksize, int threshold_constant, int line_scale, int iterations,
                    List<(float x1, float y1, float x2, float y2)> table_areas, List<(float x1, float y1, float x2, float y2)> table_regions);
    }
}
