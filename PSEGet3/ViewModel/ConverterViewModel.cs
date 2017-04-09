using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGetLib;
using PSEGetLib.Configuration;
using PSEGetLib.Converters;
using PSEGetLib.Data.Service;
using PSEGetLib.DocumentModel;
using MessageBox = System.Windows.Forms.MessageBox;
using GalaSoft.MvvmLight.Ioc;
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
    public class ConverterViewModel : ViewModelBase
    {
        private ConvertMethod _dataConvertMethod;
        private bool _isBusy;
        private bool _isConvertFromFilesMethod;
        private bool _isDownloadAndConvertMethod;
        private bool _isDownloadHistoricalDataMethod;

        /// <summary>
        ///     Initializes a new instance of the ConverterViewModel1 class.
        /// </summary>
        public ConverterViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real": Connect to service, etc...

                // register to receive specific messages
                Messenger.Default.Register<DownloadAndConvertParamMessage>(this, OnReceiveConvertParamMessage);
                //Messenger.Default.Register<DownloadHistoricalDataParamMessage>(this,
                //    OnReceiveDownloadHistoricalDataParamMsg);
                Messenger.Default.Register<ConvertFromFileDataParamMessage>(this, OnRcvConvertFromFileDataParamMessage);
                Messenger.Default.Register<OutputSettingsMessage>(this, OnRcvOutputSettingsMessage);
                Messenger.Default.Register<ExecuteConvertCommandMessage>(this,
                    msg =>
                    {
                        if (msg is ExecuteConvertCommandMessage)
                            OnConvert();
                    });

                // workaround to EventToCommand issue
                Messenger.Default.Register<SetConvertMethodMessage>(this,
                    msg =>
                    {
                        if (msg is SetConvertMethodMessage)
                        {
                            DataConvertMethod = (msg as SetConvertMethodMessage).Content;                            
                        }
                            
                    });
                InitCommands();

                RegisterCommands();
            }
        }

        private OutputSettings OutputSettings { get; set; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value)
                {
                    return;
                }
                _isBusy = value;
                RaisePropertyChanged("IsBusy");
                ConvertCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand<ConvertMethod> SetConvertMethodCommand { get; private set; }

        public ConvertMethod DataConvertMethod
        {
            get { return _dataConvertMethod; }
            set
            {
                _dataConvertMethod = value;
                IsDownloadAndConvertMethod = value == ConvertMethod.DownloadAndConvert;
                IsConvertFromFilesMethod = value == ConvertMethod.ConvertFromFiles;
                IsDownloadHistoricalDataMethod = value == ConvertMethod.DownloadHistoricalData;
            }
        }

        public bool IsDownloadAndConvertMethod
        {
            get { return _isDownloadAndConvertMethod; }
            set
            {
                _isDownloadAndConvertMethod = value;
                RaisePropertyChanged("IsDownloadAndConvertMethod");
            }
        }

        public bool IsConvertFromFilesMethod
        {
            get { return _isConvertFromFilesMethod; }
            set
            {
                _isConvertFromFilesMethod = value;
                RaisePropertyChanged("IsConvertFromFilesMethod");
            }
        }

        public bool IsDownloadHistoricalDataMethod
        {
            get { return _isDownloadHistoricalDataMethod; }
            set
            {
                _isDownloadHistoricalDataMethod = value;
                RaisePropertyChanged("IsDownloadHistoricalDataMethod");
            }
        }

        public RelayCommand TerminateCommand { get; private set; }

        public RelayCommand ConvertCommand { get; private set; }

        private void InitCommands()
        {
            TerminateCommand = new RelayCommand(
                () => Messenger.Default.Send(0),
                () => !IsBusy);

            ConvertCommand = new RelayCommand(OnConvert, () => !IsBusy);
        }

        private void RegisterCommands()
        {
            // command to set the convert method
            SetConvertMethodCommand = new RelayCommand<ConvertMethod>(c => DataConvertMethod = c);
        }

        private void OnRcvOutputSettingsMessage(MessageBase msg)
        {
            OutputSettings = ((OutputSettingsMessage) msg).DataOutputSettings;
        }

        private void OnRcvConvertFromFileDataParamMessage(MessageBase msg)
        {
            if (msg is ConvertFromFileDataParamMessage)
            {
                                
                var dataParam = msg as ConvertFromFileDataParamMessage;
                if (dataParam.FileList.Count() == 0)
                {
                    Messenger.Default.Send(new ShowAppMessage { MessageTitle = "Information", AppMessage = "Nothing to convert." });
                    return;
                }

                var t = new Thread(ConvertFromFileHelper.ConvertFromFiles);
                t.IsBackground = false;

                var param = new ConvertFromFilesParam();               
                param.FileList = dataParam.FileList;
                param.OutputSettings = OutputSettings;
                param.threadObject = t;

                IsBusy = true;
                ShowCloseProgressDialogDelegate showProgressDelegate = () =>
                {
                    Messenger.Default.Send(new CloseWindowMessage());
                    Messenger.Default.Send(new ShowProgressDialogMessage());
                };

                ShowCloseProgressDialogDelegate closeProgressDelegate = () =>
                {
                    IsBusy = false;
                    Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                        new ShowAppMessage {MessageTitle = "Done!", AppMessage = "Done."});
                };

                param.OnStartProcess = () => { Application.Current.Dispatcher.Invoke(showProgressDelegate, null); };

                param.BeforeConvertCallback = reportFilename =>
                {
                    SendAppMessageDelegate appMessage = () =>
                    {
                        Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                            new ShowAppMessage {MessageTitle = "Converting " + Path.GetFileName(reportFilename) + "..."}
                            );
                    };
                    Application.Current.Dispatcher.Invoke(appMessage, null);
                };

                param.ProgressCallback = (reportFilename, pseDocument) =>
                {                                   
                    Application.Current.Dispatcher.Invoke(() =>
                     {
                        var dataService = ServiceLocator.Current.GetInstance<IPSEGetDataService>(); //new PSEGetDataService();
                        dataService.SaveTradeData(pseDocument);

                        Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                            new ShowAppMessage
                            {
                                MessageTitle = "Done.",
                                AppMessage = "Converted " + Path.GetFileName(reportFilename)
                            }
                            );
                      });
                };

                param.ExceptionCallback = e =>
                {
                    //errorList.Add(e);
                    //System.Windows.Forms.MessageBox.Show(e.Message);                   
                    if (e is SQLiteException)
                    {
                        MessageBox.Show("The conversion was successful but PSEGet was unable to update the database.");
                    }
                    else
                    {
                        MessageBox.Show(e.Message);
                    }
                    IsBusy = false;
                    Application.Current.Dispatcher.Invoke(closeProgressDelegate, null);
                };
                param.CompletedCallback = pseDocument =>
                {                    
                    Application.Current.Dispatcher.Invoke(closeProgressDelegate, null);
                    ShowExchangeNotice(pseDocument);
                };


                t.Start(param);
            }
        }

        private void ShowExchangeNotice(PSEDocument pseDocument)
        {
            if (pseDocument != null)
            {
                ShowCloseProgressDialogDelegate showMarketSummaryTab = () =>
                {
                    Messenger.Default.Send(
                        new ShowMarketSummaryMessage {TradeDate = pseDocument.TradeDate});                    

                    Messenger.Default.Send(
                        new ShowExchangeNoticeMessage {NoticeText = pseDocument.ExchangeNotice }
                        );
                };
                Application.Current.Dispatcher.Invoke(showMarketSummaryTab, null);
            }
        }

        // process message from DownloadAndConvertViewModel
        private void OnReceiveConvertParamMessage(MessageBase message)
        {
            if (message is DownloadAndConvertParamMessage)
            {
                IsBusy = true;

                var msg = message as DownloadAndConvertParamMessage;
                string savePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Reports";

                var t = new Thread(DownloadAndConvertHelper.DownloadAndConvert);
                t.IsBackground = true;
                var param = new DownloadAndConvertParams();

                param.threadObject = t;
                param.DownloadUri = msg.DownloadURI;
                param.FromDate = msg.FromDate;
                param.ToDate = msg.ToDate;
                param.SavePath = savePath;
                param.OutputSettings = OutputSettings;

                param.ReportDownloadedHandler =
                    (s, e) =>
                    {
                        Action appMessage;
                        string fileName = Path.GetFileName(((ReportDownloader) s).CurrentDownloadFile);
                        if (e.Error != null)
                        {
                            if (e.Error is WebException)
                            {
                                appMessage = () =>
                                {
                                    string msgText = "Failed to download " + fileName;
                                    Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                        new ShowAppMessage
                                        {
                                            MessageTitle = msgText,
                                            AppMessage = msgText + "\n" + e.Error.Message
                                        });
                                };
                            }
                            else
                            {
                                appMessage = () =>
                                {
                                    Messenger.Default.Send(new ShowProgressDialogMessage());
                                    Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                        new ShowAppMessage
                                        {
                                            MessageTitle = "Error",
                                            AppMessage = e.Error.Message
                                        });
                                };
                            }
                        }
                        else
                        {
                            appMessage = null;
                            // download success                            
                            //appMessage = () =>
                            //{
                            //    Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                            //        new ShowAppMessage() { MessageTitle = "Done.", AppMessage = "Downloaded " + fileName }
                            //        );
                            //};
                        }
                        if (appMessage != null)
                            Application.Current.Dispatcher.Invoke(appMessage, null);
                    };

                param.DownloadAllCompletedHandler =
                    (s, e) =>
                    {
                        SendAppMessageDelegate appMessage = () =>
                        {
                            IsBusy = false;

                            var reportDownloader = (ReportDownloader) s;
                            int successCount = (from f in reportDownloader.DownloadedFiles
                                where (f.Success && f.Converted)
                                select f).Count();
                            int failedCount = (from f in reportDownloader.DownloadedFiles
                                where f.Success == false
                                select f).Count();

                            string msgText =
                                String.Format("Successfully downloaded and converted: {0}.\nFailed count: {1}.",
                                    successCount, failedCount);
                            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                new ShowAppMessage {AppMessage = msgText, MessageTitle = "Done."});

                            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                new ShowAppMessage {AppMessage = "Done!", MessageTitle = "Done!"});
                        };

                        Application.Current.Dispatcher.Invoke(appMessage, null);
                    };

                param.ConvertCompleteCallback =
                    (s, pseDocument) =>
                    {
                        var dataService = ServiceLocator.Current.GetInstance<IPSEGetDataService>(); //new PSEGetDataService();
                        dataService.SaveTradeData(pseDocument);

                        Action appMessageAction = () =>
                        {
                            string fileName = Path.GetFileName(((ReportDownloader) s).CurrentDownloadFile);
                            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                new ShowAppMessage {MessageTitle = "Done.", AppMessage = "Converted " + fileName}
                                );
                        };
                        Application.Current.Dispatcher.Invoke(appMessageAction);
                    };

                param.OnStartDownloadProcess =
                    (s, e) =>
                    {
                        SendAppMessageDelegate appMessage = () =>
                        {
                            Messenger.Default.Send(
                                new ShowProgressDialogMessage());
                            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                new ShowAppMessage {MessageTitle = "Trying..."});
                        };
                        Application.Current.Dispatcher.Invoke(appMessage, null);
                    };

                param.DownloadProgressHandler =
                    (s, e) =>
                    {
                        SendAppMessageDelegate sendMsg = () =>
                        {
                            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                new ShowAppMessage
                                {
                                    MessageTitle = "Downloading " + Path.GetFileName((string) s) + "..."
                                }
                                );
                        };
                        Application.Current.Dispatcher.Invoke(sendMsg, null);
                    };

                param.BeforeConvertCallback =
                    s =>
                    {
                        SendAppMessageDelegate sendMsg = () =>
                        {
                            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                new ShowAppMessage {MessageTitle = "Converting " + Path.GetFileName(s) + "..."}
                                );
                        };
                        Application.Current.Dispatcher.Invoke(sendMsg, null);
                    };

                param.ConvertErrorHandler =
                    (downloader, e) =>
                    {
                        Action appMessageAction = () =>
                        {
                            string msgText = "Failed to convert " +
                                             Path.GetFileName(((ReportDownloader) downloader).CurrentDownloadFile);
                            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                                new ShowAppMessage
                                {
                                    AppMessage = msgText + "\n" + e.Message,
                                    MessageTitle = msgText
                                });
                        };
                        Application.Current.Dispatcher.Invoke(appMessageAction);
                    };

                param.ProcessCompletedCallback =
                    pseDocument =>
                    {
                        if (pseDocument != null)
                            ShowExchangeNotice(pseDocument);
                    };

                Messenger.Default.Send(new CloseWindowMessage());

                t.Start(param);

                //this.IsBusy = false;
            }
        }

        //// download historical data
        //private void OnReceiveDownloadHistoricalDataParamMsg(MessageBase message)
        //{
        //    if (message is DownloadHistoricalDataParamMessage)
        //    {
        //        IsBusy = true;
        //        CSVOutputSettings csvOutputSettings;
        //        if (!(OutputSettings is CSVOutputSettings))
        //            throw new Exception("Invalid output settings. Conversion to CSV is your only option.");

        //        csvOutputSettings = OutputSettings as CSVOutputSettings;

        //        var downloadParam = message as DownloadHistoricalDataParamMessage;

        //        var d = new HistoricalDownloadParams();

        //        d.NumYears = downloadParam.NumYears;
        //        d.OutputSettings = csvOutputSettings;
        //        d.stockList = downloadParam.StockList;

        //        ShowCloseProgressDialogDelegate showProgressDelegate =
        //            () => { Messenger.Default.Send(new ShowProgressDialogMessage()); };

        //        ShowCloseProgressDialogDelegate closeProgressDelegate = () =>
        //        {
        //            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
        //                new ShowAppMessage {MessageTitle = Path.GetFileName("Done!")});
        //        };

        //        // configure callbacks
        //        d.StartDownloadProcess = () => { Application.Current.Dispatcher.Invoke(showProgressDelegate, null); };
        //        d.BeforeStockDataDownloadCallback = s =>
        //        {
        //            SendAppMessageDelegate msg = () =>
        //            {
        //                Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
        //                    new ShowAppMessage {MessageTitle = "Downloading " + s + "..."});
        //            };
        //            Application.Current.Dispatcher.Invoke(msg, null);
        //        };
        //        d.AfterStockDataDownloadCallback = s =>
        //        {
        //            SendAppMessageDelegate msg = () =>
        //            {
        //                Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
        //                    new ShowAppMessage {MessageTitle = "Done."}
        //                    );
        //            };
        //            Application.Current.Dispatcher.Invoke(msg, null);
        //        };

        //        d.OnExceptionCallback = e =>
        //        {
        //            IsBusy = false;
        //            ShowCloseProgressDialogDelegate msg = () =>
        //            {
        //                closeProgressDelegate();
        //                Messenger.Default.Send<ShowAppMessage, MainWindow>(
        //                    new ShowAppMessage {AppMessage = e.Message}
        //                    );
        //            };
        //            Application.Current.Dispatcher.Invoke(msg, null);
        //        };

        //        d.DownloadAllCompleteCallback = () =>
        //        {
        //            IsBusy = false;
        //            Application.Current.Dispatcher.Invoke(closeProgressDelegate, null);
        //        };

        //        if (!Directory.Exists(d.OutputSettings.OutputDirectory))
        //        {
        //            throw new Exception(d.OutputSettings.OutputDirectory + " does not exist!");
        //        }

        //        var t = new Thread(DownloadHistoricalDataHelper.DownloadAndConvertHistoricalData);
        //        t.Start(d);
        //    }
        //}

        private void OnConvert()
        {
            // get the output settings
            Messenger.Default.Send<GetConvertParamMessage, OutputSettingsViewModel>(new GetConvertParamMessage());
            switch (DataConvertMethod)
            {
                case ConvertMethod.ConvertFromFiles:
                    Messenger.Default.Send<GetConvertParamMessage, ConvertFromFileViewModel>(
                        new GetConvertParamMessage()
                        );
                    break;

                case ConvertMethod.DownloadAndConvert:
                    // send a message to DownloadAndConvertViewModel
                    // to retrieve the download parameters: fromData, toDate etc.
                    Messenger.Default.Send<GetConvertParamMessage, DownloadAndConvertViewModel>(
                        new GetConvertParamMessage());
                    break;

                case ConvertMethod.DownloadHistoricalData:
                    // send a message to DownloadHistoricalDataViewModel                                                
                    //Messenger.Default.Send<GetConvertParamMessage, DownloadHistoricalDataViewModel>(
                    //    new GetConvertParamMessage()
                    //    );

                    break;
            }
        }

        private delegate void SendAppMessageDelegate();

        private delegate void ShowCloseProgressDialogDelegate();

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}