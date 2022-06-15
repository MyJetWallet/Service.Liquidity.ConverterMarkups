using System;
using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    [DataContract]
    public class AutoMarkup
    {
        [DataMember(Order = 1)] public string FromAsset { get; set; }
        [DataMember(Order = 2)] public string ToAsset { get; set; }
        [DataMember(Order = 3)] public decimal Percent { get; set; }
        [DataMember(Order = 4)] public decimal Delay { get; set; }
        [DataMember(Order = 5)] public DateTime StartTime { get; set; }
        [DataMember(Order = 6)] public DateTime StopTime { get; set; }
        [DataMember(Order = 7)] public decimal Markup { get; set; }
        [DataMember(Order = 8)] public decimal PrevMarkup { get; set; }
        [DataMember(Order = 9)] public string User { get; set; }
        [DataMember(Order = 10)] public AutoMarkupState State { get; set; }
        [DataMember(Order = 11)] public decimal Fee { get; set; }
        [DataMember(Order = 12)] public decimal MinMarkup { get; set; }
        [DataMember(Order = 13)] public string ProfileId { get; set; }
    }
}