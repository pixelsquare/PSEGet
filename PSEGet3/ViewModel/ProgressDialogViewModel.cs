using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;

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
    public class ProgressDialogViewModel : ViewModelBase
    {
        //public RelayCommand CloseDialogCommand
        //{
        //    get;
        //    private set;
        //}

        private bool _isBusy;
        private string _logMessageText = "";

        private string _progressText = "";

        /// <summary>
        ///     Initializes a new instance of the ProgressDialogViewModel class.
        /// </summary>
        public ProgressDialogViewModel()
        {
            RegisterMessages();
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                ProgressText = "Downloading something something...";
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
                ProgressText = "";
                LogMessageText = "";
            }
            IsBusy = false;
            //CloseDialogCommand = new RelayCommand(() =>
            //    {
            //        Messenger.Default.Send<CloseWindowMessage>(new CloseWindowMessage());
            //    });
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            protected set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                if (_isBusy)
                    LogMessageText = "";
                RaisePropertyChanged("IsBusy");
            }
        }

        public string ProgressText
        {
            get { return _progressText; }
            set
            {
                _progressText = value;
                if (value == "Done!")
                {
                    IsBusy = false;
                }
                RaisePropertyChanged("ProgressText");
            }
        }

        public string LogMessageText
        {
            get { return _logMessageText; }
            set
            {
                _logMessageText = value;
                RaisePropertyChanged("LogMessageText");
            }
        }

        private void RegisterMessages()
        {
            Messenger.Default.Register<ShowAppMessage>(this, OnRcvAppMessage);
        }

        private void OnRcvAppMessage(MessageBase msg)
        {
            if (msg is ShowAppMessage)
            {
                IsBusy = true;
                string msgTitle = (msg as ShowAppMessage).MessageTitle.Replace("_", "__");
                string logMessage = (msg as ShowAppMessage).AppMessage;
                if (msgTitle != string.Empty)
                    ProgressText = msgTitle;

                if (logMessage != string.Empty)
                    LogMessageText = LogMessageText + "\n" + logMessage;
            }
        }
    }
}