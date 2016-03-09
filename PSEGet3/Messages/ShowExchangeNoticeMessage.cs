using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class ShowExchangeNoticeMessage : MessageBase
    {
        public double WindowHeight { get; set; }
        public double WindowWidth { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public string NoticeText { get; set; }
    }
}