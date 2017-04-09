using NUnit.Framework;
using System;
using PSEGetLib;
using PSEGetLib.DocumentModel;
using PSEGetLib.Interfaces;
using PSEGetLib.Service;
using System.Linq;
using System.Collections.Generic;

namespace PSEGetTests
{
	[TestFixture()]
	public class PSEReportReaderTest
	{
		[Test()]
		public void TestSector()
		{
			var expected = 0;
			var actual = 0;

			Assert.AreEqual(expected, actual, "oh oh");
		}

		[TestCase("/Users/arnolddiaz/PSEGet/Mac/PSEGetc/psegetc/PSEGetTests/TestFiles/stockQuotes_0403017.pdf")]
		public void TestUnsupportedReportFormat(string fileToConvert)
		{

			IPdfService pdfService = new PdfTextSharpService();
			var pseDocument = new PSEDocument();

			IPSEReportReader reader = new PSEReportReader2();

			Assert.Throws(
				typeof(EUnsupportedReportFormat),
				new TestDelegate(() =>
				{
					reader.Fill(pseDocument, pdfService.ExtractTextFromPdf(fileToConvert));
				}),
				"An exception wasn't thrown");

		}

		private string _reportFile = "/Users/arnolddiaz/PSEGet/Mac/PSEGetc/psegetc/PSEGetTests/TestFiles/stockQuotes_04042017.pdf";

		[Test()]
		public void TestMarketSummary_PSEi()
		{
			IPdfService pdfService = new PdfTextSharpService();
			var pseDocument = new PSEDocument();

			IPSEReportReader reader = new PSEReportReader2();
			reader.Fill(pseDocument, pdfService.ExtractTextFromPdf(_reportFile));

			//OPEN HIGH LOW CLOSE %CHANGE PT.CHANGE VOLUME VALUE, Php
			//PSEi 7,363.88 7,456.26 7,354.57 7,446.49 1.42 104.84
			//All Shares 4,424.05 4,464.21 4,420.44 4,460.56 1.06 47.14
			//GRAND TOTAL 1,949,153,231 11,941,051,832.83

			var expected = 0.0;
			var actual = 0.0;

			//psei
			var psei = pseDocument.GetSector(PSEDocument.PSEI);
			expected = 7363.88;
			actual = psei.Open;
			Assert.AreEqual(expected, actual, "PSEi open does not match");

			expected = 7456.26;
			actual = psei.High;
			Assert.AreEqual(expected, actual, "PSEi high does not match");

			expected = 7354.57;
			actual = psei.Low;
			Assert.AreEqual(expected, actual, "PSEi low does not match");

			expected = 7446.49;
			actual = psei.Close;
			Assert.AreEqual(expected, actual, "PSEi close does not match");
					 
			expected = 1949153231.0;
			actual = psei.Volume;
			Assert.AreEqual(expected, actual, "PSEi Volume does not match");

			expected = 4076175042.08;
			actual = psei.NetForeignBuy;
			Assert.AreEqual(expected, actual, "PSEi NFB does not match");

			//all shares


		}

		[Test()]
		public void TestMiscData()
		{

			IPdfService pdfService = new PdfTextSharpService();
			var pseDocument = new PSEDocument();

			IPSEReportReader reader = new PSEReportReader2();
			reader.Fill(pseDocument, pdfService.ExtractTextFromPdf(_reportFile));

			//NO.OF ADVANCES: 114
			//NO.OF DECLINES: 79
			//NO.OF UNCHANGED: 42

			//NO.OF TRADED ISSUES: 235
			//NO.OF TRADES: 92,699
			//ODDLOT VOLUME:      791,591
			//ODDLOT VALUE:         Php 267,667.16
			//BLOCK SALE VOLUME: 248,879,170
			//BLOCK SALE VALUE: Php 3,836,846,796.68

			var expected = 0.0;
			var actual = 0.0;

			expected = 114;
			actual = pseDocument.NumAdvance;
			Assert.AreEqual(expected, actual, "Advances does not match.");

			expected = 79;
			actual = pseDocument.NumDeclines;
			Assert.AreEqual(expected, actual, "Declines does not match.");

			expected = 42;
			actual = pseDocument.NumUnchanged;
			Assert.AreEqual(expected, actual, "Unchanged does not match.");
		}

