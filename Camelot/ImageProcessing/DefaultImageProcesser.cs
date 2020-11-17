using System;
using System.Collections.Generic;
using System.Text;
using UglyToad.PdfPig.Content;

namespace Camelot.ImageProcessing
{
    public class DefaultImageProcesser : IImageProcesser
    {
        public (byte[] img, byte[] threshold) adaptive_threshold(byte[] image, bool process_background = false, int blocksize = 15, int c = -2)
        {
            throw new NotImplementedException();
        }

        public List<(int x, int y, int w, int h)> find_contours(byte[] vertical, byte[] horizontal)
        {
            throw new NotImplementedException();
        }

        public Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> find_joints(List<(int x, int y, int w, int h)> contours, byte[] vertical, byte[] horizontal)
        {
            throw new NotImplementedException();
        }

        public (byte[] dmask, List<(int, int, int, int)> lines) find_lines(byte[] threshold, List<(int x1, int y1, int x2, int y2)> regions = null, string direction = "horizontal", int line_scale = 15, int iterations = 0)
        {
            throw new NotImplementedException();
        }

        public (int height, int width, int channels) GetImageShape(byte[] image)
        {
            throw new NotImplementedException();
        }

        public (Dictionary<(float x1, float y1, float x2, float y2), List<(float, float)>> table_bbox, List<(float, float, float, float)> vertical_segments, List<(float, float, float, float)> horizontal_segments)
            Process(Page page, IDrawingProcessor drawingProcessor, bool process_background, int threshold_blocksize, int threshold_constant, int line_scale, int iterations,
            List<(float x1, float y1, float x2, float y2)> table_areas, List<(float x1, float y1, float x2, float y2)> table_regions)
        {
            if (table_areas == null || table_areas.Count == 0)
            {
                List<(int, int, int, int)> regions = null;
                if (table_regions != null && table_regions.Count > 0)
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
