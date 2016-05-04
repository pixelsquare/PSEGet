using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using org.apache.pdfbox.pdmodel;
//using org.apache.pdfbox.util;
using PSEGetLib;
using PSEGetLib.Converters;
using PSEGetLib.DocumentModel;

namespace PSEGetTest
{
    /// <summary>
    ///     Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ReportReaderTest_11062013
    {
        private readonly PDDocument doc;

        public ReportReaderTest_11062013()
        {
            //
            // TODO: Add constructor logic here
            //            
            //doc = PDDocument.load(@"C:\Users\Arnold\Documents\Projects\PSEGet3\trunk\PSEGetTest\stockQuotes_11062013.pdf");
            doc = PDDocument.load(@"C:\PSEGet\Reports\stockQuotes_12172013.pdf");
        }

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

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
            var stripper = new PDFTextStripper();

            var reader = new PSEReportReader(stripper.getText(doc));

            string expected = "The Philippine Stock Exchange, Inc";
            string actual = reader.PSEReportString[0].Trim();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestReaderLastLine()
        {
            var stripper = new PDFTextStripper();
            var reader = new PSEReportReader(stripper.getText(doc).TrimEnd());

            string expected = "*** Grand total includes main,oddlot and block sale transactions";
            string actual = reader.PSEReportString[reader.PSEReportString.Count - 1].Trim();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestReaderFill_TradDate()
        {
            var stripper = new PDFTextStripper();
            var reader = new PSEReportReader(stripper.getText(doc).TrimEnd());

            var pd = new PSEDocument();
            reader.Fill(pd);

            DateTime expected = DateTime.Parse("11/06/2013");
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
            double? expected = 7979297874.23;
            double? actual = "FOREIGN BUYING      Php 7,979,297,874.23".ParseDouble();

            Assert.AreEqual(expected, actual, "ParseDouble first test failed");

            expected = -7979297874.23;
            actual = "FOREIGN BUYING      Php (7,979,297,874.23)".ParseDouble();

            Assert.AreEqual(expected, actual, "ParseDouble second test failed");
        }

        [TestMethod]
        public void TestParseInt()
        {
            //ODDLOT VOLUME:     :      1,043,508
            int? expected = 1043508;
            int? actual = "ODDLOT VOLUME:     :      1,043,508".ParseInt();

            Assert.AreEqual(expected, actual, "ParseInt first test failed");

            expected = -1043508;
            actual = "ODDLOT VOLUME:     :      (1,043,508)".ParseInt();

            Assert.AreEqual(expected, actual, "ParseInt second test failed");
        }

        [TestMethod]
        public void TestParseUlong()
        {
            //GRAND TOTAL 7,786,326,861,123 Php
            ulong? expected = 7786326861123;
            ulong? actual = "GRAND TOTAL 7,786,326,861,123 Php".ParseUlong();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseLong()
        {
            //GRAND TOTAL 7,786,326,861,123 Php

            long? expected = 7786326861123;
            long? actual = "GRAND TOTAL 7,786,326,861,123 Php".ParseLong();

            Assert.AreEqual(expected, actual, "ParseLong first test failed");

            expected = -7786326861123;
            actual = "GRAND TOTAL (7,786,326,861,123) Php".ParseLong();

            Assert.AreEqual(expected, actual, "ParseLong second test failed");
        }


        [TestMethod]
        public void TestNameValueCollection_ContainsValue()
        {
            var sectorNameMap = new NameValueCollection();

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
            var sectorNameMap = new NameValueCollection();
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
            var stripper = new PDFTextStripper();
            var reader = new PSEReportReader(stripper.getText(doc).TrimEnd());

            var pd = new PSEDocument();
            reader.Fill(pd);

            //Bid Ask Open High Low Close Volume Value NFB/S

            //METROBANK                MBT     74.2 74.75 71.2 74.75 71.05 74.75 3,098,980 226,992,448.5 4,289,723

            StockItem stock = pd.GetStock("MBT");
            double expected = 74.2;
            double actual = stock.Bid;
            Assert.AreEqual(expected, actual);

            expected = 71.2;
            actual = stock.Open;
            Assert.AreEqual(expected, actual);

            expected = 3098980;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 226992448.5;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 4289723;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //PSBANK                   PSB     51.5 - - - - - - - -
            stock = pd.GetStock("PSB");
            expected = 0;
            actual = stock.High;
            Assert.AreEqual(expected, actual);

            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //MANULIFE                 MFC     557 559 556 560 556 559 160 89,380 (16,680)
            stock = pd.GetStock("MFC");
            expected = 559;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 160;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 89380;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -16680;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //ENERGY DEVT.             EDC     6.04 6.06 5.76 6.06 5.76 6.06 84,308,100 499,835,886 18,306,534
            stock = pd.GetStock("EDC");
            expected = 6.06;
            actual = stock.Ask;
            Assert.AreEqual(expected, actual);

            expected = 5.76;
            actual = stock.Low;
            Assert.AreEqual(expected, actual);

            expected = 84308100;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 499835886;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 18306534;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //COSMOS                   CBC     - - - - - - - - -
            stock = pd.GetStock("CBC");
            expected = 0;
            actual = stock.High;
            Assert.AreEqual(expected, actual);

            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //VULCAN IND`L             VUL     0.81 0.86 0.79 0.86 0.79 0.86 140,000 118,170 -
            stock = pd.GetStock("VUL");
            expected = 0.86;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 140000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 118170;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //ANSCOR                   ANS     3.12 3.15 3.12 3.15 3.12 3.15 227,000 710,740 62,400
            stock = pd.GetStock("ANS");
            expected = 3.12;
            actual = stock.Bid;
            Assert.AreEqual(expected, actual);

            expected = 3.12;
            actual = stock.Open;
            Assert.AreEqual(expected, actual);

            expected = 227000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 710740;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 62400;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //SEAFRONT RES.            SPM     0.95 1.2 - - - - - - -
            stock = pd.GetStock("SPM");
            expected = 1.2;
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
            stock = pd.GetStock("SINO");
            expected = 0.31;
            actual = stock.High;
            Assert.AreEqual(expected, actual);

            expected = 0.305;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 1190000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 366450;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);


            //AYALA LAND               ALI     17.24 17.26 17.3 17.32 17.1 17.24 7,144,400 123,230,996 (79,735,452)
            stock = pd.GetStock("ALI");
            expected = 17.24;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 7144400;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 123230996;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -79735452;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);


            //MARSTEEL B               MCB     - - - - - - - - -
            stock = pd.GetStock("MCB");
            expected = 0;
            actual = stock.Bid;
            Assert.AreEqual(expected, actual);

            actual = stock.Open;
            Assert.AreEqual(expected, actual);

            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);


            //SM DEVT                  SMDC 9.35 9.4 8.64 9.8 8.64 9.35 3,641,200 33,750,420 633,735
            stock = pd.GetStock("SMDC");
            expected = 9.35;
            actual = stock.Bid;
            Assert.AreEqual(expected, actual);

            expected = 8.64;
            actual = stock.Low;
            Assert.AreEqual(expected, actual);

            expected = 3641200;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 33750420;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 633735;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //GMA NETWORK              GMA7 7.4 7.45 7.4 7.45 7 7.4 408,500 3,010,200 -
            stock = pd.GetStock("GMA7");
            expected = 7;
            actual = stock.Low;
            Assert.AreEqual(expected, actual);

            expected = 408500;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 3010200;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //DIGITEL                  DGTL 1.51 1.52 1.53 1.54 1.51 1.52 36,056,000 54,884,680 (686,960)
            stock = pd.GetStock("DGTL");
            expected = 1.53;
            actual = stock.Open;
            Assert.AreEqual(expected, actual);

            expected = 36056000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 54884680;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -686960;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //WATERFRONT               WPI     0.25 0.265 0.27 0.27 0.27 0.27 50,000 13,500 -
            stock = pd.GetStock("WPI");
            expected = 0.27;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 50000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 13500;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //GEOGRACE                 GEO     0.72 0.73 0.7 0.8 0.7 0.73 58,761,000 43,384,240 (7,200)
            stock = pd.GetStock("GEO");
            expected = 0.8;
            actual = stock.High;
            Assert.AreEqual(expected, actual);

            expected = 58761000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 43384240;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -7200;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //SEMIRARA MINING          SCC     142.1 143 139 145 139 143 569,960 80,377,827 (41,123,492)
            stock = pd.GetStock("SCC");
            expected = 143;
            actual = stock.Ask;
            Assert.AreEqual(expected, actual);

            expected = 569960;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 80377827;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = -41123492;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);

