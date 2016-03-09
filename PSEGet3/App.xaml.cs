using System;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using PSEGet3.Messages;
using PSEGet3.ViewModel;
using PSEGet3.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace PSEGet3
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        public Window GetMainWindow()
        {
            return MainWindow;
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Messenger.Default.Send<ShowAppMessage, ProgressDialogViewModel>(
                new ShowAppMessage {AppMessage = "Done!"});

            string errorMsg = e.Exception.Message;
            if (errorMsg.Contains("(500) Internal Server Error."))
            {
                errorMsg += Environment.NewLine + Environment.NewLine +
                            "One of the reports you are trying to download does not exist. Try selecting a valid trading date.";
            }

            var errorWindow = new MessageWindow();
            Messenger.Default.Send(new ShowAppMessage {AppMessage = errorMsg, MessageTitle = "Error"});

            errorWindow.Owner = MainWindow;
            errorWindow.ShowDialog();

            ViewModelLocator.ConverterVMStatic.IsBusy = false;

            e.Handled = true;
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.AppUnandledExceptionEventHandler);
        }

        private void AppUnandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs ea)
        {
            var e = (Exception) ea.ExceptionObject;
            MessageBox.Show(e.Message);

            /*ErrorWindow errorWindow = new ErrorWindow();

            Messenger.Default.Send<ShowErrorMessage>(new ShowErrorMessage() { ErrorMessage = e.Message });

            errorWindow.Owner = this.MainWindow;
            errorWindow.ShowDialog();

            ViewModelLocator.ConverterVMStatic.IsBusy = false;*/
        }

        private void Application_ExitHandler(object sender, ExitEventArgs e)
        {
        }
    }
}