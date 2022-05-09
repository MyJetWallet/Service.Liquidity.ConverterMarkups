using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models;

[DataContract]
public class CreateProfileRequest
{
    [DataMember (Order = 1)] public string ProfileId { get; set; }
    [DataMember (Order = 2)] public string CloneFromProfileId { get; set; }

}