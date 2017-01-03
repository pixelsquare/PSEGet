using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using System.Windows;

namespace PSEGet3.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
                TerminateCommand = new RelayCommand(() => Application.Current.Shutdown());
                ConvertCommand = new RelayCommand(
                    () => Messenger.Default.Send(new ExecuteConvertCommandMessage()));

                // message 0 terminates the application
                Messenger.Default.Register<int>(this,
                    p =>
                    {
                        if (p == 0)
                        {
                            //Messenger.Default.Send<SaveOutputSettingsMessage, OutputSettingsViewModel>
                            //    (new SaveOutputSettingsMessage());
                            //this.SaveAppConfiguration();
                            TerminateCommand.Execute(null);
                        }
                    });
            }
        }

        public RelayCommand TerminateCommand { get; private set; }

        public RelayCommand ConvertCommand { get; private set; }
    }
}