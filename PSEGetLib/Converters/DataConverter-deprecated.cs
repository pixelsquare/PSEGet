using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using PSEGetLib.DocumentModel;
using LateBindingHelper;

namespace PSEGetLib.Converters
{
    public class DataConverterdeprecated
    {
        public class ConvertEventArgs : EventArgs
        {
            public readonly OutputSettings OutputSettings;
            public ConvertEventArgs(OutputSettings outputSettings)
            {
                OutputSettings = outputSettings;
            }
        }

        public delegate void ConvertCompleteEventHandler(object sender, ConvertEventArgs e);
        public delegate void ConvertFailed(object sender, ConvertEventArgs e);


        public event ConvertCompleteEventHandler OnConvertComplete;
        //public event ConvertFailed OnConvertFailed;

        public DataConverterdeprecated()
        {
        }

        private string getCSVLine(string csvFormat, StockBase stockBase, CSVOutputSettings csvOutputSettings)
        {
            csvFormat = csvFormat.Replace(CSVOutputSettings.STOCK_SYMBOL, stockBase.Symbol);
            csvFormat = csvFormat.Replace(CSVOutputSettings.DESCRIPTION_SYMBOL, stockBase.Description);
            csvFormat = csvFormat.Replace(CSVOutputSettings.OPEN_SYMBOL, stockBase.Open.ToString());
            csvFormat = csvFormat.Replace(CSVOutputSettings.HIGH_SYMBOL, stockBase.High.ToString());
            csvFormat = csvFormat.Replace(CSVOutputSettings.LOW_SYMBOL, stockBase.Low.ToString());
            csvFormat = csvFormat.Replace(CSVOutputSettings.CLOSE_SYMBOL, stockBase.Close.ToString());
            csvFormat = csvFormat.Replace(CSVOutputSettings.NFBS_SYMBOL, stockBase.NetForeignBuy.ToString());

            if (stockBase is SectorItem)
            {
                string volume;
                if (csvOutputSettings.UseSectorValueAsVolume)
                    volume = stockBase.Value.ToString(CultureInfo.CreateSpecificCulture("en-US"));
                else
                    volume = stockBase.Volume.ToString(CultureInfo.CreateSpecificCulture("en-US"));
                csvFormat = csvFormat.Replace(CSVOutputSettings.VOLUME_SYMBOL, volume);
            }
            else
            {
                csvFormat = csvFormat.Replace(CSVOutputSettings.VOLUME_SYMBOL, stockBase.Volume.ToString(CultureInfo.CreateSpecificCulture("en-US")));
            }

            //Debug.WriteLine(csvFormat);

            return csvFormat;
        }

