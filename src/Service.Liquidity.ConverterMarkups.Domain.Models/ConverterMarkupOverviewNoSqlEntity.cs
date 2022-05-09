using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class ConverterMarkupOverviewNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-overview-v2";
        private static string GeneratePartitionKey() => "overview";
        private static string GenerateRowKey(string profileId) => $"{profileId}";

        public MarkupOverview MarkupOverview;
        
        public static ConverterMarkupOverviewNoSqlEntity Create(MarkupOverview entity)
        {
            return new ConverterMarkupOverviewNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(entity.ProfileId),
                MarkupOverview = entity
            };
        }
        
    }
}