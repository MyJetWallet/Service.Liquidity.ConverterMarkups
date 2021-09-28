using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Liquidity.ConverterMarkups.Grpc;
using Service.Liquidity.ConverterMarkups.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Services
{
    public class ConverterMarkupService: IConverterMarkupService
    {
        private readonly ILogger<ConverterMarkupService> _logger;

        public ConverterMarkupService(ILogger<ConverterMarkupService> logger)
        {
            _logger = logger;
        }

        public Task<GetMarkupSettingsResponse> GetMarkupSettingsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UpsertMarkupSettingsResponse> UpsertMarkupSettingsAsync(UpsertMarkupSettingsRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GetMarkupOverviewResponse> GetMarkupOverviewAsync()
        {
            throw new NotImplementedException();
        }
    }
}