        public void ConvertToCSV(PSEDocument pseDocument, CSVOutputSettings csvOutputSettings)
        {
            //Debug.WriteLine(csvOutputSettings.ToString());
            List<string> csvOutput = new List<string>();

            string tradeDate = pseDocument.TradeDate.ToString(csvOutputSettings.DateFormat);
            string csvFormat = csvOutputSettings.CSVFormat.Replace(CSVOutputSettings.DATE_SYMBOL, tradeDate);
			csvFormat = csvFormat.Replace(",", csvOutputSettings.Delimiter);
			
            foreach (SectorItem sectorItem in pseDocument.Sectors)
            {
                // sectors
                //sectorItem.Volume = sectorItem.Volume / csvOutputSettings.SectorVolumeDivider;
                sectorItem.Value = Math.Truncate(sectorItem.Value / csvOutputSettings.SectorVolumeDivider);
                //sectorItem.NetForeignBuy = Math.Truncate(sectorItem.NetForeignBuy / csvOutputSettings.SectorVolumeDivider);
                
                csvOutput.Add(getCSVLine(csvFormat, sectorItem, csvOutputSettings));

                //sub sectors
                foreach (SubSectorItem subsectorItem in sectorItem.SubSectors)
                {
                    // stocks
                    foreach (StockItem stockItem in subsectorItem.Stocks)
                    {
                        if (stockItem.Open > 0)
                        { 
                            csvOutput.Add(getCSVLine(csvFormat, stockItem, csvOutputSettings));
                        }
                    }
                }

            }
            
            // write to file
			string filename = csvOutputSettings.OutputDirectory + Helpers.GetDirectorySeparator() + 
									csvOutputSettings.Filename;
			 
            using (StreamWriter writer = new StreamWriter(filename))
            {
                try
                {
                    foreach (string item in csvOutput)
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

            if (OnConvertComplete != null)
                OnConvertComplete(pseDocument, new ConvertEventArgs(csvOutputSettings));
        }

        private void SetAmibrokerData(IOperationInvoker amiInvoker, PSEDocument pseDocument, 
            StockBase stockData, AmiOutputSettings amiOutputSettings)
        {
            int version = Convert.ToInt32(amiInvoker.Property("Version").Get<string>().Replace(".", string.Empty));
            
            // stocks collection
            IOperationInvoker stocks = amiInvoker.Property("Stocks").Get <IOperationInvoker>();

            // add stock item to collection
            IOperationInvoker stock = stocks.Method("Add").AddParameter(stockData.Symbol).Invoke<IOperationInvoker>();

            // quotations collection
            IOperationInvoker quotations = stock.Property("Quotations").Get<IOperationInvoker>();

            // add a quoatation item to collection
            IOperationInvoker quotation = quotations.Method("Add").AddParameter(pseDocument.TradeDate).Invoke<IOperationInvoker>();
            if (version >= 5400)
            {
                // new 5.4 and up properties
                stock.Property("FullName").Set(stockData.Description);
            }
            quotation.Property("Open").Set(stockData.Open);
            quotation.Property("High").Set(stockData.High);
            quotation.Property("Low").Set(stockData.Low);
            quotation.Property("Close").Set(stockData.Close);

            if (stockData.Symbol.StartsWith("^"))
            {
                double volume;
                double nfb;
                if (amiOutputSettings.UseSectorValueAsVolume)
                {
                    volume = Math.Truncate(stockData.Value) / amiOutputSettings.SectorVolumeDivider;
                }
                else
                {
                    volume = stockData.Volume / amiOutputSettings.SectorVolumeDivider;
                }

                if (stockData.Symbol == "^PSEi")
                {
                    nfb = pseDocument.NetForeignBuying / amiOutputSettings.SectorVolumeDivider;
                }
                else
                    nfb = stockData.NetForeignBuy / amiOutputSettings.SectorVolumeDivider;

                quotation.Property("Volume").Set(Math.Truncate(volume));
                quotation.Property("OpenInt").Set(Math.Truncate(nfb));
            }
            else
            {
                quotation.Property("Volume").Set(stockData.Volume);
                quotation.Property("OpenInt").Set(stockData.NetForeignBuy);
            }
        }
        public void ConvertToAmibroker(PSEDocument pseDocument, AmiOutputSettings amiOutputSettings)
        {
            if (!System.IO.Directory.Exists(amiOutputSettings.DatabaseDirectory))
                throw new Exception(amiOutputSettings.DatabaseDirectory + " does not exist");

            IOperationInvoker amiInvoker = BindingFactory.CreateAutomationBinding("Broker.Application");
            amiInvoker.Method("LoadDatabase").AddParameter(amiOutputSettings.DatabaseDirectory).Invoke();
            foreach (SectorItem sector in pseDocument.Sectors)
            {
                SetAmibrokerData(amiInvoker, pseDocument, sector, amiOutputSettings);                            
                foreach (SubSectorItem subSector in sector.SubSectors)
                {
                    foreach (StockItem stockItem in subSector.Stocks)
                    {
                        SetAmibrokerData(amiInvoker, pseDocument, stockItem, amiOutputSettings);
                    }
                }
            }
            amiInvoker.Method("SaveDatabase").Invoke();
            amiInvoker.Method("RefreshAll").Invoke();
            object x = amiInvoker.Property("Visible").Get<object>();
            if (x.ToString() == "0")
            {
                x = 1;
                amiInvoker.Property("Visible").Set(x);
            }

            if (OnConvertComplete != null)
                OnConvertComplete(pseDocument, new ConvertEventArgs(amiOutputSettings));
        }

        public void ConvertToMetaStock(PSEDocument pseDocument, MetaOutputSettings metaOutputSettings)
        {
            if (OnConvertComplete != null)
                OnConvertComplete(pseDocument, new ConvertEventArgs(metaOutputSettings));
        }
    }
}
