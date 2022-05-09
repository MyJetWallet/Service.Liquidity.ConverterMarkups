using System.Collections.Generic;
using MyNoSqlServer.Abstractions;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    public class MarkupProfilesNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-converter-profiles";

        public static string GeneratePartitionKey() => "ConverterProfiles";

        public static string GenerateRowKey() => "ConverterProfiles";

        public static MarkupProfilesNoSqlEntity Create(List<string> profiles)
        {
            return new MarkupProfilesNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(),
                Profiles = profiles
            };
        }

        public List<string> Profiles { get; set; }
    }
}