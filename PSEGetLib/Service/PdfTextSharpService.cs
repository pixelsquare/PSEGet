using System;
using PSEGetLib.Interfaces;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;

namespace PSEGetLib.Service
{
    public class PdfTextSharpService : IPdfService
    {
        public string ExtractTextFromPdf(string filePath)
        {
            ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();

            using (PdfReader reader = new PdfReader(filePath))
            {
                var text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string thePage = PdfTextExtractor.GetTextFromPage(reader, i);
                    string[] theLines = thePage.Split('\n');
                    foreach (var theLine in theLines)
                    {
                        text.AppendLine(theLine);
                    }
                }
                return text.ToString();
            }
        }
    }
}
