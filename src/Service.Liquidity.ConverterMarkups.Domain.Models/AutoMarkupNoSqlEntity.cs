using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class AutoMarkupNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-auto";
        private static string GeneratePartitionKey(string fromAsset) => fromAsset;
        private static string GenerateRowKey(string toAsset) => toAsset;

        public AutoMarkup AutoMarkup;
        
        public static AutoMarkupNoSqlEntity Create(AutoMarkup entity)
        {
            return new AutoMarkupNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.FromAsset),
                RowKey = GenerateRowKey(entity.ToAsset),
                AutoMarkup = entity
            };
        }
        
    }
}