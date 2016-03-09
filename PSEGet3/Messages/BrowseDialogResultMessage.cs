using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class BrowseDialogResultMessage : MessageBase
    {
        public BrowseDialogResultMessage(List<string> filenames)
        {
            FileNames = filenames;
        }

        public List<string> FileNames { get; set; }
    }
}