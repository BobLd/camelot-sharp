using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Logging;
using static Camelot.Core;

namespace Camelot.Parsers
{
    // https://github.com/atlanhq/camelot/blob/master/camelot/parsers/base.py

    /// <summary>
    /// Defines a base parser.
    /// </summary>
    public abstract class BaseParser
    {
        protected readonly ILog log;

        /// <summary>
        /// Defines a base parser.
        /// </summary>
        /// <param name="log"></param>
        public BaseParser(ILog log)
        {
            this.log = log;
        }

        public string FileName { get; protected set; }

        public DlaOptions[] LayoutOptions { get; protected set; }

        public Page Layout { get; protected set; }

        public (int width, int height) Dimensions { get; protected set; }

        public List<byte[]> Images { get; protected set; }

        public List<TextLine> HorizontalText { get; protected set; }

        public List<TextLine> VerticalText { get; protected set; }

        public int PdfWidth { get; protected set; }

        public int PdfHeight { get; protected set; }

        public string RootName { get; protected set; }

        public void GenerateLayout(string filename, params DlaOptions[] layout_kwargs)
        {
            FileName = filename;
            using (PdfDocument document = PdfDocument.Open(filename, new ParsingOptions() { ClipPaths = true, Logger = log }))
            {
                GenerateLayout(document.GetPage(1), layout_kwargs); // always page 1 for the moment
            }
        }

        public void GenerateLayout(Page page, params DlaOptions[] layout_kwargs)
        {
            LayoutOptions = layout_kwargs;
            Layout = page;
            Images = new List<byte[]>();
            foreach (var img in page.GetImages())
            {
                if (img.TryGetPng(out byte[] png))
                {
                    Images.Add(png);
                }
                else if (img.TryGetBytes(out var bytes))
                {
                    Images.Add(bytes.ToArray());
                }
                else
                {
                    Images.Add(img.RawBytes.ToArray());
                }
            }

            // get texts
            var nnweOptions = layout_kwargs?.Where(o => o is NearestNeighbourWordExtractor.NearestNeighbourWordExtractorOptions)?.FirstOrDefault();
            var words = nnweOptions == null ? NearestNeighbourWordExtractor.Instance.GetWords(page.Letters) : NearestNeighbourWordExtractor.Instance.GetWords(page.Letters, nnweOptions);

            var dbbOptions = layout_kwargs?.Where(o => o is DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions)?.FirstOrDefault();
            var blocks = dbbOptions == null ? DocstrumBoundingBoxes.Instance.GetBlocks(words) : DocstrumBoundingBoxes.Instance.GetBlocks(words, dbbOptions);

            // horizontal text: normal and rotated 180
            HorizontalText = blocks.SelectMany(b => b.TextLines.Where(tl => tl.TextOrientation == TextOrientation.Horizontal || tl.TextOrientation == TextOrientation.Rotate180)).ToList();

            // horizontal text: rotated 90 and 270
            VerticalText = blocks.SelectMany(b => b.TextLines.Where(tl => tl.TextOrientation == TextOrientation.Rotate90 || tl.TextOrientation == TextOrientation.Rotate270)).ToList();

            PdfWidth = (int)page.Width;
            PdfHeight = (int)page.Height;
            Dimensions = (PdfWidth, PdfHeight);

            RootName = Path.GetFileNameWithoutExtension(FileName);
        }

        public abstract List<Table> ExtractTables(string filename, bool suppress_stdout, params DlaOptions[] layout_kwargs);

        public abstract List<Table> ExtractTables(Page page, bool suppress_stdout, params DlaOptions[] layout_kwargs);
    }
}
