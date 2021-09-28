using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class UpsertMarkupSettingsRequest
    {
        [DataMember(Order = 1)] public List<ConverterMarkup> MarkupSettings { get; set; }
    }
}