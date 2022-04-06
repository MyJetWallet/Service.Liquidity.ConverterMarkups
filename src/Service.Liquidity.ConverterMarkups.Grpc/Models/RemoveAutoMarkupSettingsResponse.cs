using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class RemoveAutoMarkupSettingsResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }
}