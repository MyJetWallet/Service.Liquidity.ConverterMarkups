using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class GetMarkupOverviewResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public MarkupOverview Overview { get; set; }
    }
}