using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class GetMarkupOverviewRequest
    {
        [DataMember(Order = 1)] public string ProfileId { get; set; }
    }
}