using System;
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using PSEGetLib.Interfaces;

namespace PSEGetLib.Service
{
    public class PdfBoxService : IPdfService
    {
        public string ExtractTextFromPdf(string filePath)
        {
            var doc = PDDocument.load(fullFilePath);
            //PSEDocument pd = new PSEDocument();

            var stripper = new PDFTextStripper();
            string pdfText = stripper.getText(doc).TrimEnd();
            return pdfText;
        }
    }
}
