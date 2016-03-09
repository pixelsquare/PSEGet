using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class EditDownloadLinkMessage : MessageBase
    {
        public EditDownloadLinkMessage(object sender, string downloadLink)
        {
            Recipient = sender;
            DownloadLink = downloadLink;
        }

        public object Recipient { get; set; }
        public string DownloadLink { get; set; }
    }
}