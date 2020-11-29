using System;
using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Rendering;

namespace Camelot.ImageProcessing
{
    public class DefaultImageProcesser : IImageProcesser
    {
        public (Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox, List<(float, float, float, float)> vertical_segments, List<(float, float, float, float)> horizontal_segments)
            Process(Page page, IPageImageRenderer pageImageRenderer, bool process_background, int threshold_blocksize, int threshold_constant, int line_scale, int iterations,
            List<(float x1, float y1, float x2, float y2)> table_areas, List<(float x1, float y1, float x2, float y2)> table_regions)
        {
            if (table_areas == null || table_areas.Count == 0)
            {
                List<(int, int, int, int)> regions = null;
                if (table_regions?.Count > 0)
                {

                }
            }
            else
            {

            }
            throw new NotImplementedException();
        }
    }
}
