using GalaSoft.MvvmLight.Messaging;

namespace PSEGet3.Messages
{
    public class ShowFoldersDialogResultMessage : MessageBase
    {
        public ShowFoldersDialogResultMessage(string selectedPath)
        {
            SelectedPath = selectedPath;
        }

        public string SelectedPath { get; set; }
    }
}