using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    [DataContract]
    public class ConverterMarkup
    {
        [DataMember(Order = 1)] public string FromAsset { get; set; }
        [DataMember(Order = 2)] public string ToAsset { get; set; }
        [DataMember(Order = 3)] public decimal Markup { get; set; }
        [DataMember(Order = 4)] public decimal Fee { get; set; }
    }
}