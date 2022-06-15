using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    [DataContract]
    public class AutoMarkupSettings
    {
        [DataMember(Order = 1)] public string FromAsset { get; set; }
        [DataMember(Order = 2)] public string ToAsset { get; set; }
        [DataMember(Order = 3)] public decimal Percent { get; set; }
        [DataMember(Order = 4)] public decimal Delay { get; set; }
        [DataMember(Order = 5)] public string UserId { get; set; }
        [DataMember(Order = 6)] public decimal PrevMarkup { get; set; }
        [DataMember(Order = 7)] public decimal Fee { get; set; }
        [DataMember(Order = 8)] public decimal MinMarkup { get; set; }
        [DataMember(Order = 9)] public string ProfileId { get; set; }
        [DataMember(Order = 10)] public decimal VelocityPercent { get; set; }

    }
}