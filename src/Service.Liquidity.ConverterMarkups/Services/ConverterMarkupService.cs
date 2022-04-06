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
        private readonly IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingReader;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> _autoMarkupWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> _autoMarkupReader;
        private readonly OverviewHandler _overviewHandler;

        public ConverterMarkupService(ILogger<ConverterMarkupService> logger, 
            IMyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity> markupWriter, 
            OverviewHandler overviewHandler, 
            IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> autoMarkupReader, 
            IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> autoMarkupWriter, 
            IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingWriter, 
            IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingReader)
        {
            _logger = logger;
            _markupWriter = markupWriter;
            _overviewHandler = overviewHandler;
            _autoMarkupReader = autoMarkupReader;
            _autoMarkupWriter = autoMarkupWriter;
            _autoMarkupSettingWriter = autoMarkupSettingWriter;
            _autoMarkupSettingReader = autoMarkupSettingReader;
        }

        public async Task<GetMarkupSettingsResponse> GetMarkupSettingsAsync()
        {
            try
            {
                var markups = await _markupWriter.GetAsync();
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

        public async Task<UpsertMarkupSettingsResponse> UpsertMarkupSettingsAsync(UpsertMarkupSettingsRequest request)
        {
            try
            {
                var noSqlEntities = request.MarkupSettings.Select(ConverterMarkupNoSqlEntity.Create);
                await _markupWriter.BulkInsertOrReplaceAsync(noSqlEntities);

                await _overviewHandler.UpdateOverview(request.MarkupSettings);
                
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

        public async Task<GetMarkupOverviewResponse> GetMarkupOverviewAsync()
        {
            try
            {
                var overview = await _overviewHandler.GetOverview();
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

        public async Task<AutoMarkupSettingsResponse> ActivateAutoMarkupSettingsAsync(AutoMarkupSettingsRequest request)
        {
            try
            {
                if (request != null)
                {
                    var startTime = DateTime.UtcNow;
                    var stopTime = startTime.AddMinutes(Decimal.ToDouble(request.Markup.Delay));

                    var newMarkup = request.Markup.PrevMarkup + request.Markup.PrevMarkup * request.Markup.Percent/100;
                    await _autoMarkupWriter.InsertAsync(AutoMarkupNoSqlEntity.Create(new AutoMarkup
                    {
                        FromAsset = request.Markup.FromAsset,
                        ToAsset = request.Markup.ToAsset,
                        Percent = request.Markup.Percent,
                        Delay = request.Markup.Delay,
                        Markup = newMarkup,
                        StartTime = startTime,
                        StopTime = stopTime,
                        PrevMarkup = request.Markup.PrevMarkup,
                        User = request.Markup.UserId,
                        State = State.None,
                        Fee = request.Markup.Fee,
                        MinMarkup = request.Markup.MinMarkup
                    }));
                }
                return new AutoMarkupSettingsResponse
                {
                    Success = true,
                    ErrorMessage = string.Empty,
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            return new AutoMarkupSettingsResponse
            {
                Success = false,
                ErrorMessage = "Not Implemented",
            };
        }


        public async Task<GetAutoMarkupsResponse> GetAutoMarkupsAsync()
        {
            try
            {
                var entities = _autoMarkupSettingReader.Get();
                if (entities != null)
                {
                    return new GetAutoMarkupsResponse()
                    {
                        Success = true,
                        AutoMarkupItems = entities.Select(e => e.AutoMarkup).ToList()
                    };
                }
                var errorMessage = "AutoMarkupItems is empty.";
                _logger.LogError(errorMessage);
                return new GetAutoMarkupsResponse()
                {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetAutoMarkupsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<UpsertAutoMarkupSettingsResponse> UpsertAutoMarkupSettingsAsync(UpsertAutoMarkupSettingsRequest request)
        {
            try
            {
                var noSqlEntities = request.MarkupSettings.Select(AutoMarkupSettingsNoSqlEntity.Create);
                await _autoMarkupSettingWriter.BulkInsertOrReplaceAsync(noSqlEntities);

                return new UpsertAutoMarkupSettingsResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new UpsertAutoMarkupSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
