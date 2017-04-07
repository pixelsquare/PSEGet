using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using PSEGetLib.DocumentModel;
using PSEGetLib.Interfaces;
using System.Globalization;

namespace PSEGetLib
{
    public class EUnsupportedReportFormat : Exception
    {
        public override string Message
        {
            get
            {
                return "Unsupported report format. Quotation reports prior to Aug. 6, 2010 are not supported";
            }
        }
    }

    public class PSEReportReader : IPSEReportReader
    {
        private DateTime _tradeDate;
        private DateTime _reportChangeDate1 = new DateTime(2013, 11, 6); 
        private DateTime _reportChangeDate2 = new DateTime(2013, 12, 3);
        private DateTime _reportChangeDate3 = new DateTime(2017, 4, 4);
        private List<String> _pseReportString;
        private StringCollection reportBody = new StringCollection();
        private StringCollection sectorSummary = new StringCollection();
        private StringCollection reportNotice = new StringCollection();
        private StringCollection reportMisc = new StringCollection();

        private NameValueCollection sectorNameMap = new NameValueCollection();

        private DateTime getTradeDate()
        {
            return DateTime.Parse(this._pseReportString[2]);
        }

        /// <summary>
        /// Read pseReportString to this._pseReportString
        /// throws EUnsupportedReportFormat if quotation report is prior to 7/26/2010
        /// </summary>
        /// <param name="pseReportString"></param>
        private void ReadPSEReportFile(string pseReportString)
        {
            this._pseReportString.Clear();
            string[] lines = pseReportString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            this._pseReportString.AddRange(lines);
			
			List<string> tmp = new List<string>();
			tmp.AddRange(this._pseReportString.ToArray());
			IEnumerable<string> q = from s in tmp
									where(s.Trim().Length > 0)
									select s.Trim();
			this._pseReportString.Clear();
			foreach(string s in q)
			{                
				this._pseReportString.Add(s);
			}

            DateTime startDate = new DateTime(2010, 7, 26);
            if (getTradeDate() < startDate)
      	    	throw new EUnsupportedReportFormat();           
        }

        public PSEReportReader(string pseReportFile)
        {
            this._pseReportString = new List<string>();
            this.ReadPSEReportFile(pseReportFile);            
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

            if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
            {
                sectorNameMap.Add(PSEDocument.DEPOSITORY_RECEIPTS, "P H I L .   D E P O S I T A R Y   R E C E I P T S");
                sectorNameMap.Add(PSEDocument.WARRANT, "W A R R A N T S");
                sectorNameMap.Add(PSEDocument.SME, "S M A L L   &   M E D I U M   E N T E R P R I S E S");
                if (DateTime.Compare(_tradeDate, _reportChangeDate2) >= 0)
                    sectorNameMap.Add(PSEDocument.ETF, "E X C H A N G E   T R A D E D   F U N D S");
                if (DateTime.Compare(_tradeDate, _reportChangeDate2) >= 0)
                    sectorNameMap.Add(PSEDocument.DOLLAR_DONOMINATED_SECURITIES, "D O L L A R D E N O M I N A T E D S E C U R I T I E S");
            }
            else
            {                
                sectorNameMap.Add(PSEDocument.WARRANT, "WARRANTS, PHIL. DEPOSIT RECEIPT, ETC.");
                sectorNameMap.Add(PSEDocument.SME, "SMALL AND MEDIUM ENTERPRISES");
            }
        }

        /// <summary>
        /// Removes the quotation report header
        /// </summary>
        private void RemoveHeader()
        {                    
            string headerStr = "The Philippine Stock Exchange, Inc";           
            while (this._pseReportString.IndexOf(headerStr) > -1)
            {
                int index = this._pseReportString.IndexOf(headerStr);
                if (DateTime.Compare(_tradeDate, _reportChangeDate1) < 0)
                {
                    while (this._pseReportString[index] != "Buying (Selling)")                    
                    {
                        this._pseReportString.RemoveAt(index);
                    }
                    //index points to "Buying (Selling)"
                    this._pseReportString.RemoveAt(index);
                }
                else
                {
                    int startIndex = this._pseReportString.IndexOf(headerStr);
                    int endIndex = this._pseReportString.IndexOf("Buying/(Selling),");
                    int removeCount = (endIndex - startIndex) + 1;
                    this._pseReportString.RemoveRange(startIndex, removeCount);
                    if (this._pseReportString[index].Contains("Php Php"))
                        this._pseReportString.RemoveAt(index); // Php text
                }
            }
        }

