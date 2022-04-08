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
    public class AutoMarkupService: IAutoMarkupService
    {
        private readonly ILogger<AutoMarkupService> _logger;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingReader;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> _autoMarkupWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> _autoMarkupReader;

        public AutoMarkupService(ILogger<AutoMarkupService> logger, 
            IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> autoMarkupReader, 
            IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> autoMarkupWriter, 
            IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingWriter, 
            IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingReader)
        {
            _logger = logger;
            _autoMarkupReader = autoMarkupReader;
            _autoMarkupWriter = autoMarkupWriter;
            _autoMarkupSettingWriter = autoMarkupSettingWriter;
            _autoMarkupSettingReader = autoMarkupSettingReader;
        }

        public async Task<GetAutoMarkupSettingsResponse> GetAutoMarkupSettingsAsync()
        {
            try
            {
                var markups = _autoMarkupSettingReader.Get();
                return new GetAutoMarkupSettingsResponse()
                {
                    Success = true,
                    AutoMarkupItems = markups.Select(e => e.AutoMarkupSettings).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new GetAutoMarkupSettingsResponse()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RemoveAutoMarkupSettingsResponse> RemoveAutoMarkupSettingsAsync(RemoveAutoMarkupSettingsRequest request)
        {
            try
            {
                await _autoMarkupWriter.DeleteAsync(request.FromAsset, request.ToAsset);
                
                return new RemoveAutoMarkupSettingsResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new RemoveAutoMarkupSettingsResponse()
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
                    if (request.Markup.Delay == 0m)
                    {
                        return new AutoMarkupSettingsResponse
                        {
                            Success = false,
                            ErrorMessage = "Delay should be more then zero",
                        };
                    }

                    await _autoMarkupSettingWriter
                        .InsertOrReplaceAsync(AutoMarkupSettingsNoSqlEntity.Create(new AutoMarkupSettings
                        {
                            FromAsset = request.Markup.FromAsset,
                            ToAsset = request.Markup.ToAsset,
                            Percent = request.Markup.Percent,
                            Delay = request.Markup.Delay,
                            UserId = request.Markup.UserId,
                            PrevMarkup = request.Markup.PrevMarkup,
                            Fee = request.Markup.Fee,
                            MinMarkup = request.Markup.MinMarkup    
                        }));

                    //Add to status table
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


        public async Task<GetAutoMarkupsResponse> GetAutoMarkupStatusAsync()
        {
            try
            {
                var entities = _autoMarkupReader.Get();
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
