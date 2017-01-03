using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGetLib.Data.Common.DataContracts;
using PSEGetLib.Data.DesignService;
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
    public class MarketSummaryViewModel : ViewModelBase
    {
        protected IPSEGetDataService MarketSummaryService;
        private ObservableCollection<Stock> _allStocks;
        private ObservableCollection<Stock> _foreignBuying;
        private ObservableCollection<Stock> _foreignSelling;
        private ObservableCollection<Stock> _gainers;
        private ObservableCollection<Stock> _indexSummary;
        private ObservableCollection<Stock> _losers;
        private string _selectedIndex;
        //private readonly ServiceProviderBase _sp;

        private DateTime _tradeDate;

        /// <summary>
        ///     Initializes a new instance of the MarketSummaryViewModel class.
        /// </summary>
        public MarketSummaryViewModel()
        {
            TradeDate = DateTime.Today;
            IsBusy = false;
            IndexSummary = new ObservableCollection<Stock>();
            Gainers = new ObservableCollection<Stock>();
            Losers = new ObservableCollection<Stock>();
            AllStocks = new ObservableCollection<Stock>();
            ForeignBuying = new ObservableCollection<Stock>();
            ForeignSelling = new ObservableCollection<Stock>();
            //MarketActivity = new MarketActivityModel();

            Messenger.Default.Register<LoadMarketSummaryMessage>(this, OnRcvLoadMarketSummaryMsg);

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                LoadDesignData();
            }
            else
            {
                // Code runs "for real": Connect to service, etc...            
                MarketSummaryService = ServiceLocator.Current.GetInstance<IPSEGetDataService>(); //new PSEGetDataService();                
                RegisterCommands();
            }
        }

        public DateTime TradeDate
        {
            get { return _tradeDate; }
            set
            {
                if (_tradeDate == value)
                {
                    return;
                }
                _tradeDate = value;
                RaisePropertyChanged("TradeDate");
            }
        }

        public string SelectedIndex
        {
            get { return _selectedIndex; }
            protected set
            {
                if (_selectedIndex == value)
                {
                    return;
                }
                _selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        public ObservableCollection<Stock> IndexSummary
        {
            get { return _indexSummary; }
            protected set
            {
                _indexSummary = value;
                RaisePropertyChanged("IndexSummary");
            }
        }


        public ObservableCollection<Stock> Gainers
        {
            get { return _gainers; }
            protected set
            {
                _gainers = value;
                RaisePropertyChanged("Gainers");
            }
        }

        public ObservableCollection<Stock> Losers
        {
            get { return _losers; }
            protected set
            {
                _losers = value;
                RaisePropertyChanged("Losers");
            }
        }

        public ObservableCollection<Stock> AllStocks
        {
            get { return _allStocks; }
            protected set
            {
                _allStocks = value;
                RaisePropertyChanged("AllStocks");
            }
        }

        public ObservableCollection<Stock> ForeignBuying
        {
            get { return _foreignBuying; }
            protected set
            {
                _foreignBuying = value;
                RaisePropertyChanged("ForeignBuying");
            }
        }

        public ObservableCollection<Stock> ForeignSelling
        {
            get { return _foreignSelling; }
            protected set
            {
                _foreignSelling = value;
                RaisePropertyChanged("ForeignSelling");
            }
        }

        //private MarketActivityModel _marketActivity;
        //public MarketActivityModel MarketActivity
        //{
        //    get
        //    {
        //        return this._marketActivity;
        //    }
        //    protected set
        //    {
        //        this._marketActivity = value;                
        //        RaisePropertyChanged("MarketActivity");
        //    }
        //}

        public bool IsBusy { get; set; }

        public RelayCommand GetSummaryCommand { get; private set; }
        public RelayCommand<string> ShowIndexDetailsCommand { get; private set; }

        private void OnRcvLoadMarketSummaryMsg(MessageBase msg)
        {
            var message = msg as LoadMarketSummaryMessage;
            if (message != null)
            {
                TradeDate = message.TradeDate;
                if (GetSummaryCommand.CanExecute(null))
                    GetSummaryCommand.Execute(null);
            }
        }

        private void RegisterCommands()
        {
            GetSummaryCommand = new RelayCommand(OnGetSummary, () => !IsBusy);
            ShowIndexDetailsCommand = new RelayCommand<string>(OnGetIndexDetails);
        }

        private void LoadDesignData()
        {
            GetIndexSummaryCallback(new DesignIndecesData());
            GetGainersCallback(new DesignStocks());
            GetLosersCallback(new DesignStocks());
            GetForeignBuyCallback(new DesignStocks());
            GetForeignSellCallback(new DesignStocks());
            //MarketActivity.Advances = 111;
        }

        private void GetAllStocksCallback(IEnumerable<Stock> allStocks)
        {
            AllStocks.Clear();
            foreach (Stock stock in allStocks)
            {
                AllStocks.Add(stock);
            }
        }

        private void GetIndexSummaryCallback(IEnumerable<Stock> Indeces)
        {
            IndexSummary.Clear();
            if (Indeces.Count() == 0)
                return;

            Stock psei = (from q in Indeces
                where q.Symbol == "^PSEi"
                select q).FirstOrDefault();

            IndexSummary.Add(psei);

            Stock allShares = (from q in Indeces
                where q.Symbol == "^ALLSHARES"
                select q).FirstOrDefault();

            IndexSummary.Add(allShares);

            IEnumerable<Stock> query = from q in Indeces
                where q.Symbol != "^PSEi" && q.Symbol != "^ALLSHARES"
                select q;

            foreach (Stock index in query)
            {
                IndexSummary.Add(index);
            }
        }

        private void GetGainersCallback(IEnumerable<Stock> gainers)
        {
            Gainers.Clear();
            foreach (Stock stock in gainers)
            {
                Gainers.Add(stock);
            }
        }

        private void GetLosersCallback(IEnumerable<Stock> losers)
        {
            Losers.Clear();
            foreach (Stock stock in losers)
            {
                Losers.Add(stock);
            }
        }

        private void GetForeignBuyCallback(IEnumerable<Stock> foreignBuy)
        {
            ForeignBuying.Clear();
            foreach (Stock stock in foreignBuy)
            {
                ForeignBuying.Add(stock);
            }
        }

        private void GetForeignSellCallback(IEnumerable<Stock> foreignSell)
        {
            ForeignSelling.Clear();
            foreach (Stock stock in foreignSell)
            {
                ForeignSelling.Add(stock);
            }
        }

        protected void OnGetSummary()
        {
            IsBusy = true;
            try
            {
                //Messenger.Default.Send<CloseWindowMessage>(new CloseWindowMessage());
                if (!MarketSummaryService.MarketSummaryExist(TradeDate))
                {
                    Messenger.Default.Send(
                        new ShowAppMessage
                        {
                            AppMessage = "You have not converted the report for the date you specified."
                        });
                    return;
                }
                MarketSummaryService.GetIndexSummary(GetIndexSummaryCallback, TradeDate);
                MarketSummaryService.GetGainers(GetGainersCallback, TradeDate);
                MarketSummaryService.GetLosers(GetLosersCallback, TradeDate);
                MarketSummaryService.GetForeignBuy(GetForeignBuyCallback, TradeDate);
                MarketSummaryService.GetForeignSell(GetForeignSellCallback, TradeDate);
                MarketSummaryService.GetAllStocks(GetAllStocksCallback, TradeDate);
                //marketSummaryService.GetMarketActivity(this.GetMarketActivityCallback, this.TradeDate);  

                Messenger.Default.Send(new TradeDateParamMessage(TradeDate));
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnGetIndexDetails(string symbol)
        {
            SelectedIndex = symbol;
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}