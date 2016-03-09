using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGet3.Windows;
using PSEGetLib.Configuration;

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
    public class DownloadAndConvertViewModel : ViewModelBase
    {
        private string _downloadLink;

        private DateTime _fromDate;

        private DateTime _toDate;

        /// <summary>
        ///     Initializes a new instance of the DownloadAndConvertViewModel class.
        /// </summary>
        public DownloadAndConvertViewModel()
        {
            //this.FromDate = DateTime.Today;
            //this.ToDate = DateTime.Today;

            if (IsInDesignMode)
            {
                //Code runs in Blend --> create design time data.
            }
            else
            {
                //Code runs "for real": Connect to service, etc...                
                Messenger.Default.Register<EditDownloadLinkResultMessage>(this, ReceiveMessage);
                Messenger.Default.Register<GetConvertParamMessage>(this, ReceiveGetParamMessage);

                DownloadLink = GetDownloadLink(); //ConfigurationManager.AppSettings["DownloadLink"]; 
                ShowWindowCommand = new RelayCommand(() =>
                {
                    // send message
                    var editDownloadLinkMessage = new EditDownloadLinkMessage(this, DownloadLink);
                    Messenger.Default.Send(editDownloadLinkMessage);
                });
            }
        }

        public string DownloadLink
        {
            get { return _downloadLink; }
            set
            {
                if (_downloadLink == value)
                    return;
                _downloadLink = value;
                RaisePropertyChanged("DownloadLink");
            }
        }

        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                if (_fromDate == value)
                    return;
                _fromDate = value;
                RaisePropertyChanged("FromDate");
            }
        }

        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                if (_toDate == value)
                    return;
                _toDate = value;
                RaisePropertyChanged("ToDate");
            }
        }


        public RelayCommand ShowWindowCommand { get; private set; }

        private string GetDownloadLink()
        {
            var xml = new XmlSerializer(typeof (AppConfiguration));

            string xmlFilename = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\PSEGet.conf.xml";
            TextReader reader = new StreamReader(xmlFilename);
            var conf = (AppConfiguration) xml.Deserialize(reader);
            return conf.ReportUrl;
        }

        // receive a message
        private void ReceiveGetParamMessage(MessageBase message)
        {
            if (message is GetConvertParamMessage)
            {
                var param = new DownloadAndConvertParamMessage
                {
                    DownloadURI = new Uri(DownloadLink),
                    FromDate = FromDate,
                    ToDate = ToDate
                };

                // reply message
                Messenger.Default.Send(param);
            }
        }

        private void ReceiveMessage(MessageBase message)
        {
            if (message is EditDownloadLinkResultMessage)
            {
                string downloadLink = ((EditDownloadLinkResultMessage) message).DownloadLink;
                if (downloadLink.Trim() == "")
                    throw new Exception("Download URL can not be empty");
                DownloadLink = downloadLink;
                Messenger.Default.Send<CloseWindowMessage, EditDownloadLinkWindow>(new CloseWindowMessage());
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}