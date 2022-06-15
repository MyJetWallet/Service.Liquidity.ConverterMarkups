using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    [DataContract]
    public class AutoMarkupSettings
    {
        [DataMember(Order = 1)] public string FromAsset { get; set; }
        [DataMember(Order = 2)] public string ToAsset { get; set; }
        [DataMember(Order = 3)] public decimal IncreasePercent { get; set; }
        [DataMember(Order = 4)] public decimal DurationMinutes { get; set; }
        [DataMember(Order = 9)] public string ProfileId { get; set; }
        [DataMember(Order = 10)] public decimal VelocityActivationCondition { get; set; }
    }
}