            //PHILODRILL A             OV      0.014 0.015 0.014 0.015 0.014 0.014 173,200,000 2,525,900 -
            stock = pd.GetStock("OV");
            expected = 0.014;
            actual = stock.Close;
            Assert.AreEqual(expected, actual);

            expected = 173200000;
            actual = stock.Volume;
            Assert.AreEqual(expected, actual);

            expected = 2525900;
            actual = stock.Value;
            Assert.AreEqual(expected, actual);

            expected = 0;
            actual = stock.NetForeignBuy;
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestReader_SectorSummary()
        {
            var stripper = new PDFTextStripper();
            var reader = new PSEReportReader(stripper.getText(doc).TrimEnd());

            var pd = new PSEDocument();
            reader.Fill(pd);

            // psei
            SectorItem psei = pd.GetSector(PSEDocument.PSEI);

            ulong expected = 7786326861;
            ulong actual = psei.Volume;

            Assert.AreEqual(expected, actual, "PSEi volume not equal");

            double expected_value = 12960265679.1516;
            double actual_value = pd.GetSector(PSEDocument.PSEI).Value;

            Assert.AreEqual(expected_value, actual_value, "PSEi value not equal");

            expected_value = 3979.97;
            actual_value = psei.Open;
            Assert.AreEqual(expected_value, actual_value, "PSEi open not equal");

            expected_value = 4053.32;
            actual_value = psei.High;
            Assert.AreEqual(expected_value, actual_value, "PSEi high not equal");

            expected_value = 3979.97;
            actual_value = psei.Low;
            Assert.AreEqual(expected_value, actual_value, "PSEi low not equal");

            expected_value = 4053.32;
            actual_value = psei.Close;
            Assert.AreEqual(expected_value, actual_value, "PSEi close not equal");


            // financial
            SectorItem financial = pd.GetSector(PSEDocument.FINANCIAL);

            expected = 24780801;
            actual = financial.Volume;

            Assert.AreEqual(expected, actual, "Financial volume not equal");

            expected_value = 882690827.9;
            actual_value = financial.Value;

            Assert.AreEqual(expected, actual, "Financial value not equal");

            //913.01 935.52 909.34 935.52 2.47 22.51 24,780,801 882,690,827.9
            expected_value = 913.01;
            actual_value = financial.Open;

            Assert.AreEqual(expected_value, actual_value, "Financial open not equal");

            expected_value = 935.52;
            actual_value = financial.High;

            Assert.AreEqual(expected_value, actual_value, "Financial high not equal");

            expected_value = 909.34;
            actual_value = financial.Low;

            Assert.AreEqual(expected_value, actual_value, "Financial low not equal");

            expected_value = 935.52;
            actual_value = financial.Close;

            Assert.AreEqual(expected_value, actual_value);


            // mining
            SectorItem mining = pd.GetSector(PSEDocument.MINING_OIL);

            expected = 3832444034;
            actual = mining.Volume;

            Assert.AreEqual(expected, actual, "Mining volume not equal");

            expected_value = 977394265.25;
            actual_value = mining.Value;

            Assert.AreEqual(expected, actual, "Mining value not equal");

            //11,644.77 12,468.64 11,644.77 12,387.7 7.97 914.68 3,832,444,034 977,394,265.25

            expected_value = 11644.77;
            actual_value = mining.Open;

            Assert.AreEqual(expected_value, actual_value);

            expected_value = 12468.64;
            actual_value = mining.High;

            Assert.AreEqual(expected_value, actual_value);

            expected_value = 11644.77;
            actual_value = mining.Low;

            Assert.AreEqual(expected_value, actual_value);

            expected_value = 12387.7;
            actual_value = mining.Close;

            Assert.AreEqual(expected_value, actual_value);

            SectorItem pse = pd.GetSector(PSEDocument.PSEI);
            expected_value = 1938423893.11;
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

            var downloadUri = new Uri(downloadStr);
            var downloader = new HistoricalDataDownloader(downloadUri);
            downloader.Download();

            HistoricalDataReader reader = downloader.GetReader();

            var outputSettings = new CSVOutputSettings();
            outputSettings.CSVFormat = "S,D,O,H,L,C,V,F";
            outputSettings.Delimiter = ",";
            outputSettings.OutputDirectory = "C:\\Users\\yeahbah\\Documents\\projects";
            outputSettings.Filename = symbol + ".csv";
            outputSettings.DateFormat = "MM/DD/YYYY";

            reader.ToCSV(outputSettings);
        }

        [TestMethod]
        public void TestDownloadHistoricalDataUsingUtil()
        {
            var stockList = new List<string>();
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

            var outputSettings = new CSVOutputSettings();
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