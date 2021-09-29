using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class ConverterMarkupNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-converter-markup-settings";
        private static string GeneratePartitionKey(string fromAsset) => fromAsset;
        private static string GenerateRowKey(string toAsset) => toAsset;

        public ConverterMarkup ConverterMarkup;
        
        public static ConverterMarkupNoSqlEntity Create(ConverterMarkup entity)
        {
            return new ConverterMarkupNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(entity.FromAsset),
                RowKey = GenerateRowKey(entity.ToAsset),
                ConverterMarkup = entity
            };
        }
        
    }
}