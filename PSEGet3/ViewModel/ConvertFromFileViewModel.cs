using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class ConvertFromFileViewModel : ViewModelBase
    {
        public ObservableCollection<string> _filesList = new ObservableCollection<string>();

        /// <summary>
        ///     Initializes a new instance of the ConvertFromFileViewModel class.
        /// </summary>
        public ConvertFromFileViewModel()
        {
            //this.FilesList = new ObservableCollection<string>();
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real": Connect to service, etc...

                // register to accept this message
                Messenger.Default.Register<GetConvertParamMessage>(this, OnRcvGetConvertParamMessage);
                Messenger.Default.Register<BrowseDialogResultMessage>(this, OnReceiveDialogResult);
                RegisterCommands();
            }
        }

        public RelayCommand BrowseFilesCommand { get; private set; }

        public ObservableCollection<string> FilesList
        {
            get { return _filesList; }
            set
            {
                if (_filesList == value)
                    return;
                _filesList = value;
                RaisePropertyChanged("FilesList");
            }
        }

        public RelayCommand<IList> RemoveSelectedItemsCommand { get; protected set; }

        private void RegisterCommands()
        {
            BrowseFilesCommand = new RelayCommand(() =>
            {
                // send message to show file selection dialog
                Messenger.Default.Send(new ShowBrowseDialogMessage());
            });

            RemoveSelectedItemsCommand = new RelayCommand<IList>(OnRemoveSelectedItems);
        }

        private void OnRemoveSelectedItems(IList selectedItems)
        {
            IEnumerable<string> collection = selectedItems.Cast<string>();
            var copy = new List<string>(collection);
            foreach (string s in copy)
            {
                FilesList.Remove(s);
            }
        }

        private void OnRcvGetConvertParamMessage(MessageBase msg)
        {
            if (msg is GetConvertParamMessage)
            {
                Messenger.Default.Send(
                    new ConvertFromFileDataParamMessage(FilesList));
            }
        }

        private void OnReceiveDialogResult(BrowseDialogResultMessage message)
        {
            if (message is BrowseDialogResultMessage)
            {
                //this.FilesList.Clear();
                foreach (string s in message.FileNames)
                {
                    FilesList.Add(s);
                }
            }
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}