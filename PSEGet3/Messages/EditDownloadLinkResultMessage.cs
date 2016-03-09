namespace PSEGet3.Messages
{
    public class EditDownloadLinkResultMessage : EditDownloadLinkMessage
    {
        public EditDownloadLinkResultMessage(object sender, string downloadLink)
            : base(sender, downloadLink)
        {
        }
    }
}