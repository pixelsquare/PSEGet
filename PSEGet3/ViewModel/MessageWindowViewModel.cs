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
    public class MessageWindowViewModel : ViewModelBase
    {
        private string _appMessage;

        /// <summary>
        ///     Initializes a new instance of the ErrorWindowViewModel class.
        /// </summary>
        public MessageWindowViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                AppMessage = "This is the error message";
            }

            //Messenger.Default.Register<ShowAppMessage>(this, OnReceiveAppMessage);
        }

        public string AppMessage
        {
            get { return _appMessage; }
            set
            {
                if (_appMessage == value)
                {
                    return;
                }
                _appMessage = value;
                RaisePropertyChanged("AppMessage");
            }
        }

        private void OnReceiveAppMessage(MessageBase msg)
        {
            if (msg is ShowAppMessage)
            {
                AppMessage = ((ShowAppMessage) msg).AppMessage.Replace("_", "__");
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}