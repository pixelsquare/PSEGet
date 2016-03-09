using GalaSoft.MvvmLight.Messaging;
using PSEGetLib.Converters;

namespace PSEGet3.Messages
{
    public class OutputSettingsMessage : MessageBase
    {
        public OutputSettings DataOutputSettings { get; set; }
    }
}