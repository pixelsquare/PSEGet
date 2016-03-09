using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;
using PSEGet3.Windows;

namespace PSEGet3.View
{
    /// <summary>
    ///     Interaction logic for DownloadAndConvertView.xaml
    /// </summary>
    public partial class DownloadAndConvertView : UserControl
    {
        public DownloadAndConvertView()
        {
            InitializeComponent();
            Messenger.Default.Register<EditDownloadLinkMessage>(this, ReceiveMessage);
        }

        public void ReceiveMessage(MessageBase message)
        {
            // receive message to display the edit form
            if (message is EditDownloadLinkMessage)
            {
                var edw = new EditDownloadLinkWindow();
                var msg = (EditDownloadLinkMessage) message;
                edw.textDownloadLink.Text = msg.DownloadLink;
                edw.Owner = Window.GetWindow(this);
                edw.ShowDialog();
            }
        }
    }
}