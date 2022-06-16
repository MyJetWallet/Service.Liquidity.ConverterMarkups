using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models;

[DataContract]
public class ActivateAutoMarkupRequest
{
    [DataMember(Order = 1)] public AutoMarkupSettings AutoMarkupSettings { get; set; }
    [DataMember(Order = 2)] public ConverterMarkup ConverterMarkup { get; set; }
    [DataMember(Order = 4)] public string UserId { get; set; }
}