using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;

namespace PSEGet3.Windows
{
    /// <summary>
    ///     Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public ProgressDialog()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            Messenger.Default.Register<CloseWindowMessage>(this, onRcvCloseWindowMessage);
        }

        private void onRcvCloseWindowMessage(MessageBase msg)
        {
            if (msg is CloseWindowMessage)
            {
                Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
            textLog.Text = "";
        }
    }
}