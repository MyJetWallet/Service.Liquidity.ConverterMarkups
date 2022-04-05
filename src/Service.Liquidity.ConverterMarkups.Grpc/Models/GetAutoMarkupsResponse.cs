using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class GetAutoMarkupsResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public List<AutoMarkup> AutoMarkupItems { get; set; }
    }
}