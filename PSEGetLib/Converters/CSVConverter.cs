using System;
using System.Collections.Generic;
using System.Globalization;
using PSEGetLib.DocumentModel;
using System.IO;

namespace PSEGetLib.Converters
{
    public class CSVConverter : IDataConverter<CSVOutputSettings>
    {

        public CSVConverter()
        {
        }

        private PSEDocument PSEDocument
        {
            get; set;
        }
        
        private CSVOutputSettings OutputSettings
        {
            get; set;
        }

        private string getTradeDate()
        {
            DateTime result = new DateTime(this.PSEDocument.TradeDate.Year,
                                            this.PSEDocument.TradeDate.Month,
                                            this.PSEDocument.TradeDate.Day);

            return result.ToString(this.OutputSettings.DateFormat);
        }

        protected void DoExecute()
        {
            //Debug.WriteLine(csvOutputSettings.ToString());
            if (!Directory.Exists( this.OutputSettings.OutputDirectory))
                throw new DirectoryNotFoundException(this.OutputSettings.OutputDirectory + " does not exist");

            List<string> csvOutput = new List<string>();

            string tradeDate = getTradeDate(); //this._pseDocument.TradeDate.ToString(this._outputSettings.DateFormat);
            string csvFormat = this.OutputSettings.CSVFormat.Replace(CSVOutputSettings.DATE_SYMBOL, tradeDate);
			csvFormat = csvFormat.Replace(",", this.OutputSettings.Delimiter);

            foreach (SectorItem sectorItem in this.PSEDocument.Sectors)
            {
                // sectors
                //sectorItem.Volume = sectorItem.Volume / csvOutputSettings.SectorVolumeDivider;
                if (sectorItem.Open > 0)
                    csvOutput.Add(getCSVLine(csvFormat, sectorItem, this.OutputSettings));

                //sub sectors
                foreach (SubSectorItem subsectorItem in sectorItem.SubSectors)
                {
                    // stocks
                    foreach (StockItem stockItem in subsectorItem.Stocks)
                    {
                        if (stockItem.Open > 0)
                        {
                            csvOutput.Add(getCSVLine(csvFormat, stockItem, this.OutputSettings));
                        }
                    }
                }
            }

            WriteToFile(csvOutput);
        }

        protected void WriteToFile(IEnumerable<string> csvLines)
        {
            // write to file
            string filename = this.OutputSettings.OutputDirectory + Helpers.GetDirectorySeparator() +
                                    this.OutputSettings.Filename;

            using (StreamWriter writer = new StreamWriter(filename))
            {
                try
                {
                    foreach (string item in csvLines)
                    {
                        writer.WriteLine(item);
                    }
                }
                finally
                {
                    writer.Flush();
                    writer.Close();
                }
            };
            
        }

        public void Execute(PSEDocument pseDocument, CSVOutputSettings csvOutputSettings)
        {   
            this.PSEDocument = pseDocument;
            this.OutputSettings = csvOutputSettings;            
            DoExecute();
        }

        private string getCSVLine(string csvFormat, StockBase stockBase, CSVOutputSettings csvOutputSettings)
        {
            //double stockValue = stockBase.Value;
            
            // remove decimals
            double netForeignBuy = Math.Truncate(stockBase.NetForeignBuy);
            if (stockBase is SectorItem)
            {                
                // scale down nfb and volume for sectors
               // stockValue = Math.Truncate(stockBase.Value / this._outputSettings.SectorVolumeDivider); 
                netForeignBuy = Math.Truncate(stockBase.NetForeignBuy / this.OutputSettings.SectorVolumeDivider);
                
                // index value as volume
                double volume;
                if (csvOutputSettings.UseSectorValueAsVolume)
                    volume = stockBase.Value;
                else
                    volume = stockBase.Volume;
                
                // index divisor
                volume = Math.Truncate(volume / csvOutputSettings.SectorVolumeDivider);
                csvFormat = csvFormat.Replace(CSVOutputSettings.VOLUME_SYMBOL, volume.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            }
            else
            {
                csvFormat = csvFormat.Replace(CSVOutputSettings.VOLUME_SYMBOL, stockBase.Volume.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            }
            
            csvFormat = csvFormat.Replace(CSVOutputSettings.STOCK_SYMBOL, stockBase.Symbol);
            csvFormat = csvFormat.Replace(CSVOutputSettings.DESCRIPTION_SYMBOL, stockBase.Description);
            csvFormat = csvFormat.Replace(CSVOutputSettings.OPEN_SYMBOL, stockBase.Open.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            csvFormat = csvFormat.Replace(CSVOutputSettings.HIGH_SYMBOL, stockBase.High.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            csvFormat = csvFormat.Replace(CSVOutputSettings.LOW_SYMBOL, stockBase.Low.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            csvFormat = csvFormat.Replace(CSVOutputSettings.CLOSE_SYMBOL, stockBase.Close.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            csvFormat = csvFormat.Replace(CSVOutputSettings.NFBS_SYMBOL, netForeignBuy.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            csvFormat = csvFormat.Replace(CSVOutputSettings.VALUE_SYMBOL, stockBase.Value.ToString(CultureInfo.CreateSpecificCulture("en-US")));

            //Debug.WriteLine(csvFormat);

            return csvFormat;
        }
    }
}
