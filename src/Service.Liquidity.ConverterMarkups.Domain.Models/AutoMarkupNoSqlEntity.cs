using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class AutoMarkupNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-auto-v2";
        public static string GeneratePartitionKey(string profileId) => $"{profileId}";
        public static string GenerateRowKey(string fromAsset, string toAsset) => $"{fromAsset}-{toAsset}";

        public AutoMarkup AutoMarkup;
        
        public static AutoMarkupNoSqlEntity Create(AutoMarkup entity)
        {
            return new AutoMarkupNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.ProfileId),
                RowKey = GenerateRowKey(entity.FromAsset, entity.ToAsset),
                AutoMarkup = entity
            };
        }
        
    }
}