using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace Camelot.LayoutExtractor
{
    public class PdfMinerLayoutExtractor
    {
        /// <summary>
        /// Parameters for layout analysis
        /// </summary>
        public class LAParams : DlaOptions
        {
            /// <summary>
            /// If two characters have more overlap than this they
            /// are considered to be on the same line.The overlap is specified
            /// relative to the minimum height of both characters.
            /// </summary>
            public float line_overlap { get; set; }

            /// <summary>
            /// If two characters are closer together than this
            /// margin they are considered part of the same line. The margin is
            /// specified relative to the width of the character.
            /// </summary>
            public float char_margin { get; set; } = float.PositiveInfinity;

            /// <summary>
            /// If two characters on the same line are further apart
            /// than this margin then they are considered to be two separate words, and
            /// an intermediate space will be added for readability. The margin is
            /// specified relative to the width of the character.
            /// </summary>
            public float word_margin { get; set; }

            /// <summary>
            /// If two lines are close together they are
            /// considered to be part of the same paragraph. The margin is
            /// specified relative to the height of a line.
            /// </summary>
            public float line_margin { get; set; }

            /// <summary>
            /// Specifies how much a horizontal and vertical position
            /// of a text matters when determining the order of text boxes. The value
            /// should be within the range of -1.0 (only horizontal position
            /// matters) to +1.0 (only vertical position matters). You can also pass
            /// `NaN` to disable advanced layout analysis, and instead return text
            /// based on the position of the bottom left corner of the text box.
            /// </summary>
            public float boxes_flow { get; set; } = float.NaN;

            /// <summary>
            /// If vertical text should be considered during layout analysis.
            /// </summary>
            public bool detect_vertical { get; set; }

            /// <summary>
            /// If layout analysis should be performed on text in figures.
            /// </summary>
            public bool all_texts { get; set; }
        }

        //https://github.com/pdfminer/pdfminer.six/blob/f389b97923c7a847bc9c6f4c3374951e1a7ff764/pdfminer/layout.py#L593
        /// <summary>
        /// group_objects: group text object to textlines.
        /// </summary>
        /// <param name="laparams"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public IEnumerable<TextLine> group_objects(LAParams laparams, IEnumerable<Letter> objs)
        {
            Letter obj0 = null;
            TextLine line = null;
            foreach (var obj1 in objs)
            {
                if (obj0 != null)
                {
                    // halign: obj0 and obj1 is horizontally aligned.
                    //
                    //   +------+ - - -
                    //   | obj0 | - - +------+   -
                    //   |      |     | obj1 |   | (line_overlap)
                    //   +------+ - - |      |   -
                    //          - - - +------+
                    //
                    //          |<--->|
                    //        (char_margin)
                    var halign = obj0.is_compatible(obj1) && obj0.is_voverlap(obj1) &&
                        Math.Min(obj0.GlyphRectangle.Height, obj1.GlyphRectangle.Height) * laparams.line_overlap < obj0.voverlap(obj1) &&
                        obj0.hdistance(obj1) < Math.Max(obj0.GlyphRectangle.Width, obj1.GlyphRectangle.Width) * laparams.char_margin;

                    var is_hoverlap = DocstrumBoundingBoxes.GetStructuralBlockingParameters(new PdfLine(obj0.StartBaseLine, obj0.EndBaseLine), new PdfLine(obj1.StartBaseLine, obj1.EndBaseLine), 1e-3,
                                                                                            out double angularDifference, out double normalisedOverlap, out double perpendicularDistance);

                    // valign: obj0 and obj1 is vertically aligned.
                    //
                    //   +------+
                    //   | obj0 |
                    //   |      |
                    //   +------+ - - -
                    //     |    |     | (char_margin)
                    //     +------+ - -
                    //     | obj1 |
                    //     |      |
                    //     +------+
                    //
                    //     |<-->|
                    //   (line_overlap)
                    var valign = laparams.detect_vertical && obj0.is_compatible(obj1) && obj0.is_hoverlap(obj1) &&
                        Math.Min(obj0.GlyphRectangle.Width, obj1.GlyphRectangle.Width) * laparams.line_overlap < obj0.hoverlap(obj1) &&
                        obj0.vdistance(obj1) < Math.Max(obj0.GlyphRectangle.Height, obj1.GlyphRectangle.Height) * laparams.char_margin;





                    if ((halign && line.isHorizontal()) || (valign && line.isVertical()))
                    {
                        //line.Add(obj1);
                        throw new NotImplementedException();
                    }
                    else if (line != null)
                    {
                        yield return line;
                        line = null;
                    }
                    else
                    {
                        if (valign && !halign)
                        {
                            throw new NotImplementedException();
                        }
                        else if (halign && !valign)
                        {
                            throw new NotImplementedException();
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }

            if (line == null)
            {
                //line = LTTextLineHorizontal(laparams.word_margin)
                //line.add(obj0)
            }
            yield return line;
        }

        /// <summary>
        /// Group neighboring lines to textboxes
        /// <para>https://github.com/pdfminer/pdfminer.six/blob/f389b97923c7a847bc9c6f4c3374951e1a7ff764/pdfminer/layout.py#L674</para>
        /// </summary>
        /// <param name="laparams"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        public IEnumerable<TextBlock> group_textlines(LAParams laparams, IEnumerable<TextLine> lines)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// https://github.com/pdfminer/pdfminer.six/blob/f389b97923c7a847bc9c6f4c3374951e1a7ff764/pdfminer/layout.py#L705
        /// </summary>
        public IEnumerable<IEnumerable<TextBlock>> group_textboxes(LAParams laparams, IEnumerable<object> boxes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// https://github.com/pdfminer/pdfminer.six/blob/f389b97923c7a847bc9c6f4c3374951e1a7ff764/pdfminer/layout.py#L786
        /// </summary>
        /// <param name="laparams"></param>
        public void analyze(Page page, LAParams laparams)
        {
            // textobjs is a list of LTChar objects, i.e.
            // it has all the individual characters in the page.
            var textobjs = page.Letters;
            //(textobjs, otherobjs) = fsplit(lambda obj: isinstance(obj, LTChar), self)
            // for obj in otherobjs:
            //    obj.analyze(laparams)

            if (textobjs.Count == 0) return;
            var textlines = group_objects(laparams, textobjs);
            IEnumerable<TextLine> empties;
            // (empties, textlines) = fsplit(lambda obj: obj.is_empty(), textlines)
            var lu = textlines.ToList().ToLookup(obj => obj.IsEmpty());
            (empties, textlines) = (lu[true], lu[false]);

            // for obj in empties:
            //    obj.analyze(laparams)

            var textboxes = group_textlines(laparams, textlines);

            if (float.IsNaN(laparams.boxes_flow))
            {
                //for textbox in textboxes:
                //    textbox.analyze(laparams)

                (int, float, float) getKey(TextBlock box)
                {
                    if (box.TextOrientation == TextOrientation.Rotate90 || box.TextOrientation == TextOrientation.Rotate270)
                    {
                        return (0, -box.X1(), -box.Y0());
                    }
                    else
                    {
                        return (1, -box.Y0(), box.X0());
                    }
                }

                textboxes = textboxes.OrderBy(box => getKey(box));
            }
            else
            {
                int index = 0;
                var groups = group_textboxes(laparams, textboxes);
                // assigner = IndexAssigner()
                foreach (var g in groups)
                {
                    //group.analyze(laparams)
                    foreach (var b in g)
                    {
                        b.SetReadingOrder(index);
                        index++;
                    }
                }
                textboxes = textboxes.OrderBy(box => box.ReadingOrder);
            }
            //self._objs = textboxes + otherobjs + empties
        }
    }
}
