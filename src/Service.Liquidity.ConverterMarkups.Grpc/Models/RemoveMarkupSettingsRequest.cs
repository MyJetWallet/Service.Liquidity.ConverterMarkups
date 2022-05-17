using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class RemoveMarkupSettingsRequest
    {
        [DataMember(Order = 1)] public string FromAsset { get; set; }
        [DataMember(Order = 2)] public string ToAsset { get; set; }
        [DataMember(Order = 3)] public string ProfileId { get; set; }
    }
}