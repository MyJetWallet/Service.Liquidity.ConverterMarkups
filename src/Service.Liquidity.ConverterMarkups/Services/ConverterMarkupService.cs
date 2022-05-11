using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.ConverterMarkups.Grpc;
using Service.Liquidity.ConverterMarkups.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Services
{
    public class ConverterMarkupService: IConverterMarkupService
    {
        private readonly ILogger<ConverterMarkupService> _logger;
        private readonly IMyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity> _markupWriter;
        private readonly IMyNoSqlServerDataReader<ConverterMarkupNoSqlEntity> _markupReader;

        private readonly OverviewHandler _overviewHandler;

        public ConverterMarkupService(ILogger<ConverterMarkupService> logger, 
            IMyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity> markupWriter, 
            OverviewHandler overviewHandler, 
            IMyNoSqlServerDataReader<ConverterMarkupNoSqlEntity> markupReader)
        {
            _logger = logger;
            _markupWriter = markupWriter;
            _overviewHandler = overviewHandler;
            _markupReader = markupReader;
        }

        public async Task<GetMarkupSettingsResponse> GetMarkupSettingsAsync()
        {
            try
            {
                var markups = _markupReader.Get();
                return new GetMarkupSettingsResponse()
                {
                    Success = true,
                    MarkupSettings = markups.Select(e => e.ConverterMarkup).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetMarkupSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<UpsertMarkupSettingsResponse> UpsertMarkupSettingsAsync
            (UpsertMarkupSettingsRequest request)
        {
            try
            {
                var noSqlEntities = request
                    .MarkupSettings.Select(ConverterMarkupNoSqlEntity.Create);
                await _markupWriter.BulkInsertOrReplaceAsync(noSqlEntities);

                var markupSettings = await _markupWriter.GetAsync();

                await _overviewHandler
                    .UpdateOverview(markupSettings.Select(e => e.ConverterMarkup)
                        .ToList());
                
                return new UpsertMarkupSettingsResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new UpsertMarkupSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RemoveMarkupSettingsResponse> RemoveMarkupSettingsAsync(RemoveMarkupSettingsRequest request)
        {
            try
            {
                await _markupWriter.DeleteAsync(request.FromAsset, request.ToAsset);
                await _overviewHandler.UpdateOverview();
                
                return new RemoveMarkupSettingsResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new RemoveMarkupSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<GetMarkupOverviewResponse> GetMarkupOverviewAsync(GetMarkupOverviewRequest request)
        {
            try
            {
                var overview = await _overviewHandler.GetOverview(request.ProfileId);
                if (overview != null)
                {
                    return new GetMarkupOverviewResponse()
                    {
                        Success = true,
                        Overview = overview
                    };
                }
                var errorMessage = "ConverterMarkupItems is empty.";
                _logger.LogError(errorMessage);
                return new GetMarkupOverviewResponse()
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetMarkupOverviewResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
