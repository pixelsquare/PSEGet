using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSEGetLib;
//using org.apache.pdfbox.pdmodel;
//using org.apache.pdfbox.util;
using System.Collections.Specialized;
using PSEGetLib.DocumentModel;
using PSEGetLib.Converters;
using PSEGetLib.Service;
using PSEGetLib.Interfaces;

namespace PSEGetTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ReportReaderTest
    {
        //private PDDocument doc;        
        private string pdfDocPath = @"C:\Users\arnolddiaz\Downloads\stockQuotes_05052016.pdf";

        public ReportReaderTest()
        {
            //
            // TODO: Add constructor logic here
            //            
            //doc = PDDocument.load(@"C:\Users\Arnold\Documents\Projects\PSEGet3\trunk\PSEGetTest\stockQuotes_09202010.pdf");
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestReaderFirstLine()
        {

            //PDFTextStripper stripper = new PDFTextStripper();
            var pdfSharpService = new PdfTextSharpService();

            PSEReportReader reader = new PSEReportReader(pdfSharpService.ExtractTextFromPdf(pdfDocPath));

            string expected = "The Philippine Stock Exchange, Inc";
            string actual = reader.PSEReportString[0].Trim();
            Assert.AreEqual(expected, actual);           
        }

        //[TestMethod]
        //public void TestReaderLastLine()
        //{
        //    //PDFTextStripper stripper = new PDFTextStripper();
        //    var pdfSharpService = new PdfTextSharpService();
        //    PSEReportReader reader = new PSEReportReader(pdfSharpService.ExtractTextFromPdf(pdfDocPath));

        //    string expected = "*** Grand total includes main,oddlot and block sale transactions";
        //    string actual = reader.PSEReportString[reader.PSEReportString.Count - 1].Trim();
        //    Assert.AreEqual(expected, actual);
        //}

        [TestMethod]
        public void TestReaderFill_TradDate()
        {
            //PDFTextStripper stripper = new PDFTextStripper();
            var pdfSharpService = new PdfTextSharpService();
            PSEReportReader reader = new PSEReportReader(pdfSharpService.ExtractTextFromPdf(pdfDocPath));

            PSEDocument pd = new PSEDocument();
            reader.Fill(pd);

            DateTime expected = DateTime.Parse("05/05/2016");
            DateTime actual = pd.TradeDate;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestStringCollection_IndexOfString()
        {
            var sc = new StringCollection();
            sc.Add("Hello World!");
            sc.Add("Kamusta na ba?");
            sc.Add("Talaga lang ha");
            sc.Add("Kamote ka ba?");

            int expected = 2;
            int actual = sc.IndexOfString("Talaga");

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void TestParseDouble()
        {
            //FOREIGN BUYING      Php 7,979,297,874.23
            Nullable<double> expected = 7979297874.23;
            Nullable<double> actual = "FOREIGN BUYING      Php 7,979,297,874.23".ParseDouble();

            Assert.AreEqual(expected, actual, "ParseDouble first test failed");

            expected = -7979297874.23;
            actual = "FOREIGN BUYING      Php (7,979,297,874.23)".ParseDouble();

            Assert.AreEqual(expected, actual, "ParseDouble second test failed");
        }

        [TestMethod]
        public void TestParseInt()
        {
            //ODDLOT VOLUME:     :      1,043,508
            Nullable<int> expected = 1043508;
            Nullable<int> actual = "ODDLOT VOLUME:     :      1,043,508".ParseInt();

            Assert.AreEqual(expected, actual, "ParseInt first test failed");

            expected = -1043508;
            actual = "ODDLOT VOLUME:     :      (1,043,508)".ParseInt();

            Assert.AreEqual(expected, actual, "ParseInt second test failed");
        }

        [TestMethod]
        public void TestParseUlong()
        {
            //GRAND TOTAL 7,786,326,861,123 Php
            Nullable<ulong> expected = 7786326861123;
            Nullable<ulong> actual = "GRAND TOTAL 7,786,326,861,123 Php".ParseUlong();

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void TestParseLong()
        {
            //GRAND TOTAL 7,786,326,861,123 Php

            Nullable<long> expected = 7786326861123;
            Nullable<long> actual = "GRAND TOTAL 7,786,326,861,123 Php".ParseLong();

            Assert.AreEqual(expected, actual, "ParseLong first test failed");

            expected = -7786326861123;
            actual = "GRAND TOTAL (7,786,326,861,123) Php".ParseLong();

            Assert.AreEqual(expected, actual, "ParseLong second test failed");

        }


        [TestMethod]
        public void TestNameValueCollection_ContainsValue()
        {
            NameValueCollection sectorNameMap = new NameValueCollection();

            sectorNameMap.Add(PSEDocument.FINANCIAL, "F I N A N C I A L S");
            sectorNameMap.Add(PSEDocument.INDUSTRIAL, "I N D U S T R I A L");
            sectorNameMap.Add(PSEDocument.HOLDING, "H O L D I N G   F I R M S");
            sectorNameMap.Add(PSEDocument.MINING_OIL, "M I N I N G   &   O I L");
            sectorNameMap.Add(PSEDocument.PROPERTY, "P R O P E R T Y");
            sectorNameMap.Add(PSEDocument.SERVICE, "S E R V I C E S");

            bool expected = true;
            bool actual = sectorNameMap.ContainsValue("H O L D I N G   F I R M S");

            Assert.AreEqual(expected, actual, "NameValueCollection first test failed");

            expected = false;
            actual = sectorNameMap.ContainsValue("Hello World");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNameValueCollection_GetKey()
        {
            NameValueCollection sectorNameMap = new NameValueCollection();
            sectorNameMap.Add(PSEDocument.FINANCIAL, "F I N A N C I A L S");
            sectorNameMap.Add(PSEDocument.INDUSTRIAL, "I N D U S T R I A L");
            sectorNameMap.Add(PSEDocument.HOLDING, "H O L D I N G   F I R M S");
            sectorNameMap.Add(PSEDocument.MINING_OIL, "M I N I N G   &   O I L");
            sectorNameMap.Add(PSEDocument.PROPERTY, "P R O P E R T Y");
            sectorNameMap.Add(PSEDocument.SERVICE, "S E R V I C E S");
            sectorNameMap.Add(PSEDocument.PREFERRED, "P R E F E R R E D");
            sectorNameMap.Add(PSEDocument.SME, "SMALL AND MEDIUM ENTERPRISES");

            string expected = PSEDocument.HOLDING;
            string actual = sectorNameMap.GetKey("H O L D I N G   F I R M S");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestReader_ReportBody()
        {
            //PDFTextStripper stripper = new PDFTextStripper();
            var pdfSharpService = new PdfTextSharpService();
            PSEReportReader reader = new PSEReportReader(pdfSharpService.ExtractTextFromPdf(pdfDocPath));

            PSEDocument pd = new PSEDocument();
            reader.Fill(pd);

            //Bid Ask Open High Low Close Volume Value NFB/S

            //METROBANK MBT 78.75 78.8 78.95 79.1 78.55 78.8 2,050,170 161,601,172 7,609,520

            StockItem stock = pd.GetStock("MBT");
            double expected = 78.75;
            double actual = stock.Bid;
            Assert.AreEqual(expected, actual);

            expected = 78.95;
            actual = stock.Open;
            Assert.AreEqual(expected, actual);

            expected = 2050170;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 161601172;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 7609520;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //PSBANK PSB 98.55 102.9 102.9 102.9 102.9 102.9 50 5,145 -
            stock = pd.GetStock("PSB");
            expected = 102.9;
            actual = stock.High;
            Assert.AreEqual(expected, actual);

            actual = stock.Volume;
            expected = 50;
            Assert.AreEqual(expected, actual);

            actual = stock.Value;
            expected = 5145;
            Assert.AreEqual(expected, actual);

            actual = stock.NetForeignBuy;
            expected = 0;
            Assert.AreEqual(expected, actual);

            //MANULIFE MFC 580 610 - - - - - - -
            stock = pd.GetStock("MFC");
            expected = 0;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //ENERGY DEVT EDC 5.65 5.66 5.62 5.7 5.6 5.65 5,854,000 32,973,299 (19,410,534)
            stock = pd.GetStock("EDC");
            expected = 5.66;
            actual = stock.Ask;
            Assert.AreEqual(expected, actual);

            expected = 5.6;
            actual = stock.Low;
            Assert.AreEqual(expected, actual);

            expected = 5854000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 32973299;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -19410534;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            ////COSMOS                   CBC     - - - - - - - - -
            //stock = pd.GetStock("CBC");
            //expected = 0;
            //actual = stock.High;
            //Assert.AreEqual(expected, actual);

            //actual = stock.Volume;
            //Assert.AreEqual(expected, actual);

            //actual = stock.Value;
            //Assert.AreEqual(expected, actual);

            //actual = stock.NetForeignBuy;
            //Assert.AreEqual(expected, actual);    

            //VULCAN INDL VUL 1.26 1.27 1.25 1.3 1.25 1.27 369,000 463,800 -
            stock = pd.GetStock("VUL");
            expected = 1.27;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 369000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 463800;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //ANSCOR ANS 5.92 6 6 6 5.97 5.97 3,000 17,970 -
            stock = pd.GetStock("ANS");
            expected = 5.92;
            actual = stock.Bid;
            Assert.AreEqual(expected, actual);

            expected = 6;
            actual = stock.Open;
            Assert.AreEqual(expected, actual);

            expected = 3000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 17970;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //SEAFRONT RES SPM 2.13 2.34 - - - - - - -
            stock = pd.GetStock("SPM");
            expected = 2.34;
            actual = stock.Ask;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //SINOPHIL                 SINO    0.305 0.31 0.31 0.31 0.305 0.305 1,190,000 366,450 -
            //stock = pd.GetStock("SINO");
            //expected = 0.31;
            //actual = stock.High;
            //Assert.AreEqual(expected, actual);

            //expected = 0.305;
            //actual = stock.Close;
            //Assert.AreEqual(expected, actual);

            //expected = 1190000;
            //actual = stock.Volume;
            //Assert.AreEqual(expected, actual);

            //expected = 366450;
            //actual = stock.Value;
            //Assert.AreEqual(expected, actual);

            //expected = 0;
            //actual = stock.NetForeignBuy;
            //Assert.AreEqual(expected, actual);


            //AYALA LAND ALI 33.8 33.85 33.7 34 33.65 33.8 11,833,600 400,085,500 (62,456,645)
            stock = pd.GetStock("ALI");
            expected = 33.8;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 11833600;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 400085500;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -62456645;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);


            //MARSTEEL B               MCB     - - - - - - - - -
            //stock = pd.GetStock("MCB");
            //expected = 0;
            //actual = stock.Bid;
            //Assert.AreEqual(expected, actual);

            //actual = stock.Open;
            //Assert.AreEqual(expected, actual);

            //actual = stock.Close;
            //Assert.AreEqual(expected, actual);

            //actual = stock.Volume;
            //Assert.AreEqual(expected, actual);

            //actual = stock.Value;
            //Assert.AreEqual(expected, actual);

            //actual = stock.NetForeignBuy;
            //Assert.AreEqual(expected, actual);


            //SM DEVT                  SMDC 9.35 9.4 8.64 9.8 8.64 9.35 3,641,200 33,750,420 633,735
            //stock = pd.GetStock("SMDC");
            //expected = 9.35;
            //actual = stock.Bid;
            //Assert.AreEqual(expected, actual);

            //expected = 8.64;
            //actual = stock.Low;
            //Assert.AreEqual(expected, actual);

            //expected = 3641200;
            //actual = stock.Volume;
            //Assert.AreEqual(expected, actual);

            //expected = 33750420;
            //actual = stock.Value;
            //Assert.AreEqual(expected, actual);

            //expected = 633735;
            //actual = stock.NetForeignBuy;
            //Assert.AreEqual(expected, actual);

            //GMA NETWORK GMA7 6.75 6.78 6.75 6.78 6.71 6.78 74,300 501,102 -
            stock = pd.GetStock("GMA7");
            expected = 6.71;
            actual = stock.Low;
            Assert.AreEqual(expected, actual);

            expected = 74300;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 501102;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //DIGITEL                  DGTL 1.51 1.52 1.53 1.54 1.51 1.52 36,056,000 54,884,680 (686,960)
            //stock = pd.GetStock("DGTL");
            //expected = 1.53;
            //actual = stock.Open;
            //Assert.AreEqual(expected, actual);

            //expected = 36056000;
            //actual = stock.Volume;
            //Assert.AreEqual(expected, actual);

            //expected = 54884680;
            //actual = stock.Value;
            //Assert.AreEqual(expected, actual);

            //expected = -686960;
            //actual = stock.NetForeignBuy;
            //Assert.AreEqual(expected, actual);

            //WATERFRONT WPI 0.33 0.34 0.33 0.33 0.33 0.33 430,000 141,900 -
            stock = pd.GetStock("WPI");
            expected = 0.33;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 430000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 141900;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //GEOGRACE GEO 0.28 0.29 0.28 0.285 0.28 0.285 450,000 126,050 -
            stock = pd.GetStock("GEO");
            expected = 0.285;
            actual = stock.High;
            Assert.AreEqual(expected, actual);

            expected = 450000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 126050;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //SEMIRARA MINING SCC 126.4 126.5 127 127 126.1 126.5 240,020 30,357,793 (10,789,654)
            stock = pd.GetStock("SCC");
            expected = 126.5;
            actual = stock.Ask;
            Assert.AreEqual(expected, actual);

            expected = 240020;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 30357793;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -10789654;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //PHILODRILL OV 0.012 0.013 0.012 0.012 0.012 0.012 13,200,000 158,400 -
            stock = pd.GetStock("OV");
            expected = 0.012;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 13200000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 158400;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

        }


        [TestMethod]
        public void TestReader_SectorSummary()
        {
            //PDFTextStripper stripper = new PDFTextStripper();
            IPdfService pdfService = new PdfTextSharpService();
            var reader = new PSEReportReader(pdfService.ExtractTextFromPdf(pdfDocPath));

            var pd = new PSEDocument();
            reader.Fill(pd);
            
            // psei
            SectorItem psei = pd.GetSector(PSEDocument.PSEI);
                       
            ulong expected = 3150242905;
            ulong actual = psei.Volume;

            Assert.AreEqual(expected, actual, "PSEi volume not equal");

            double expected_value = 5634576802.26;
            double actual_value = pd.GetSector(PSEDocument.PSEI).Value;

            Assert.AreEqual(expected_value, actual_value, "PSEi value not equal");

            expected_value = 7007.63;
            actual_value = psei.Open;
            Assert.AreEqual(expected_value, actual_value, "PSEi open not equal");

            expected_value = 7011.28;
            actual_value = psei.High;
            Assert.AreEqual(expected_value, actual_value, "PSEi high not equal");

            expected_value = 6986.86;
            actual_value = psei.Low;
            Assert.AreEqual(expected_value, actual_value, "PSEi low not equal");

            expected_value = 6999.75;
            actual_value = psei.Close;
            Assert.AreEqual(expected_value, actual_value, "PSEi close not equal");

            
            // financial
            SectorItem financial = pd.GetSector(PSEDocument.FINANCIAL);

            expected = 8105540;
            actual = financial.Volume;

            Assert.AreEqual(expected, actual, "Financial volume not equal");

            expected_value = 755542372.19;
            actual_value = financial.Value;

            Assert.AreEqual(expected, actual, "Financial value not equal");

            //913.01 935.52 909.34 935.52 2.47 22.51 24,780,801 882,690,827.9
            expected_value = 1585.35;
            actual_value = financial.Open;

            Assert.AreEqual(expected_value, actual_value, "Financial open not equal");

            expected_value = 1585.39;
            actual_value = financial.High;

            Assert.AreEqual(expected_value, actual_value, "Financial high not equal");

            expected_value = 1577.85;
            actual_value = financial.Low;

            Assert.AreEqual(expected_value, actual_value, "Financial low not equal");

            expected_value = 1583.44;
            actual_value = financial.Close;

            Assert.AreEqual(expected_value, actual_value);


            // mining
            SectorItem mining = pd.GetSector(PSEDocument.MINING_OIL);

            expected = 2528326978;
            actual = mining.Volume;

            Assert.AreEqual(expected, actual, "Mining volume not equal");

            expected_value = 143087427.64;
            actual_value = mining.Value;

            Assert.AreEqual(expected, actual, "Mining value not equal");

            //11,644.77 12,468.64 11,644.77 12,387.7 7.97 914.68 3,832,444,034 977,394,265.25

            expected_value = 10885.19;
            actual_value = mining.Open;

            Assert.AreEqual(expected_value, actual_value);

            expected_value = 10886.63;
            actual_value = mining.High;

            Assert.AreEqual(expected_value, actual_value);

            expected_value = 10700.58;
            actual_value = mining.Low;

            Assert.AreEqual(expected_value, actual_value);

            expected_value = 10740.09;
            actual_value = mining.Close;

            Assert.AreEqual(expected_value, actual_value);

            SectorItem pse = pd.GetSector(PSEDocument.PSEI);
            expected_value = -616052104.21;
            actual_value = pse.NetForeignBuy;

            Assert.AreEqual(expected_value, actual_value);

        }

        //[TestMethod]
        public void TestDownloadHistoricalData()
        {
            const string symbol = "MEG";
            const int numYears = 10;
            string downloadStr = "http://www.pse.com.ph/servlet/PSEChartServlet?securitySymbol=%s&years=%f";
            downloadStr = downloadStr.Replace("%s", symbol).Replace("%f", numYears.ToString());

            Uri downloadUri = new Uri(downloadStr);
            HistoricalDataDownloader downloader = new HistoricalDataDownloader(downloadUri);
            downloader.Download();

            HistoricalDataReader reader = downloader.GetReader();

            CSVOutputSettings outputSettings = new CSVOutputSettings();
            outputSettings.CSVFormat = "S,D,O,H,L,C,V,F";
            outputSettings.Delimiter = ",";
            outputSettings.OutputDirectory = "C:\\Users\\yeahbah\\Documents\\projects";
            outputSettings.Filename = symbol +".csv";
            outputSettings.DateFormat = "MM/DD/YYYY";

            reader.ToCSV(outputSettings);
                 
        }

        [TestMethod]
        public void TestDownloadHistoricalDataUsingUtil()
        {
            List<string> stockList = new List<string>();
            stockList.Add("MEG");
            stockList.Add("DMC");
            stockList.Add("PX");
            stockList.Add("NIKL");
            stockList.Add("GEO");
            stockList.Add("SCC");
            stockList.Add("ALI");
            stockList.Add("AC");
            stockList.Add("EEI");
            stockList.Add("GERI");
            stockList.Add("FGEN");
            stockList.Add("FPH");
            stockList.Add("ANI");
            stockList.Add("ELI");
            stockList.Add("LPZ");
            stockList.Add("MUSX");
            stockList.Add("LC");

            CSVOutputSettings outputSettings = new CSVOutputSettings();
            outputSettings.CSVFormat = "S,D,O,H,L,C,V,F";
            outputSettings.Delimiter = ",";
            outputSettings.OutputDirectory = "C:\\Users\\yeahbah\\Documents\\projects";
            outputSettings.Filename = "HistoricalData.csv";
            outputSettings.DateFormat = "MM/DD/YYYY";


            //Util.DownloadAndConvertHistoricalData(stockList, 20, outputSettings,
            //    null);
                       
        }

    }
}
