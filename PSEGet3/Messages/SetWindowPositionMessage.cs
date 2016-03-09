using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class SetWindowPositionMessage : MessageBase
    {
        public double Top { get; set; }
        public double Left { get; set; }
    }
}