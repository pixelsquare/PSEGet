using PSEGetLib.DocumentModel;

namespace PSEGetLib.Converters
{
    public interface IDataConverter<T>
        where T: OutputSettings
    {
        PSEDocument PSEDocument { get; set; }
        T OutputSettings{get; set;}
        void Execute();
    }
}
