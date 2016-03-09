using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using PSEGet3.Messages;

namespace PSEGet3.Windows
{
    /// <summary>
    ///     Interaction logic for ExchangeNoticeWindow.xaml
    /// </summary>
    public partial class ExchangeNoticeWindow : Window
    {
        public ExchangeNoticeWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<SetWindowPositionMessage>(this, OnRcvShowWindowMsg);
            Messenger.Default.Register<CloseWindowMessage>(this, OnRcvCloseMsg);
        }

        private void OnRcvCloseMsg(MessageBase msg)
        {
            if (msg is CloseWindowMessage)
            {
                Close();
            }
        }

        private void OnRcvShowWindowMsg(MessageBase msg)
        {
            if (msg is SetWindowPositionMessage)
            {
                var theMessage = msg as SetWindowPositionMessage;
                Top = theMessage.Top;
                Left = theMessage.Left;
            }
        }
    }
}