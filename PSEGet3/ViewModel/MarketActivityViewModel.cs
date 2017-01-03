using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGetLib.Data.Common.DataContracts;
using PSEGetLib.Data.Service;
using Microsoft.Practices.ServiceLocation;

namespace PSEGet3.ViewModel
{
    /// <summary>
    ///     This class contains properties that a View can data bind to.
    ///     <para>
    ///         Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    ///     </para>
    ///     <para>
    ///         You can also use Blend to data bind with the tool's support.
    ///     </para>
    ///     <para>
    ///         See http://www.galasoft.ch/mvvm/getstarted
    ///     </para>
    /// </summary>
    public class MarketActivityViewModel : ViewModelBase
    {
        private int _advances;
        private ObservableCollection<BlockSaleModel> _blockSales;
        private double _compositeValue;
        private int _declines;
        private ulong _mainBoardCrossValue;
        private ulong _mainBoardCrossVolume;
        private int _numTrades;
        private double _totalForeignBuying;
        private double _totalForeignSelling;
        private int _tradedIssues;
        private int _unchanged;
        protected IPSEGetDataService marketSummaryService;

        /// <summary>
        ///     Initializes a new instance of the MarketActivityViewModel class.
        /// </summary>
        public MarketActivityViewModel()
        {
            BlockSales = new ObservableCollection<BlockSaleModel>();
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                Advances = 123;
                Declines = 456;
                CompositeValue = 1234566789;
                //BlockSales = new ObservableCollection<BlockSaleModel>();
                //BlockSales.Add(new BlockSaleModel() { Symbol = "XXX", Price = 123.45, Volume = 12345667, Value=12345678 });
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
                marketSummaryService = ServiceLocator.Current.GetInstance<IPSEGetDataService>(); //new PSEGetDataService();
                Messenger.Default.Register<TradeDateParamMessage>(this, OnRcvTradeDateParamMessage);
            }
        }

        public DateTime TradeDate { get; set; }

        public ObservableCollection<BlockSaleModel> BlockSales
        {
            get { return _blockSales; }
            protected set
            {
                _blockSales = value;
                RaisePropertyChanged("BlockSales");
            }
        }

        public int Advances
        {
            get { return _advances; }
            protected set
            {
                if (_advances == value)
                    return;
                _advances = value;
                RaisePropertyChanged("Advances");
            }
        }

        public int Declines
        {
            get { return _declines; }
            set
            {
                if (_declines == value)
                    return;
                _declines = value;
                RaisePropertyChanged("Declines");
            }
        }

        public int Unchanged
        {
            get { return _unchanged; }
            set
            {
                if (_unchanged == value)
                    return;
                _unchanged = value;
                RaisePropertyChanged("Unchanged");
            }
        }

        public int TradedIssues
        {
            get { return _tradedIssues; }
            set
            {
                if (_tradedIssues == value)
                    return;
                _tradedIssues = value;
                RaisePropertyChanged("TradedIssues");
            }
        }

        public int NumTrades
        {
            get { return _numTrades; }
            set
            {
                if (_numTrades == value)
                    return;
                _numTrades = value;
                RaisePropertyChanged("NumTrades");
            }
        }

        public double CompositeValue
        {
            get { return _compositeValue; }
            set
            {
                if (_compositeValue == value)
                    return;
                _compositeValue = value;
                RaisePropertyChanged("CompositeValue");
            }
        }

        public double TotalForeignBuying
        {
            get { return _totalForeignBuying; }
            set
            {
                if (_totalForeignBuying == value)
                    return;
                _totalForeignBuying = value;
                RaisePropertyChanged("TotalForeignBuying");
            }
        }

        public double TotalForeignSelling
        {
            get { return _totalForeignSelling; }
            set
            {
                if (_totalForeignSelling == value)
                    return;
                _totalForeignSelling = value;
                RaisePropertyChanged("TotalForeignSelling");
            }
        }

        public ulong MainBoardCrossVolume
        {
            get { return _mainBoardCrossVolume; }
            set
            {
                if (_mainBoardCrossVolume == value)
                    return;
                _mainBoardCrossVolume = value;
                RaisePropertyChanged("MainBoardCrossVolume");
            }
        }

        public ulong MainBoardCrossValue
        {
            get { return _mainBoardCrossValue; }
            set
            {
                if (_mainBoardCrossValue == value)
                    return;
                _mainBoardCrossValue = value;
                RaisePropertyChanged("MainBoardCrossValue");
            }
        }

        private void OnRcvTradeDateParamMessage(MessageBase msg)
        {
            if (msg is TradeDateParamMessage)
            {
                TradeDate = (msg as TradeDateParamMessage).TradeDate;
                LoadMarketActivityData();
            }
        }

        private void LoadMarketActivityData()
        {
            marketSummaryService.GetMarketActivity(GetMarketActivityCallback, TradeDate);
        }

        private void GetMarketActivityCallback(MarketActivityModel marketActivity)
        {
            Advances = marketActivity.Advances;
            Declines = marketActivity.Declines;
            Unchanged = marketActivity.Unchanged;
            TradedIssues = marketActivity.TradedIssues;
            NumTrades = marketActivity.NumTrades;
            CompositeValue = marketActivity.CompositeValue;
            TotalForeignBuying = marketActivity.TotalForeignBuying;
            TotalForeignSelling = marketActivity.TotalForeignSelling;
            MainBoardCrossVolume = marketActivity.MainBoardCrossVolume;
            MainBoardCrossValue = marketActivity.MainBoardCrossValue;
            BlockSales.Clear();
            foreach (BlockSaleModel b in marketActivity.BlockSales)
            {
                BlockSales.Add(b);
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}