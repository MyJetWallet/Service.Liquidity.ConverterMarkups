using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.ConverterMarkups.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc;

[ServiceContract]
public interface IMarkupProfilesService
{
    [OperationContract] public Task<ProfilesResponse> GetAllProfiles();
    
    [OperationContract] public Task<OperationResponse> CreateProfile(CreateProfileRequest request);
    
    [OperationContract] public Task<OperationResponse> DeleteProfile(DeleteProfileRequest request);
}