        /// <summary>
        /// Cleanup() method removes unecessary information from this._pseReportString 
        /// </summary>
        private void Cleanup()
        {
            this.RemoveHeader();
            this.FillReportBody();
            this.FillReportSectorSummary();
            this.FillReportNotice();
            this.FillReportMisc();
        }

        /// <summary>
        /// fills this.reportMisc string collection with misclaneaous data of the report
        /// </summary>
        private void FillReportMisc()
        {
            int i = 0;
            while (i < this._pseReportString.Count)
            {
                string s = this._pseReportString[i].Trim();
                if (s.Contains("NO. OF ADVANCES:"))
                {
                    do
                    {
                        reportMisc.Add(this._pseReportString[i].Trim());
                        i++;
                    } while (!this._pseReportString[i].Contains("S E C T O R A L   S U M M A R Y"));
                }


                if (s.Contains("FOREIGN BUYING"))
                {
                    reportMisc.Add(s);

                    //foreign selling comes next
                    i++;
                    reportMisc.Add(this._pseReportString[i].Trim());

                }

                i++;
            }

        }

        /// <summary>
        /// fills this.reportNotice string collection with exchange notice stuff
        /// </summary>
        private void FillReportNotice()
        {
            bool foundNotice = false;
            string endText = "FOREIGN SELLING";
            if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
                endText = "TOTAL FOREIGN";
            for (int i = 0; i < this._pseReportString.Count; i++)            
            {
                if (this._pseReportString[i].Contains(endText))
                {
                    foundNotice = true;
                    continue;                
                }
                if (foundNotice)
                {
                    this.reportNotice.Add(this._pseReportString[i].TrimEnd() + "\n");
                }
            }

        }

        /// <summary>
        /// fills this.sectorSummary string collection with sector summary data
        /// </summary>
        private void FillReportSectorSummary()
        {
            int index = this._pseReportString.IndexOf("S E C T O R A L   S U M M A R Y") + 1;

            string stopLoopText = "FOREIGN SELLING";            
            if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
                stopLoopText = "TOTAL FOREIGN";

            do
            {
                index++;
                string s = this._pseReportString[index].Trim();
                string[] row = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				
				//Debug.WriteLine(s);
				if (s.Contains("FOREIGN SELLING"))
				{
					sectorSummary.Add(s);
					continue;
				}
				
                // skip SME line
                if (row[0] == "SME")
                    continue;


                //fix broken lines
                if (row.Length < 2)
                {
                    int x = this.sectorSummary.Count - 1;
                    this.sectorSummary[x] = this.sectorSummary[x] + " " + this._pseReportString[index].Trim();
                    continue;
                }
               
                sectorSummary.Add(s);
            } while (!this._pseReportString[index].Contains(stopLoopText));
        }

