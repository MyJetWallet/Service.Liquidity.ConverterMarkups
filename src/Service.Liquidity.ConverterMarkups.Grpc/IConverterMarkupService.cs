using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.ConverterMarkups.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc
{
    [ServiceContract]
    public interface IConverterMarkupService
    {
        [OperationContract]
        Task<GetMarkupSettingsResponse> GetMarkupSettingsAsync();
        [OperationContract]
        Task<UpsertMarkupSettingsResponse> UpsertMarkupSettingsAsync(UpsertMarkupSettingsRequest request);
        [OperationContract]
        Task<RemoveMarkupSettingsResponse> RemoveMarkupSettingsAsync(RemoveMarkupSettingsRequest request);
        [OperationContract]
        Task<GetMarkupOverviewResponse> GetMarkupOverviewAsync();
        
        //Auto
        [OperationContract]
        Task<AutoMarkupSettingsResponse> ActivateAutoMarkupSettingsAsync(AutoMarkupSettingsRequest request);
        [OperationContract]
        Task<GetAutoMarkupsResponse> GetAutoMarkupsAsync();
        [OperationContract]
        Task<UpsertAutoMarkupSettingsResponse> UpsertAutoMarkupSettingsAsync(UpsertAutoMarkupSettingsRequest request);

    }
}