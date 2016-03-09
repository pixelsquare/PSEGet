using PSEGetLib.DocumentModel;

namespace PSEGetLib.Interfaces
{
    public interface IPSEReportReader
    {
        void Fill(PSEDocument pseDocument);
        void Fill(PSEDocument pseDocument, string pseReportFile);
    }
}
