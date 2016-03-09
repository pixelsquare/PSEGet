namespace PSEGetLib.Data.Common.Mapping
{
    public interface IObjectConverter
    {
        TTarget Convert<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class;
    }
}
