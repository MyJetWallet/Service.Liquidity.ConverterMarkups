using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class RemoveAutoMarkupSettingsRequest
    {
        [DataMember(Order = 1)] public string FromAsset { get; set; }
        [DataMember(Order = 2)] public string ToAsset { get; set; }
    }
}