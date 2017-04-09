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
using System.Diagnostics;

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
            InitSectors();

			pseDocument.TradeDate = GetTradeDate();
			if (pseDocument.TradeDate.CompareTo(new DateTime(2017, 4, 4)) < 0)
				throw new EUnsupportedReportFormat("Unsupported report format. Quotation reports prior to April 6, 2017 are not supported.");
			
			CleanupReportString();

			ParseReportBody();
			ParseSectorSummary();
			ParseForeignTransaction();
			ParseReportMisc();

		}

		private void ParseForeignTransaction()
		{
			// foreign buy/sell
			var pattern = @"(FOREIGN\s+)(BUYING|SELLING):\s+Php\s([0-9\.\s,\(\)\-]+)";
			MatchCollection matches = Regex.Matches(_reportString, pattern);
			Nullable<double> nfb = null;
			Nullable<double> nfs = null;

			nfb = matches[0].Groups[3].Value.ParseDouble();
			nfs = matches[1].Groups[3].Value.ParseDouble();
			SectorItem psei = _pseDocument.GetSector(PSEDocument.PSEI);
			psei.NetForeignBuy = Math.Round((double)nfb - (double)nfs, 2);
		}

		private void ParseReportMisc()
		{
			// advance / declines
			var pattern = @"(NO.\sOF\s)([A-Z\s]+)\:\s+([\d.,]+)";
			MatchCollection matches = Regex.Matches(_reportString, pattern);

			for (var i = 0; i < matches.Count - 1; i++)
			{
				Match match = matches[i];
				var matchValue = int.Parse(match.Groups[3].Value, NumberStyles.Any);
				switch (match.Groups[2].Value)
				{
					case "ADVANCES": _pseDocument.NumAdvance = matchValue;
						break;
					case "DECLINES": _pseDocument.NumDeclines = matchValue;
						break;
					case "UNCHANGED": _pseDocument.NumUnchanged = matchValue;
						break;
					case "TRADES": _pseDocument.NumTrades = matchValue;
						break;
					case "TRADED ISSUES": _pseDocument.NumTraded = matchValue;
						break;
				}				 
			}

			// oddlot
			pattern = @"(ODDLOT\s)([A-Z\s]+):\s+(Php\s)?([\d.,]+)";
			matches = Regex.Matches(_reportString, pattern);

			// volume
			GroupCollection matchGroup = matches[0].Groups;
			_pseDocument.OddLotVolume = matchGroup[2].Value == "VOLUME" ? ulong.Parse(matchGroup[4].Value, NumberStyles.Any) : 0;

			// value
			matchGroup = matches[1].Groups;
			_pseDocument.OddLotValue = matchGroup[2].Value == "VALUE" ? double.Parse(matchGroup[4].Value, NumberStyles.Any) : 0;

			// block sale
			pattern = @"(BLOCK SALE\s)([A-Z\s]+):\s+(Php\s)?([\d.,]+)";
			matches = Regex.Matches(_reportString, pattern);

			// volume
			matchGroup = matches[0].Groups;
			_pseDocument.BlockSaleVolume = matchGroup[2].Value == "VOLUME" ? ulong.Parse(matchGroup[4].Value, NumberStyles.Any) : 0;

			// value
			matchGroup = matches[1].Groups;
			_pseDocument.BlockSaleValue = matchGroup[2].Value == "VALUE" ? double.Parse(matchGroup[4].Value, NumberStyles.Any) : 0;

			// parse block sale
			pattern = @"((BLOCK SALES\s)([\w\d\s,.\(\):])+)(S E C T O R A L   S U M M A R Y)";
			Match m = Regex.Match(_reportString, pattern);
			_pseDocument.BlockSales = m.Groups[1].Value;

			// parse report notice
			pattern = @"(TOTAL FOREIGN:\s)(.+\s){2}([\w\d\s,.\(\):])+";
			m = Regex.Match(_reportString, pattern);
			_pseDocument.ExchangeNotice = m.Value;
		}

		private void ParseSectorSummary()
		{
			var pattern = @"\b(Financials|Industrials|Holding Firms|Property|Services|Mining & Oil|SME|ETF|PSEi|All Shares|GRAND TOTAL)\s\b([0-9,\.\s\(\)\-]+)";

			MatchCollection matches = Regex.Matches(_reportString, pattern);
			for (var sectorIndex = 0; sectorIndex < matches.Count; sectorIndex++)
			{
				GroupCollection matchCol = matches[sectorIndex].Groups;

				// index match
				var indexName = matchCol[1].Value;
				string[] row = matchCol[2].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Reverse<string>().ToArray<string>();

				if (indexName == "PSEi" || indexName == "All Shares")
				{
					SectorItem sectorItem = _pseDocument.Sectors[sectorIndex];
					sectorItem.PointChange = double.Parse(row[0], NumberStyles.Any);
					sectorItem.PercentChange = double.Parse(row[1], NumberStyles.Any);
					sectorItem.Close = double.Parse(row[2], NumberStyles.Any);
					sectorItem.Low = double.Parse(row[3], NumberStyles.Any);
					sectorItem.High = double.Parse(row[4], NumberStyles.Any);
					sectorItem.Open = double.Parse(row[5], NumberStyles.Any);
				}
				else if (indexName == "GRAND TOTAL")
				{					
					SectorItem sectorItem = _pseDocument.GetSector(PSEDocument.PSEI);
                    sectorItem.Value = double.Parse(row[0], NumberStyles.Any);                    
                    sectorItem.Volume = ulong.Parse(row[1], NumberStyles.Any);                    
				}
				else
				{
					SectorItem sectorItem = _pseDocument.Sectors[sectorIndex];
					if (sectorItem != null)
					{

						sectorItem.Value = double.Parse(row[0], NumberStyles.Any);
						sectorItem.Volume = ulong.Parse(row[1], NumberStyles.Any);

						// SME and ETF contains only two elements: Volume and Value
						if (sectorItem.Symbol == PSEDocument.SME || sectorItem.Symbol == PSEDocument.ETF)
							continue;
						
						sectorItem.Open = double.Parse(row[7], NumberStyles.Any);
						sectorItem.High = double.Parse(row[6], NumberStyles.Any);
						sectorItem.Low = double.Parse(row[5], NumberStyles.Any);
						sectorItem.Close = double.Parse(row[4], NumberStyles.Any);
						sectorItem.PercentChange = double.Parse(row[3], NumberStyles.Any);
						sectorItem.PointChange = double.Parse(row[2], NumberStyles.Any);

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

				// end of the line
				var pattern = @"(TOTAL MAIN BOARD.+\s+)(.+)";
				if (Regex.Match(line, pattern).Success)
					break;
				
				// create the sector object
			    pattern = @"\b([A-Z&\.,]\s+)+([A-Z])\b";
				Match sectorTextMatch = Regex.Match(line, pattern);
				if (sectorTextMatch.Success)
				{
					var sectorKey = sectorNameMap.GetKey(sectorTextMatch.Value);
					Debug.Assert(sectorKey != null, "Unable to locate sector key" + sectorTextMatch.Value);
					sector = _pseDocument.GetSector(sectorKey);

					// not all sectors have sub-sectors. 
					// we have to null this to make sure stocks don't get mixed up in other sectors
					subSector = null; 
					continue;
				}

				// sub sector
				// proceed to next line if matched
				pattern = @"(\*+\s+)(.+\s)(.\*+)";
				Match match = Regex.Match(line, pattern);
				if (match.Success)
				{
					Debug.Assert(sector != null, "Unable to initialize the sector object.");
					subSector = new SubSectorItem();
					subSector.Name = match.Groups[2].Value.Trim();
					sector.SubSectors.Add(subSector);
					continue;
				}

				// will only match stocks that has trading activity
				pattern = @"\b([\w]+\s)+\b([0-9\.\s,\(\)\-]+)";
				if (Regex.Match(line, pattern).Success)
				{
					// found the stock line
					Debug.Assert(sector != null, "Unable to initialize the sector object.");

					if (subSector == null)
					{
						// not all sectors have sub-sectors so we create a default
						subSector = new SubSectorItem();
						subSector.Name = "DEFAULT";
						sector.SubSectors.Add(subSector);
					}

					StockItem stock = ParseStockLine(line, sector);
					subSector.Stocks.Add(stock);
				}

			}

			//calculate sector netforeign buying/selling
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

			// build the stock description
			int index = 9;
			var stockDescription = string.Empty;
            for (int i = stockInfo.Length - 1; i > index; i--)
            {
				stockDescription = stockDescription + stockInfo[i] + " ";
            }

			stock.Description = stockDescription.Trim();

			return stock;
		}

		private void CleanupReportString()
		{
			// remove header text
			var textPattern = @"(The Philippine Stock Exchange, Inc)(\s+.+\s+)((?i)(April|May|June|July|August|September|October|November|December)\s+\d+.,.\d+)\s+";
			_reportString = Regex.Replace(_reportString, textPattern, string.Empty);

			// remove column names
			textPattern = @"(MAIN BOARD\s+)?(Net Foreign\s.+)\s+(Name Symbol)(.+\s+)";
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

		private void InitSectors()
		{
			sectorNameMap.Add(PSEDocument.FINANCIAL, "F I N A N C I A L S");
			sectorNameMap.Add(PSEDocument.INDUSTRIAL, "I N D U S T R I A L");
			sectorNameMap.Add(PSEDocument.HOLDING, "H O L D I N G   F I R M S");
			sectorNameMap.Add(PSEDocument.MINING_OIL, "M I N I N G   &   O I L");
			sectorNameMap.Add(PSEDocument.PROPERTY, "P R O P E R T Y");
			sectorNameMap.Add(PSEDocument.SERVICE, "S E R V I C E S");
			sectorNameMap.Add(PSEDocument.PREFERRED, "P R E F E R R E D");
			sectorNameMap.Add(PSEDocument.DEPOSITORY_RECEIPTS, "P H I L .   D E P O S I T A R Y   R E C E I P T S");
			sectorNameMap.Add(PSEDocument.WARRANT, "W A R R A N T S");
			sectorNameMap.Add(PSEDocument.SME, "S M A L L ,   M E D I U M   &   E M E R G I N G");
			sectorNameMap.Add(PSEDocument.ETF, "E X C H A N G E   T R A D E D   F U N D S");
			sectorNameMap.Add(PSEDocument.DOLLAR_DONOMINATED_SECURITIES, "D O L L A R   D E N O M I N A T E D   S E C U R I T I E S");

			// this is the order in which the sectors are printed in the sectory summary section
			// of the report
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.FINANCIAL });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.INDUSTRIAL });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.HOLDING });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.PROPERTY });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.SERVICE });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.MINING_OIL });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.SME });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.ETF });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.PSEI });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.ALLSHARES });
			//-

			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.PREFERRED });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.DEPOSITORY_RECEIPTS });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.WARRANT });
			_pseDocument.Sectors.Add(new SectorItem(_pseDocument) { Symbol = PSEDocument.DOLLAR_DONOMINATED_SECURITIES });   
		}

	}

}
