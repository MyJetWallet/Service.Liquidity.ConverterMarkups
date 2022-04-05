using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models
{
    [DataContract]
    public class AutoMarkupSettingsRequest
    {
        [DataMember(Order = 1)] public AutoMarkupSettings Markup { get; set; }
    }
}