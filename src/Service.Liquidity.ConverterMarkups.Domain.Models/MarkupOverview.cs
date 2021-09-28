using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Domain.Models
{
    [DataContract]
    public class MarkupOverview
    {
        [DataMember(Order = 1)] public List<ConverterMarkup> Overview { get; set; }
    }
}