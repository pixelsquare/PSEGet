using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class ShowAppMessage : MessageBase
    {
        public ShowAppMessage()
        {
            AppMessage = "";
            MessageTitle = "";
        }

        public string AppMessage { get; set; }
        public string MessageTitle { get; set; }
    }
}