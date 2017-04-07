using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using PSEGetLib.DocumentModel;
using PSEGetLib.Interfaces;
using System.Globalization;

namespace PSEGetLib
{
    public class PSEReportReader2 : IPSEReportReader
    {
        private string _reportString;
        private PSEDocument _pseDocument;
        private NameValueCollection sectorNameMap = new NameValueCollection();

        public void Fill(PSEDocument pseDocument)
        {
            throw new NotImplementedException();
        }

        public void Fill(PSEDocument pseDocument, string pseReportFile)
        {
            _reportString = pseReportFile;
            _pseDocument = pseDocument;

            pseDocument.TradeDate = GetTradeDate();
            CleanupReportString();

            ParseReportBody();
            ParseSectorSummary();
            ParseReportMisc();            

        }

        private void ParseReportMisc()
        {
            // advance / declines
            var pattern = @"(NO.\sOF\s)([A-Z\s]+\:\s+)([\d.,]+)";
            MatchCollection matches = Regex.Matches(_reportString, pattern);
            foreach(Match match in matches)
            {                                
                _pseDocument.NumAdvance = match.Groups[2].Value == "ADVANCES" ? int.Parse(match.Groups[3].Value) : 0;                
                _pseDocument.NumDeclines = match.Groups[2].Value == "DECLINES" ? int.Parse(match.Groups[3].Value) : 0;
                _pseDocument.NumDeclines = match.Groups[2].Value == "UNCHANGED" ? int.Parse(match.Groups[3].Value) : 0;
                _pseDocument.NumDeclines = match.Groups[2].Value == "TRADES" ? int.Parse(match.Groups[3].Value) : 0;
            }

            // oddlot
            pattern = @"(ODDLOT\s)([A-Z\s]+):\s+(Php\s)?([\d.,]+)";
            matches = Regex.Matches(_reportString, pattern);

            // volume
            GroupCollection matchGroup = matches[0].Groups;
            _pseDocument.OddLotVolume = matchGroup[2].Value == "VOLUME" ? ulong.Parse(matchGroup[3].Value) : 0;

            // value
            matchGroup = matches[1].Groups;
            _pseDocument.OddLotValue = matchGroup[2].Value == "VOLUME" ? ulong.Parse(matchGroup[3].Value) : 0;

            // block sale
            pattern = @"(BLOCK SALE\s)([A-Z\s]+):\s+(Php\s)?([\d.,]+)";
            matches = Regex.Matches(_reportString, pattern);
            matchGroup = matches[0].Groups;

            // volume
            matchGroup = matches[0].Groups;
            _pseDocument.BlockSaleVolume = matchGroup[2].Value == "VOLUME" ? ulong.Parse(matchGroup[3].Value) : 0;

            // value
            matchGroup = matches[0].Groups;
            _pseDocument.BlockSaleValue = matchGroup[2].Value == "VOLUME" ? ulong.Parse(matchGroup[3].Value) : 0;

            // TODO: parse block sale
        }

        private void ParseSectorSummary()
        {
            var pattern = @"(Financials|Industrials|Holding Firms|Property|Services|Mining & Oil|SME|ETF|PSEi|All Shares|GRAND TOTAL)([0-9\.\s,\(\)\-]+)";

            MatchCollection matches = Regex.Matches(_reportString, pattern);
            for (var sectorIndex = 0; sectorIndex < matches.Count; sectorIndex++)
            {
                GroupCollection matchCol = matches[sectorIndex].Groups;

                // index match
                SectorItem sectorItem = _pseDocument.Sectors[sectorIndex];
                string[] row = matchCol[1].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Reverse<string>().ToArray<string>();

                if (matchCol[0].Value == "PSEi" || matchCol[0].Value == "All Shares")
                {
                    sectorItem.PointChange = double.Parse(row[0], NumberStyles.Any);
                    sectorItem.PercentChange = double.Parse(row[1], NumberStyles.Any);
                    sectorItem.Close = double.Parse(row[2], NumberStyles.Any);
                    sectorItem.Low = double.Parse(row[3], NumberStyles.Any);
                    sectorItem.High = double.Parse(row[4], NumberStyles.Any);
                    sectorItem.Open = double.Parse(row[5], NumberStyles.Any);
                }
                else
                {
                    sectorItem.Open = double.Parse(row[7], NumberStyles.Any);
                    sectorItem.High = double.Parse(row[6], NumberStyles.Any);
                    sectorItem.Low = double.Parse(row[5], NumberStyles.Any);
                    sectorItem.Close = double.Parse(row[4], NumberStyles.Any);
                    sectorItem.PercentChange = double.Parse(row[3], NumberStyles.Any);
                    sectorItem.PointChange = double.Parse(row[2], NumberStyles.Any);
                    sectorItem.Volume = ulong.Parse(row[1], NumberStyles.Any);
                    sectorItem.Value = double.Parse(row[0], NumberStyles.Any);
                }
                
            }

            // foreign buy/sell
            pattern = @"(FOREIGN\s+)(BUYING|SELLING):\s+Php\s([0-9\.\s,\(\)\-]+)";
            matches = Regex.Matches(_reportString, pattern);
            Nullable<double> nfb = null;
            Nullable<double> nfs = null;

            nfb = matches[0].Groups[2].Value.ParseDouble();
            nfs = matches[1].Groups[2].Value.ParseDouble();
            SectorItem psei = _pseDocument.GetSector(PSEDocument.PSEI);
            psei.NetForeignBuy = Math.Round((double)nfb - (double)nfs, 2);

            //calculate sector netforeign buying/selling
            // TODO: make this internal
            foreach (SectorItem sectorItem in _pseDocument.Sectors)
            {
                if (sectorItem.Symbol == "^PSEi")
                    continue;
                sectorItem.NetForeignBuy = 0;
                foreach (SubSectorItem subSectorItem in sectorItem.SubSectors)
                {
                    foreach (StockItem stockItem in subSectorItem.Stocks)
                    {
                        sectorItem.NetForeignBuy += stockItem.NetForeignBuy;
                    }
                }
            }

        }

