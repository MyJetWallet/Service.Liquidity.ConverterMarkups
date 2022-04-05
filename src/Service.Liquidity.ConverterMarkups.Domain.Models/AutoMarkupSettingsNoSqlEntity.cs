using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class AutoMarkupSettingsNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-auto-settings";
        private static string GeneratePartitionKey(string fromAsset) => fromAsset;
        private static string GenerateRowKey(string toAsset) => toAsset;

        public AutoMarkupSettings AutoMarkup;
        
        public static AutoMarkupSettingsNoSqlEntity Create(AutoMarkupSettings entity)
        {
            return new AutoMarkupSettingsNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.FromAsset),
                RowKey = GenerateRowKey(entity.ToAsset),
                AutoMarkup = entity
            };
        }
        
    }
}