        /// <summary>
        /// fills this.reportBody string collection with stock data
        /// </summary>
        private void FillReportBody()
        {
            // CROWN bug
            if (DateTime.Compare(this._tradeDate, new DateTime(2015, 6, 22)) >= 0)
            {
                var crownIndex = this._pseReportString.IndexOf("N");
                if (crownIndex > -1 /*&& this._pseReportString[crownIndex + 1] == "N"*/)
                {
                    this.PSEReportString[crownIndex - 1].Replace("CROWN ASIA CROW", "CROWN ASIA CROWN");
                    this._pseReportString.RemoveAt(crownIndex);                    
                    //this._pseReportString[crownIndex] = this._pseReportString[crownIndex] + this._pseReportString[crownIndex + 1] + " " + this._pseReportString[crownIndex + 2];
                    //this._pseReportString.RemoveAt(crownIndex + 1);
                    //this._pseReportString.RemoveAt(crownIndex + 1);
                }
            }

            for (int i = 0; i < this._pseReportString.Count; i++)
            {
                string s = this._pseReportString[i].TrimEnd();
                if (s.Length == 0 || s.Contains("TOTAL VOLUME"))
                {
                    continue;
                }

                if (s.Contains("TOTAL REGULAR VOLUME"))
                {
                    break;
                }

                if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
                {
                    if (s.Contains("TOTAL MAIN BOARD VOLUME"))
                        break;
                }

                string[] row = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool condition = ((row[0] != "****") && (row.Length < 10) &&
                    (row[0].Length > 1) &&
                    (s != "SMALL AND MEDIUM ENTERPRISES") &&
                    (s != "WARRANTS, PHIL. DEPOSIT RECEIPT, ETC.") &&
                    (s != "TOTAL REGULAR VOLUME") && !s.Contains("TOTAL VOLUME"));


                if (condition)
                {
                    int nextIndex = i + 1;
                    while (this._pseReportString[nextIndex].TrimEnd().Length == 0)
                    {
                        nextIndex++;
                    }
                    s = s + " " + this._pseReportString[nextIndex].TrimEnd();
                    this._pseReportString.RemoveAt(nextIndex);
                }

                reportBody.Add(s);

            }
        }

        /// <summary>
        /// parse this._pseReportString and fill PSEDocument Collection
        /// </summary>
        /// <param name="pseDocument"></param>
        public void Fill(PSEDocument pseDocument)
        {
            _tradeDate = this.getTradeDate();
            pseDocument.TradeDate = _tradeDate;
            InitializeSectorMapping();

            this.Cleanup();

            ParseReportBody(pseDocument);
            ParseReportMisc(pseDocument);
            ParseSectorSummary(pseDocument);

            pseDocument.ExchangeNotice = reportNotice;

        }

