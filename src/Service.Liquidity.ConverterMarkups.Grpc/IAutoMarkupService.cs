using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.Liquidity.ConverterMarkups.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Grpc
{
    [ServiceContract]
    public interface IAutoMarkupService
    {
        // Status
        [OperationContract]
        Task<GetAutoMarkupsResponse> GetAutoMarkupStatusAsync();

        //Settings
        [OperationContract, Obsolete]
        Task<AutoMarkupSettingsResponse> ActivateAutoMarkupSettingsAsync(AutoMarkupSettingsRequest request);
        
        [OperationContract]
        Task<ActivateAutoMarkupSettingsResponse> ActivateAutoMarkupAsync(ActivateAutoMarkupRequest request);

        [OperationContract]
        Task<UpsertAutoMarkupSettingsResponse> UpsertAutoMarkupSettingsAsync(UpsertAutoMarkupSettingsRequest request);

        [OperationContract]
        Task<GetAutoMarkupSettingsResponse> GetAutoMarkupSettingsAsync();

        [OperationContract]
        Task<RemoveAutoMarkupSettingsResponse> RemoveAutoMarkupSettingsAsync(RemoveAutoMarkupSettingsRequest request);
        
        [OperationContract]
        Task<DeactivateAutoMarkupResponse> DeactivateAutoMarkupAsync(DeactivateAutoMarkupRequest request);
    }
}