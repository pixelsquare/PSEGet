using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class ConvertFromFileDataParamMessage : MessageBase
    {
        public ConvertFromFileDataParamMessage(IEnumerable<string> fileList)
        {
            FileList = fileList;
        }

        public IEnumerable<string> FileList { get; set; }
    }
}