		[Test()]
		public void TestMarketSummary_Financials()
		{
			IPdfService pdfService = new PdfTextSharpService();
			var pseDocument = new PSEDocument();

			IPSEReportReader reader = new PSEReportReader2();
			reader.Fill(pseDocument, pdfService.ExtractTextFromPdf(_reportFile));

			//OPEN HIGH LOW CLOSE %CHANGE PT.CHANGE VOLUME VALUE, Php
			//Financials 1,835.16 1,865.93 1,830.89 1,865.93 1.7 31.31 15,721,945 1,537,689,244.77
			//GRAND TOTAL 1,949,153,231 11,941,051,832.83

			var expected = 0.0;
			var actual = 0.0;

			//psei
			var psei = pseDocument.GetSector(PSEDocument.FINANCIAL);
			expected = 1835.16;
			actual = psei.Open;
			Assert.AreEqual(expected, actual, "FINANCIAL open does not match");

			expected = 1865.93;
			actual = psei.High;
			Assert.AreEqual(expected, actual, "FINANCIAL high does not match");

			expected = 1830.89;
			actual = psei.Low;
			Assert.AreEqual(expected, actual, "FINANCIAL low does not match");

			expected = 1865.93;
			actual = psei.Close;
			Assert.AreEqual(expected, actual, "FINANCIAL close does not match");

			expected = 15721945.0;
			actual = psei.Volume;
			Assert.AreEqual(expected, actual, "FINANCIAL Volume does not match");

			expected = 1537689244.77;
			actual = psei.Value;
			Assert.AreEqual(expected, actual, "FINANCIAL Value does not match");

			//all shares
		}

		public void TestSectorStockCount()
		{
			IPdfService pdfService = new PdfTextSharpService();
			var pseDocument = new PSEDocument();

			IPSEReportReader reader = new PSEReportReader2();
			reader.Fill(pseDocument, pdfService.ExtractTextFromPdf(_reportFile));

			var finance = pseDocument.GetSector(PSEDocument.FINANCIAL);
			var expected = 0;
			var actual = 0;
			Assert.AreEqual(expected, actual, PSEDocument.FINANCIAL + " sector stock count fail.");

			var industrial = pseDocument.GetSector(PSEDocument.INDUSTRIAL);
			expected = 0;
			actual = 0;
			Assert.AreEqual(expected, actual, PSEDocument.INDUSTRIAL + " sector stock count fail.");

			var property = pseDocument.GetSector(PSEDocument.PROPERTY);
			expected = 0;
			actual = 0;
			Assert.AreEqual(expected, actual, PSEDocument.PROPERTY + " sector stock count fail.");

			var services = pseDocument.GetSector(PSEDocument.SERVICE);
			expected = 0;
			actual = 0;
			Assert.AreEqual(expected, actual, PSEDocument.SERVICE + " sector stock count fail.");

			var mining = pseDocument.GetSector(PSEDocument.MINING_OIL);
			expected = 0;
			Assert.AreEqual(expected, actual, PSEDocument.MINING_OIL + " sector stock count fail.");

			var preferred = pseDocument.GetSector(PSEDocument.PREFERRED);
			expected = 0;
			actual = 0;
			Assert.AreEqual(expected, actual, PSEDocument.PREFERRED + " sector stock count fail.");

			var warrants = pseDocument.GetSector(PSEDocument.WARRANT);
			expected = 0;
			actual = 0;
			Assert.AreEqual(expected, actual, PSEDocument.WARRANT + " sector stock count fail.");

			var sme = pseDocument.GetSector(PSEDocument.SME);
			expected = 0;
			actual = 0;
			Assert.AreEqual(expected, actual, PSEDocument.SME + " sector stock count fail.");

		}
	}
}
