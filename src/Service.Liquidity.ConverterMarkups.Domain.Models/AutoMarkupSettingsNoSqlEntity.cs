using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class AutoMarkupSettingsNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-auto-settings-v2";
        public static string GeneratePartitionKey(string profileId) => $"{profileId}";
        public static string GenerateRowKey(string fromAsset, string toAsset) => $"{fromAsset}-{toAsset}";

        public AutoMarkupSettings AutoMarkupSettings;
        
        public static AutoMarkupSettingsNoSqlEntity Create(AutoMarkupSettings entity)
        {
            return new AutoMarkupSettingsNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.ProfileId),
                RowKey = GenerateRowKey(entity.FromAsset, entity.ToAsset),
                AutoMarkupSettings = entity
            };
        }
        
    }
}