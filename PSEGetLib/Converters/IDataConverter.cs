using PSEGetLib.DocumentModel;

namespace PSEGetLib.Converters
{
    public interface IDataConverter<T>
        where T: OutputSettings
    {
        void Execute(PSEDocument pseDocument, T outputSettings);
    }
}
