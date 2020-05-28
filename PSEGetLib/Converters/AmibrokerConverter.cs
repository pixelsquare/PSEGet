using System;
using System.IO;
using PSEGetLib.DocumentModel;
using LateBindingHelper;
using PSEGetLib.Types;

namespace PSEGetLib.Converters
{
    public class AmibrokerConverter : IDataConverter<AmiOutputSettings>
    {
        public AmibrokerConverter()
        {
        }

        private PSEDocument PSEDocument
        {
            get;
            set;
        }

        private AmiOutputSettings OutputSettings
        {
            get;
            set;
        }

        public void Execute(PSEDocument pseDocument, AmiOutputSettings outputSettings)
        {
            this.PSEDocument = pseDocument;
            this.OutputSettings = outputSettings;
            this.DoExecute();     
        }

        protected void DoExecute()
        {
            if (!Directory.Exists(this.OutputSettings.DatabaseDirectory))
                throw new DirectoryNotFoundException(this.OutputSettings.DatabaseDirectory + " does not exist");

            IOperationInvoker amiInvoker = BindingFactory.CreateAutomationBinding("Broker.Application");
            if (amiInvoker == null)
                throw new PSEGetException("Unable to find Amibroker.");
            
            // open the amibroker
            // PixelSquare: Launch amibroker silently

            //object x = amiInvoker.Property("Visible").Get<object>();
            //if (x.ToString() == "0")
            //{
            //    x = 1;
            //    amiInvoker.Property("Visible").Set(x);
            //}

            var databaseLoaded = amiInvoker.Method("LoadDatabase").AddParameter(this.OutputSettings.DatabaseDirectory).Invoke<bool>();
            if (!databaseLoaded)
                throw new PSEGetException(this.OutputSettings.DatabaseDirectory + " is not a valid Amibroker database.");

            foreach (SectorItem sector in this.PSEDocument.Sectors)
            {

                SetAmibrokerData(amiInvoker, this.PSEDocument, sector, this.OutputSettings);
                foreach (SubSectorItem subSector in sector.SubSectors)
                {
                    foreach (StockItem stockItem in subSector.Stocks)
                    {
                        if (stockItem.Open > 0)
                            SetAmibrokerData(amiInvoker, this.PSEDocument, stockItem, this.OutputSettings);
                    }
                }

            }
            amiInvoker.Method("SaveDatabase").Invoke();
            amiInvoker.Method("RefreshAll").Invoke();

        }

        private void SetAmibrokerData(IOperationInvoker amiInvoker, PSEDocument pseDocument,
            StockBase stockData, AmiOutputSettings amiOutputSettings)
        {
            //int version = Convert.ToInt32(amiInvoker.Property("Version").Get<string>().Replace(".", string.Empty));

            // stocks collection
            IOperationInvoker stocks = amiInvoker.Property("Stocks").Get<IOperationInvoker>();
                        

            // add stock item to collection
            IOperationInvoker stock = stocks.Method("Add").AddParameter(stockData.Symbol).Invoke<IOperationInvoker>();

            // quotations collection
            IOperationInvoker quotations = stock.Property("Quotations").Get<IOperationInvoker>();

            //object ok = quotations.Method("Remove").AddParameter(pseDocument.TradeDate).Invoke<IOperationInvoker>();

            // add a quoatation item to collection
            IOperationInvoker quotation = quotations.Method("Add").AddParameter(pseDocument.TradeDate).Invoke<IOperationInvoker>();
            //if (version >= 5400)
            //{
            //    // new 5.4 and up properties
            //    stock.Property("FullName").Set(stockData.Description);
            //}
            
            stock.Property("FullName").Set(stockData.FullName);
            stock.Property("Currency").Set(stockData.Currency);

            //stock.Property("MarketID").Set(0);
            //stock.Property("GroupID").Set(0);
            //stock.Property("IndustryID").Set(0);

            stock.Property("Favourite").Set(stockData.IsFavourite);
            stock.Property("Index").Set(stockData.IsIndex);

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
                    nfb = pseDocument.NetForeignBuying;
                }
                else
                    nfb = stockData.NetForeignBuy;                
                nfb = nfb / amiOutputSettings.SectorVolumeDivider;

                quotation.Property("Volume").Set(Math.Truncate(volume));
                quotation.Property("OpenInt").Set(Math.Truncate(nfb));
            }
            else
            {
                quotation.Property("Volume").Set(stockData.Volume);
                quotation.Property("OpenInt").Set(Math.Truncate(stockData.NetForeignBuy));
            }
        }
       
    }
}
