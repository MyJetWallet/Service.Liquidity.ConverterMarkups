using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class ConverterMarkupNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-settings-v2";
        public static string GeneratePartitionKey(string profileId) => $"{profileId}";
        public static string GenerateRowKey(string fromAsset, string toAsset) => $"{fromAsset}-{toAsset}";

        public ConverterMarkup ConverterMarkup;
        
        public static ConverterMarkupNoSqlEntity Create(ConverterMarkup entity)
        {
            return new ConverterMarkupNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.ProfileId),
                RowKey = GenerateRowKey(entity.FromAsset, entity.ToAsset),
                ConverterMarkup = entity
            };
        }
        
    }
}