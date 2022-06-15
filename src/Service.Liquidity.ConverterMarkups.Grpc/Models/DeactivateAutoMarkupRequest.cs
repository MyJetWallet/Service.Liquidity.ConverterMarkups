using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models;

[DataContract]
public class DeactivateAutoMarkupRequest
{
    [DataMember(Order = 1)] public AutoMarkup AutoMarkup { get; set; }
    [DataMember(Order = 2)] public string UserId { get; set; }
}