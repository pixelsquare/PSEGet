using GalaSoft.MvvmLight.Messaging;
using PSEGetLib.Configuration;

namespace PSEGet3.Messages
{
    public class SetConvertMethodMessage : GenericMessage<ConvertMethod>
    {
        public SetConvertMethodMessage(ConvertMethod convertMethod) : base(convertMethod)
        {

        }
    }
}
