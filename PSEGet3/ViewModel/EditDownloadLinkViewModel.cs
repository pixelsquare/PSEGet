using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
    public class EditDownloadLinkViewModel : ViewModelBase
    {
        public string _downloadLink;

        /// <summary>
        ///     Initializes a new instance of the EditDownloadLinkViewModel class.
        /// </summary>
        public EditDownloadLinkViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                AcceptCommand = new RelayCommand(
                    () =>
                        Messenger.Default.Send(
                            new EditDownloadLinkResultMessage(this, DownloadLink))
                    );
            }
        }

        public RelayCommand AcceptCommand { get; private set; }

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

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}