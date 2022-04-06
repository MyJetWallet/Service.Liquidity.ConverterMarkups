using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class UpsertAutoMarkupSettingsRequest
    {
        [DataMember(Order = 1)] public List<AutoMarkupSettings> MarkupSettings { get; set; }
    }
}