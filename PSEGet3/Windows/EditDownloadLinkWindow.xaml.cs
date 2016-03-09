using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;

namespace PSEGet3.Windows
{
    /// <summary>
    ///     Interaction logic for EditDownloadLinkWindow.xaml
    /// </summary>
    public partial class EditDownloadLinkWindow : Window
    {
        public EditDownloadLinkWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<CloseWindowMessage>(this, m => Close());
        }

        public void ReceiveMessage(MessageBase message)
        {
            if (message is EditDownloadLinkMessage)
            {
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}