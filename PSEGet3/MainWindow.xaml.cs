using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGet3.ViewModel;
using PSEGet3.Windows;
using PSEGetLib.Configuration;

namespace PSEGet3
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            Messenger.Default.Register<ShowAppMessage>(this, OnRcvAppMessage);
            Messenger.Default.Register<ShowMarketSummaryMessage>(this, OnRcvOnShowMarketSummaryMsg);
            Messenger.Default.Register<ShowExchangeNoticeMessage>(this, OnShowNoticeMsg);
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Title = "PSEGet v" + fileVersionInfo.FileVersion;
        }

        private void OnRcvOnShowMarketSummaryMsg(MessageBase msg)
        {
            if (msg is ShowMarketSummaryMessage)
            {
                MainTab.SelectedItem = tabMarketSummary;
                Messenger.Default.Send(
                    new LoadMarketSummaryMessage {TradeDate = (msg as ShowMarketSummaryMessage).TradeDate});
            }
        }

        private void OnShowNoticeMsg(MessageBase msg)
        {
            if (msg is ShowExchangeNoticeMessage)
            {
                var theMessage = msg as ShowExchangeNoticeMessage;
                var noticeWindow = new ExchangeNoticeWindow();
                noticeWindow.Height = Height;
                noticeWindow.Width = Width;
                noticeWindow.Left = Left + Width + 6;
                noticeWindow.Top = Top;
                noticeWindow.noticeText.Text = theMessage.NoticeText;
                noticeWindow.Owner = this;
                noticeWindow.Show();
            }
        }

        private void OnRcvAppMessage(MessageBase msg)
        {
            if (msg is ShowAppMessage)
            {
                var msgWindow = new MessageWindow();
                msgWindow.Owner = Application.Current.MainWindow;
                msgWindow.MessageText.Text = (msg as ShowAppMessage).AppMessage.Replace("_", "__");
                msgWindow.Title = (msg as ShowAppMessage).MessageTitle;
                msgWindow.ShowDialog();
            }
        }

        private void MainWindow_ClosingHandler(object sender, CancelEventArgs e)
        {
            Messenger.Default.Send(new CloseWindowMessage());
            SaveAppConfiguration();
        }

        private void SaveAppConfiguration()
        {
            var conf = new AppConfiguration();
            OutputSettingsViewModel outputSettings = ViewModelLocator.OutputSettingsVMStatic;
            DownloadAndConvertViewModel downloadAndConvertVM = ViewModelLocator.DownloadAndConvertVMStatic;
            ConverterViewModel convertVM = ViewModelLocator.ConverterVMStatic;

            conf.CSVOutputFormat = outputSettings.Format;
            conf.CSVDateFormat = outputSettings.DateFormat;
            conf.CSVDelimiter = outputSettings.Delimiter;
            conf.FormTop = Application.Current.MainWindow.Top;
            conf.FormLeft = Application.Current.MainWindow.Left;
            conf.IndexValueAsVolume = outputSettings.IndexValueAsVolume;
            conf.OutputFolder = outputSettings.OutputLocation;
            conf.TargetOutput = outputSettings.OutputTo;

            conf.ReportUrl = downloadAndConvertVM.DownloadLink;
            conf.DataSource = convertVM.DataConvertMethod;
            conf.IndexValueDivisor = outputSettings.IndexValueDivisor;
            conf.ReportFilenameFormat = outputSettings.ReportFilenameFormat;

            string xmlFilename = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\PSEGet.conf.xml";
            TextWriter writer = new StreamWriter(xmlFilename);

            var serializer = new XmlSerializer(typeof (AppConfiguration));
            serializer.Serialize(writer, conf);
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            Messenger.Default.Send(
                new SetWindowPositionMessage {Left = Left + Width + 6, Top = Top}
                );
        }
    }
}