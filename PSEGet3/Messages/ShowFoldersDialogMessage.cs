using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class ShowFoldersDialogMessage : MessageBase
    {
        public string SelectedPath { get; set; }
    }
}