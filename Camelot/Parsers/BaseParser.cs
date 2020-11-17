﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using static Camelot.Core;

namespace Camelot.Parsers
{
    // https://github.com/atlanhq/camelot/blob/master/camelot/parsers/base.py

    /// <summary>
    /// Defines a base parser.
    /// </summary>
    public abstract class BaseParser
    {
        public string filename { get; protected set; }

        public DlaOptions[] layout_kwargs { get; protected set; }

        public Page layout { get; protected set; }

        public (int width, int height) dimensions { get; protected set; }

        public List<byte[]> images { get; protected set; }

        public List<TextLine> horizontal_text { get; protected set; }

        public List<TextLine> vertical_text { get; protected set; }

        public int pdf_width { get; protected set; }

        public int pdf_height { get; protected set; }

        public string rootname { get; protected set; }

        public void _generate_layout(string filename, params DlaOptions[] layout_kwargs)
        {
            //this.filename = filename;
            //this.layout_kwargs = layout_kwargs;
            //(this.layout, this.dimensions) = Utils.get_page_layout(filename, layout_kwargs);
            //this.images = Utils.get_text_objects(this.layout, ltype: "image");
            //this.horizontal_text = Utils.get_text_objects(this.layout, ltype: "horizontal_text").Cast<TextLine>().ToList();
            //this.vertical_text = Utils.get_text_objects(this.layout, ltype: "vertical_text").Cast<TextLine>().ToList();
            //(this.pdf_width, this.pdf_height) = this.dimensions;
            //this.rootname = Path.GetFileNameWithoutExtension(this.filename); //this.rootname, __ = os.path.splitext(this.filename);

            this.filename = filename;
            this.layout_kwargs = layout_kwargs;
            using (PdfDocument document = PdfDocument.Open(filename))
            {
                this.layout = document.GetPage(1); // always page 1 for the moment
            }

            this.images = new List<byte[]>();
            foreach (var img in this.layout.GetImages())
            {
                if (img.TryGetPng(out byte[] png))
                {
                    images.Add(png);
                }
                else if (img.TryGetBytes(out var bytes))
                {
                    images.Add(bytes.ToArray());
                }
                else
                {
                    images.Add(img.RawBytes.ToArray());
                }
            }

            //this.images = this.layout.GetImages().Cast<object>().ToList(); // cast as object for the moment

            // get texts
            var nnweOptions = layout_kwargs?.Where(o => o is NearestNeighbourWordExtractor.NearestNeighbourWordExtractorOptions)?.First();
            var words = nnweOptions == null ? NearestNeighbourWordExtractor.Instance.GetWords(this.layout.Letters) : NearestNeighbourWordExtractor.Instance.GetWords(this.layout.Letters, nnweOptions);

            var dbbOptions = layout_kwargs?.Where(o => o is DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions)?.First();
            var blocks = dbbOptions == null ? DocstrumBoundingBoxes.Instance.GetBlocks(words) : DocstrumBoundingBoxes.Instance.GetBlocks(words, dbbOptions);

            // horizontal text: normal and rotated 180
            this.horizontal_text = blocks.SelectMany(b => b.TextLines.Where(tl => tl.TextOrientation == TextOrientation.Horizontal || tl.TextOrientation == TextOrientation.Rotate180)).ToList();

            // horizontal text: rotated 90 and 270
            this.vertical_text = blocks.SelectMany(b => b.TextLines.Where(tl => tl.TextOrientation == TextOrientation.Rotate90 || tl.TextOrientation == TextOrientation.Rotate270)).ToList();

            this.pdf_width = (int)this.layout.Width;
            this.pdf_height = (int)this.layout.Height;
            this.dimensions = (pdf_width, pdf_height);

            this.rootname = Path.GetFileNameWithoutExtension(this.filename);
        }

        //public (List<(float, float)> cols, List<(float, float)> rows) _generate_columns_and_rows(int table_idx, (float, float, float, float) tk)

        //public abstract Table _generate_table(int table_idx, List<(float, float)> cols, List<(float, float)> rows, params string[] kwargs);
        public abstract List<Table> extract_tables(string filename, bool suppress_stdout, DlaOptions[] layout_kwargs);
    }
}