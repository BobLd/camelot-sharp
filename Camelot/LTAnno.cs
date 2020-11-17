using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace Camelot
{
    /// <summary>
    /// Represent an actual letter in the text as a Unicode string.
    /// Note that, while a LTChar object has actual boundaries, LTAnno objects does not, as these are
    /// "virtual" characters, inserted by a layout analyzer according to the relationship between two characters (e.g. a space).
    /// <para>https://euske.github.io/pdfminer/programming.html</para>
    /// </summary>
    internal class LTAnno : Letter
    {
        public LTAnno(string value)
            : base(value, new PdfRectangle(), new PdfPoint(), new PdfPoint(), 0, 0, null, null, 0, -1)
        { }
    }
}
