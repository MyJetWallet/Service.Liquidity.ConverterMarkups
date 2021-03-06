using System.Linq;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Client
{
    public class MarkupExtractor : IMarkupExtractor
    {
        private readonly IMyNoSqlServerDataReader<ConverterMarkupOverviewNoSqlEntity> _markupReader;

        public MarkupExtractor(IMyNoSqlServerDataReader<ConverterMarkupOverviewNoSqlEntity> markupReader)
        {
            _markupReader = markupReader;
        }

        public decimal GetMarkup(string fromAsset, string toAsset, string profile)
        {
            var overview = _markupReader.Get(ConverterMarkupOverviewNoSqlEntity.GeneratePartitionKey(), ConverterMarkupOverviewNoSqlEntity.GenerateRowKey(profile))?.MarkupOverview.Overview;
            return overview?.FirstOrDefault(e => e.FromAsset == fromAsset && e.ToAsset == toAsset)?.Markup ?? 0m;
        }
        
        public decimal GetMinMarkup(string fromAsset, string toAsset, string profile)
        {
            var overview = _markupReader.Get(ConverterMarkupOverviewNoSqlEntity.GeneratePartitionKey(), ConverterMarkupOverviewNoSqlEntity.GenerateRowKey(profile))?.MarkupOverview.Overview;
            return overview?.FirstOrDefault(e => e.FromAsset == fromAsset && e.ToAsset == toAsset)?.MinMarkup ?? 0m;
        }
        
        public decimal GetFee(string fromAsset, string toAsset, string profile)
        {
            var overview = _markupReader.Get(ConverterMarkupOverviewNoSqlEntity.GeneratePartitionKey(), ConverterMarkupOverviewNoSqlEntity.GenerateRowKey(profile))?.MarkupOverview.Overview;
            return overview?.FirstOrDefault(e => e.FromAsset == fromAsset && e.ToAsset == toAsset)?.Fee ?? 0m;
        }
    }
}