        private void ParseSectorSummary(PSEDocument pseDocument)
        {
            string[] row;
            int i = 0;
            Nullable<double> nfb = null;
            Nullable<double> nfs = null;

            foreach (string s in this.sectorSummary)
            {
                row = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Reverse<string>().ToArray<string>();

                //OPEN HIGH LOW CLOSE %CHANGE PT.CHANGE VOLUME VALUE

                SectorItem sectorItem = pseDocument.Sectors[i];

                if (s.Contains("PSEi") || s.Contains("All Shares"))
                {
                    sectorItem.PointChange = double.Parse(row[0], NumberStyles.Any);
                    sectorItem.PercentChange = double.Parse(row[1], NumberStyles.Any);
                    sectorItem.Close = double.Parse(row[2], NumberStyles.Any);
                    sectorItem.Low = double.Parse(row[3], NumberStyles.Any);
                    sectorItem.High = double.Parse(row[4], NumberStyles.Any);
                    sectorItem.Open = double.Parse(row[5], NumberStyles.Any);
                }
                else if (s.Contains("GRAND TOTAL"))
                {
                    sectorItem = pseDocument.GetSector(PSEDocument.PSEI);
                    sectorItem.Value = double.Parse(row[0], NumberStyles.Any);
                    if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
                        sectorItem.Volume = ulong.Parse(row[1], NumberStyles.Any);
                    else
                        sectorItem.Volume = ulong.Parse(row[2], NumberStyles.Any);                                       
                }
                else if (s.Contains("SME") || s.Contains("ETF"))
                {
                    continue;
                }
                else if (s.Contains("FOREIGN"))
                {
                    if (s.Contains("FOREIGN BUYING"))
                    {
                        nfb = row[0].ParseDouble();
                        if (!nfb.HasValue)
                            nfb = 0;
                    }
                    if (s.Contains("FOREIGN SELLING"))
                    {
                        nfs = row[0].ParseDouble();
                        if (!nfs.HasValue)
                            nfs = 0;
                        // nothing more to parse after foreign selling
                        break;
                    }
                }
                else if (s.Contains("and Block Sale transactions.") ||
                    s.Contains("NET FOREIGN") ||
                    s.Contains("TOTAL FOREIGN"))
                {
                    continue;
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
                i++;
            }

            SectorItem psei = pseDocument.GetSector(PSEDocument.PSEI);
			psei.NetForeignBuy = Math.Round((double)nfb - (double)nfs, 2);

            //calculate sector netforeign buying/selling
            foreach (SectorItem sectorItem in pseDocument.Sectors)
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

        /// <summary>
        /// Parse miscelaneous information into pseDocument
        /// </summary>
        /// <param name="pseDocument"></param>
        private void ParseReportMisc(PSEDocument pseDocument)
        {
            Nullable<int> intValue = this.reportMisc[this.reportMisc.IndexOfString("NO. OF ADVANCES:")].ParseInt();
            pseDocument.NumAdvance = intValue.HasValue ? (int)intValue : 0;

            intValue = this.reportMisc[this.reportMisc.IndexOfString("NO. OF DECLINES:")].ParseInt();
            pseDocument.NumDeclines = intValue.HasValue ? (int)intValue : 0;

            intValue = this.reportMisc[this.reportMisc.IndexOfString("NO. OF UNCHANGED:")].ParseInt();
            pseDocument.NumUnchanged = intValue.HasValue ? (int)intValue : 0;

            pseDocument.NumTraded = (int)this.reportMisc[this.reportMisc.IndexOfString("NO. OF TRADED ISSUES:")].ParseInt();
            pseDocument.NumTrades = (int)this.reportMisc[this.reportMisc.IndexOfString("NO. OF TRADES:")].ParseInt();

            Nullable<ulong> ulongValue = this.reportMisc[this.reportMisc.IndexOfString("ODDLOT VOLUME:")].ParseUlong();
            pseDocument.OddLotVolume = ulongValue.HasValue ? (ulong)ulongValue : 0;

            Nullable<double> doubleValue = this.reportMisc[this.reportMisc.IndexOfString("ODDLOT VALUE:")].ParseDouble();
            pseDocument.OddLotValue = doubleValue.HasValue ? (double)doubleValue : 0;

            int crossVolumeIndex = this.reportMisc.IndexOfString("MAIN BOARD CROSS VOLUME:");            
            if (crossVolumeIndex > -1)
            {
                ulongValue = this.reportMisc[crossVolumeIndex].ParseUlong();
                pseDocument.MainCrossVolume = ulongValue.HasValue ? (ulong)ulongValue : 0;
            }

            int crossValueIndex = this.reportMisc.IndexOfString("MAIN BOARD CROSS VALUE:");
            if (crossValueIndex > -1)
            {
                doubleValue = this.reportMisc[crossValueIndex].ParseDouble();
                pseDocument.MainCrossValue = doubleValue.HasValue ? (double)doubleValue : 0;
            }

            doubleValue = this.reportMisc[this.reportMisc.IndexOfString("FOREIGN BUYING")].ParseDouble();
            double foreignBuying = doubleValue.HasValue ? (double)doubleValue : 0;

            doubleValue = this.reportMisc[this.reportMisc.IndexOfString("FOREIGN SELLING")].ParseDouble();
            double foreigntSelling = doubleValue.HasValue ? (double)doubleValue : 0;

            pseDocument.TotalForeignBuying = foreignBuying;
            pseDocument.TotalForeignSelling = foreigntSelling;

            // block sales
            int startIndex = this.reportMisc.IndexOfString("BLOCK SALES") + 2;
            int endIndex = this.reportMisc.IndexOfString("ODDLOT VOLUME:");
            if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
                endIndex = this.reportMisc.IndexOfString("FOREIGN BUYING");

            StringBuilder sb = new StringBuilder();

            for (int i = startIndex; i < endIndex; i++)
            {
                sb.Append(this.reportMisc[i] + "\n");
            }

            pseDocument.BlockSales = sb.ToString();

        }

        /// <summary>
        /// Parse stock information into pseDocument object
        /// </summary>
        /// <param name="pseDocument"></param>
        private void ParseReportBody(PSEDocument pseDocument)
        {

            SectorItem sector = null;            
            Nullable<ulong> uLongValue = null;
            Nullable<double> doubleValue = null;
		   	 			
            foreach (string s in this.reportBody)
            {
                //retrieve the sector object then move on to the next line
                if (sectorNameMap.ContainsValue(s))
                {
                    sector = pseDocument.GetSector(sectorNameMap.GetKey(s));
                    bool parseCondition = (s == sectorNameMap[PSEDocument.PREFERRED]) ||
                        (s == sectorNameMap[PSEDocument.SME]) ||
                        (s == sectorNameMap[PSEDocument.WARRANT]);

                    if (DateTime.Compare(_tradeDate, _reportChangeDate1) >= 0)
                        parseCondition = parseCondition || s == sectorNameMap[PSEDocument.DEPOSITORY_RECEIPTS];

                    if (DateTime.Compare(_tradeDate, _reportChangeDate2) >= 0)
                        parseCondition = parseCondition || s == sectorNameMap[PSEDocument.ETF];

                    if (DateTime.Compare(_tradeDate, _reportChangeDate3) >= 0)
                        parseCondition = parseCondition || s == sectorNameMap[PSEDocument.DOLLAR_DONOMINATED_SECURITIES];

                    if (parseCondition)
                    {
                        var subSector = new SubSectorItem();
                        subSector.Name = s;
                        sector.SubSectors.Add(subSector);
                    }

                    continue;
                }
				 
                string[] row = s.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			 
                if (row[0] == "****")
                {
                    // retrieve the subsector then create the subsector object
                    // continue to the next line after
                    var subSector = new SubSectorItem();

                    string subSectorName = s.Replace("****", null);
                    subSector.Name = subSectorName.Trim();
                    sector.SubSectors.Add(subSector);
                    continue;
                }

                // skip volume summary
                if (s.Contains("TOTAL VOLUME"))
                    continue;
                if (sector.SubSectors.Count == 0)
                    sector.SubSectors.Add(new SubSectorItem() {Name = "UNKNOWN"}); 
                // now load stock information into the subsector object

                row = row.Reverse<string>().ToArray<string>();
                

                SubSectorItem currentSubSector = sector.SubSectors[sector.SubSectors.Count - 1]; // always points to the last added subsector

                //Name Symbol Bid Ask Open High Low Close Volume Value NB/S
                //in reverse order

                StockItem stock = new StockItem(pseDocument, sector);

                doubleValue = row[0].ParseDouble();
                stock.NetForeignBuy = doubleValue.HasValue ? (double)doubleValue : 0;
   
                doubleValue = row[1].ParseDouble();
                stock.Value = doubleValue.HasValue ? (double)doubleValue : 0;

                uLongValue = row[2].ParseUlong();
                stock.Volume = uLongValue.HasValue ? (ulong)uLongValue : 0;

                doubleValue = row[3].ParseDouble();
                stock.Close = doubleValue.HasValue ? (double)doubleValue : 0;

                doubleValue = row[4].ParseDouble();
                stock.Low = doubleValue.HasValue ? (double)doubleValue : 0;

                doubleValue = row[5].ParseDouble();
                stock.High = doubleValue.HasValue ? (double)doubleValue : 0;

                doubleValue = row[6].ParseDouble();
                stock.Open = doubleValue.HasValue ? (double)doubleValue : 0;

                doubleValue = row[7].ParseDouble();
                stock.Ask = doubleValue.HasValue ? (double)doubleValue : 0;

                doubleValue = row[8].ParseDouble();
                stock.Bid = doubleValue.HasValue ? (double)doubleValue : 0;

                stock.Symbol = row[9];
 

                int index = 9;
                StringBuilder sb = new StringBuilder();
                for (int i = row.Length - 1; i > index; i--)
                {
                    sb.Append(row[i] + " ");
                }

                stock.Description = sb.ToString().TrimEnd();

                currentSubSector.Stocks.Add(stock);
            }
        }


        /// <summary>
        /// overloaded method of Fill()
        /// </summary>
        /// <param name="pseDocument"></param>
        /// <param name="pseReportFile"></param>
        public void Fill(PSEDocument pseDocument, string pseReportFile)
        {
            this.ReadPSEReportFile(pseReportFile);
            this.Fill(pseDocument);
        }

        public List<string> PSEReportString
        {
            get { return this._pseReportString; }
            set { this._pseReportString = value; }
        }
    }
}