        private void ParseReportBody()
        {
            string[] lines = _reportString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            SectorItem sector = null;
            SubSectorItem subSector = null;
            foreach (var line in lines)
            {
                // create the sector object
                var pattern = @"([A-Z&\.,]\s)+([A-Z]-?)\n";
                Match sectorTextMatch = Regex.Match(line, pattern);
                if (sectorTextMatch.Success)
                {
                    sector = _pseDocument.GetSector(sectorNameMap.GetKey(sectorTextMatch.Value));
                    continue;
                }

                // sub sector
                // proceed to next line if matched
                pattern = @"(\*+\s+)(.+)(.\*+\s+)";
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    subSector = new SubSectorItem();
                    subSector.Name = match.Groups[1].Value;
                    sector.SubSectors.Add(subSector);
                    continue;
                }

                // will only match stocks that has trading activity
                pattern = @"\b([\w]+\s)+\b([0-9\.\s,\(\)\-]+)";
                if (Regex.Match(line, pattern).Success)
                {
                    // found the stock line
                    StockItem stock = ParseStockLine(line, sector);
                    subSector.Stocks.Add(stock);
                }

                // end of the line
                pattern = @"(TOTAL MAIN BOARD.+\s+)(.+)";
                if (Regex.Match(line, pattern).Success)
                    break;
            }
        }

        private StockItem ParseStockLine(string stockText, SectorItem sector)
        {
            StockItem stock = new StockItem(_pseDocument, sector);

            string[] stockInfo = stockText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // easier to work from end of the array first
            stockInfo = stockInfo.Reverse<string>().ToArray<string>();
           
            double? doubleValue = stockInfo[0].ParseDouble();
            stock.NetForeignBuy = doubleValue.HasValue ? (double)doubleValue : 0;

            doubleValue = stockInfo[1].ParseDouble();
            stock.Value = doubleValue.HasValue ? (double)doubleValue : 0;

            ulong? uLongValue = stockInfo[2].ParseUlong();
            stock.Volume = uLongValue.HasValue ? (ulong)uLongValue : 0;

            doubleValue = stockInfo[3].ParseDouble();
            stock.Close = doubleValue.HasValue ? (double)doubleValue : 0;

            doubleValue = stockInfo[4].ParseDouble();
            stock.Low = doubleValue.HasValue ? (double)doubleValue : 0;

            doubleValue = stockInfo[5].ParseDouble();
            stock.High = doubleValue.HasValue ? (double)doubleValue : 0;

            doubleValue = stockInfo[6].ParseDouble();
            stock.Open = doubleValue.HasValue ? (double)doubleValue : 0;

            doubleValue = stockInfo[7].ParseDouble();
            stock.Ask = doubleValue.HasValue ? (double)doubleValue : 0;

            doubleValue = stockInfo[8].ParseDouble();
            stock.Bid = doubleValue.HasValue ? (double)doubleValue : 0;

            stock.Symbol = stockInfo[9];

            return stock;
        }

        private void CleanupReportString()
        {
            // remove header text
            var textPattern = @"(\s+)(The Philippine Stock Exchange, Inc)(\s+.+\s+)((?i)(April|May|June|July|August|September|October|November|December)\s+\d+.,.\d+)\s+";
            _reportString = Regex.Replace(_reportString, textPattern, string.Empty);

            // remove column names
            textPattern = @"(MAIN BOARD\s+)?(Name Symbol)(.+\s+){4}";
            _reportString = Regex.Replace(_reportString, textPattern, string.Empty);

            // total main board lines
            //textPattern = @"(TOTAL MAIN BOARD.+\s+)(.+)";
            //_reportString = Regex.Replace(_reportString, textPattern, string.Empty);
        }

        private DateTime GetTradeDate()
        {
            Match match = Regex.Match(_reportString, @"(?i)(April|May|June|July|August|September|October|November|December)\s+\d+.,.\d+");
            if (match.Success)
            {
                var dateStr = match.Value;
                return DateTime.Parse(dateStr);
            }

            throw new Exception("Unable to locate the report date.");
        }


        private void InitializeSectorMapping()
        {
            //initialize sector name mapping
            sectorNameMap.Add(PSEDocument.FINANCIAL, "F I N A N C I A L S");
            sectorNameMap.Add(PSEDocument.INDUSTRIAL, "I N D U S T R I A L");
            sectorNameMap.Add(PSEDocument.HOLDING, "H O L D I N G   F I R M S");
            sectorNameMap.Add(PSEDocument.MINING_OIL, "M I N I N G   &   O I L");
            sectorNameMap.Add(PSEDocument.PROPERTY, "P R O P E R T Y");
            sectorNameMap.Add(PSEDocument.SERVICE, "S E R V I C E S");
            sectorNameMap.Add(PSEDocument.PREFERRED, "P R E F E R R E D");          
            sectorNameMap.Add(PSEDocument.DEPOSITORY_RECEIPTS, "P H I L .   D E P O S I T A R Y   R E C E I P T S");
            sectorNameMap.Add(PSEDocument.WARRANT, "W A R R A N T S");
            sectorNameMap.Add(PSEDocument.SME, "S M A L L   &   M E D I U M   E N T E R P R I S E S");
            sectorNameMap.Add(PSEDocument.ETF, "E X C H A N G E   T R A D E D   F U N D S");
        }

    }
}
