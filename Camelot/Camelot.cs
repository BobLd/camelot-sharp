using Camelot.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using static Camelot.Core;

namespace Camelot
{
    public static class Camelot
    {
        /// <summary>
        /// Read PDF and return extracted tables.
        /// </summary>
        /// <param name="filepath">Filepath or URL of the PDF file.</param>
        /// <param name="pages">Comma-separated page numbers. <para>Example: '1,3,4' or '1,4-end' or 'all'.</para></param>
        /// <param name="password">Password for decryption.</param>
        /// <param name="flavor">The parsing method to use ('lattice' or 'stream'). Lattice is used by default.</param>
        /// <param name="suppress_stdout">Print all logs and warnings.</param>
        /// <param name="layout_kwargs"></param>
        /// <returns>camelot.core.TableList</returns>
        public static TableList ReadPdf(string filepath,
            string pages = "1",
            string password = null,
            string flavor = "lattice",
            bool suppress_stdout = false,
            params DlaOptions[] layout_kwargs)
        {
            if (flavor != "lattice" && flavor != "stream")
            {
                throw new NotImplementedException("Unknown flavor specified. Use either 'lattice' or 'stream'");
            }

            using (var p = new PDFHandler(filepath, pages: pages, password: password))
            {
                return p.Parse(flavor, suppress_stdout, layout_kwargs);
            }
        }
    }

    /// <summary>
    /// Handles all operations like temp directory creation, splitting
    /// file into single page PDFs, parsing each PDF and then removing the
    /// temp directory.
    /// </summary>
    internal class PDFHandler : IDisposable
    {
        private readonly PdfDocument pdfDocument;
        private readonly int[] pagesInt;

        public PDFHandler(string filepath, string pages, string password)
        {
            pdfDocument = PdfDocument.Open(filepath,
                new ParsingOptions()
                {
                    ClipPaths = true,
                    Password = password
                });
            this.pagesInt = GetPages(pdfDocument, pages);
        }

        public void Dispose()
        {
            pdfDocument.Dispose();
        }

        /// <summary>
        /// Extracts tables by calling parser.get_tables on all single page PDFs.
        /// </summary>
        /// <param name="flavor">The parsing method to use ('lattice' or 'stream').
        /// Lattice is used by default.</param>
        /// <param name="suppress_stdout">Suppress logs and warnings.</param>
        /// <param name="layout_kwargs"></param>
        /// <returns></returns>
        public TableList Parse(string flavor = "lattice", bool suppress_stdout = false, params DlaOptions[] layout_kwargs) //, **kwargs)
        {
            var tables = new List<Table>();
            BaseParser parser;
            switch (flavor.ToLowerInvariant())
            {
                case "lattice":
                    parser = new Lattice();
                    break;

                default:
                    parser = new Stream();
                    break;
            }

            foreach (var page in this.pagesInt)
            {
                var p = pdfDocument.GetPage(page);
                tables.AddRange(parser.ExtractTables(p, suppress_stdout, layout_kwargs));
            }

            tables.Sort();
            return new TableList(tables);
        }

        /// <summary>
        /// Converts pages string to list of ints.
        /// </summary>
        /// <param name="infile">Filepath or URL of the PDF file.</param>
        /// <param name="pages">Comma-separated page numbers.
        /// <para>Example: '1,3,4' or '1,4-end' or 'all'.</para></param>
        /// <returns></returns>
        private static int[] GetPages(PdfDocument infile, string pages)
        {
            List<(int, int)> page_numbers = new List<(int, int)>();
            if (pages == "1")
            {
                page_numbers.Add((1, 1));
            }
            else
            {
                if (pages == "all")
                {
                    page_numbers.Add((1, infile.NumberOfPages));
                }
                else
                {
                    foreach (var r in pages.Split(','))
                    {
                        if (r.Contains("-"))
                        {
                            var p = r.Split('-');
                            if (p.Length != 2)
                            {
                                throw new ArgumentException("", nameof(pages));
                            }
                            var a = p[0];
                            var b = p[1];
                            if (b == "end")
                            {
                                b = infile.NumberOfPages.ToString();
                            }
                            page_numbers.Add((int.Parse(a), int.Parse(b)));
                        }
                        else
                        {
                            page_numbers.Add((int.Parse(r), int.Parse(r)));
                        }
                    }
                }
            }

            List<int> P = new List<int>();
            foreach (var p in page_numbers)
            {
                var count = p.Item2 + 1 - p.Item1;
                P.AddRange(Enumerable.Range(p.Item1, count));
            }

            return P.Distinct().OrderBy(x => x).ToArray();
        }
    }
}
