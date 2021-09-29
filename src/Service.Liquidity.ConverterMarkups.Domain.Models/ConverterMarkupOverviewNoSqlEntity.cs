using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class ConverterMarkupOverviewNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-overview";
        private static string GeneratePartitionKey() => "overview";
        private static string GenerateRowKey() => string.Empty;

        public MarkupOverview MarkupOverview;
        
        public static ConverterMarkupOverviewNoSqlEntity Create(MarkupOverview entity)
        {
            return new ConverterMarkupOverviewNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                MarkupOverview = entity
            };
        }
        
    }
}