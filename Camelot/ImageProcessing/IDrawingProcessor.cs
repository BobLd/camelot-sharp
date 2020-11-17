using System;
using System.IO;
using UglyToad.PdfPig.Content;

namespace Camelot.ImageProcessing
{
    [Obsolete("Will be made available in PdfPig.")]
    public interface IDrawingProcessor
    {
        /// <summary>
        /// DrawPage
        /// </summary>
        /// <param name="page"></param>
        /// <param name="scale"></param>
        [Obsolete("Will be made available in PdfPig.")]
        MemoryStream DrawPage(Page page, double scale);
    }
}
