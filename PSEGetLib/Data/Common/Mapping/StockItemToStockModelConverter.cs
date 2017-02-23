using PSEGetLib.Data.Common.DataContracts;
using PSEGetLib.DocumentModel;

namespace PSEGetLib.Data.Common.Mapping
{
    public class StockItemToStockModelConverter : IObjectConverter
    {
        public TTarget Convert<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            var src = source as StockItem;
            //var tgt = target as Stock;

            var stock = new Stock();
            stock.Symbol = src.Symbol;
            stock.Open = src.Open;
            stock.High = src.High;
            stock.Low = src.Low;
            stock.Close = src.Close;
            stock.Volume = src.Volume;
            stock.Value = src.Value;
            return stock as TTarget;
        }
    }
}
