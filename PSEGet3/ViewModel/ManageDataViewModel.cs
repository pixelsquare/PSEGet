using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
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
    public class ManageDataViewModel : ViewModelBase
    {
        private DateTime _purgeDate;

        /// <summary>
        ///     Initializes a new instance of the ManageDataViewModel class.
        /// </summary>
        public ManageDataViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                PurgeDate = DateTime.Today;
            }
            else
            {
                // Code runs "for real": Connect to service, etc...
                var span = new TimeSpan(60, 0, 0, 0);
                PurgeDate = DateTime.Today.Subtract(span);
            }
            Purge = new RelayCommand(OnPurgeCommand);
        }

        public DateTime PurgeDate
        {
            get { return _purgeDate; }
            set
            {
                _purgeDate = value;
                RaisePropertyChanged("PurgeDate");
            }
        }

        public RelayCommand Purge { get; set; }

        private void OnPurgeCommand()
        {
            var dataService = ServiceLocator.Current.GetInstance<IPSEGetDataService>(); //new PSEGetDataService();
            dataService.PurgeData(_purgeDate);
            Messenger.Default.Send<ShowAppMessage, MainWindow>(
                new ShowAppMessage {AppMessage = "Done Purging!"});
        }


        ////public override void Cleanup()
        ////{
        ////    // Clean own resources if needed

        ////    base.Cleanup();
        ////}
    }
}