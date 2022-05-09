using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models;

[DataContract]
public class ProfilesResponse
{
    [DataMember(Order = 1)] public List<string> Profiles { get; set; }
}