using System.Runtime.Serialization;

namespace Service.Liquidity.ConverterMarkups.Grpc.Models;

[DataContract]
public class DeleteProfileRequest
{
    [DataMember (Order = 1)] public string ProfileId { get; set; }
}