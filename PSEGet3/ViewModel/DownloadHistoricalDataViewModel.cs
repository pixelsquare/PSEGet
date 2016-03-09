using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGetLib.Data.DesignService;
using PSEGetLib.Data.Service;

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
    public class DownloadHistoricalDataViewModel : ViewModelBase
    {
        private readonly List<string> _selectedStocks = new List<string>();
        public bool _isSingleFile;
        public int _numYears;
        private ObservableCollection<string> _stockList = new ObservableCollection<string>();
        private IPSEGetDataService dataService;

        /// <summary>
        ///     Initializes a new instance of the DownloadHistoricalDataViewModel class.
        /// </summary>
        public DownloadHistoricalDataViewModel()
        {
            if (IsInDesignMode)
            {
                dataService = new MarketSummaryDesign();
            }
            else
            {
                dataService = new PSEGetDataService();
            }

            dataService.GetStockList(getStockListCallback);
            NumYears = 20;
            IsSingleFile = false;
            RegisterCommands();

            // subscribe to messages
            Messenger.Default.Register<GetConvertParamMessage>(this, OnReceiveGetParamMessage);
        }

        public ObservableCollection<string> StockList
        {
            get { return _stockList; }
            set
            {
                if (_stockList == value)
                {
                    return;
                }
                _stockList = value;
                RaisePropertyChanged("StockList");
            }
        }

        public int NumYears
        {
            get { return _numYears; }
            set
            {
                if (_numYears == value)
                    return;
                _numYears = value;
                RaisePropertyChanged("NumYears");
            }
        }

        public bool IsSingleFile
        {
            get { return _isSingleFile; }
            set
            {
                if (_isSingleFile == value)
                    return;
                _isSingleFile = value;
                RaisePropertyChanged("IsSingleFile");
            }
        }

        public RelayCommand<string> AddSelectedStockCommand { get; private set; }

        public RelayCommand<string> RemoveSelectedStockCommand { get; private set; }

        private void RegisterCommands()
        {
            AddSelectedStockCommand = new RelayCommand<string>(OnAddSelectedStock);
            RemoveSelectedStockCommand = new RelayCommand<string>(OnRemoveSelectedStock);
        }

        private void getStockListCallback(IEnumerable<string> stockList)
        {
            StockList.Clear();
            foreach (string s in stockList)
            {
                StockList.Add(s);
            }
        }

        private void OnReceiveGetParamMessage(MessageBase msg)
        {
            if (msg is GetConvertParamMessage)
            {
                var param = new DownloadHistoricalDataParamMessage
                {
                    StockList = _selectedStocks,
                    NumYears = NumYears,
                    IsSingleFile = IsSingleFile
                };
                Messenger.Default.Send(param);
            }
        }

        private void OnAddSelectedStock(string symbol)
        {
            _selectedStocks.Add(symbol);
        }

        private void OnRemoveSelectedStock(string symbol)
        {
            _selectedStocks.Remove(symbol);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}