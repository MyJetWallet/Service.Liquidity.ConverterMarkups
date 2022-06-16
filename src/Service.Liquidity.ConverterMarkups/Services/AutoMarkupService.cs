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
    public class AutoMarkupService : IAutoMarkupService
    {
        private readonly ILogger<AutoMarkupService> _logger;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingReader;
        private readonly IMyNoSqlServerDataReader<ConverterMarkupNoSqlEntity> _markupReader;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> _autoMarkupWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> _autoMarkupReader;

        public AutoMarkupService(
            ILogger<AutoMarkupService> logger,
            IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> autoMarkupReader,
            IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> autoMarkupWriter,
            IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingWriter,
            IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingReader,
            IMyNoSqlServerDataReader<ConverterMarkupNoSqlEntity> markupReader
        )
        {
            _logger = logger;
            _autoMarkupReader = autoMarkupReader;
            _autoMarkupWriter = autoMarkupWriter;
            _autoMarkupSettingWriter = autoMarkupSettingWriter;
            _autoMarkupSettingReader = autoMarkupSettingReader;
            _markupReader = markupReader;
        }
        
         public async Task<AutoMarkupSettingsResponse> ActivateAutoMarkupSettingsAsync(AutoMarkupSettingsRequest request)
        {
            try
            {
                if (request.Markup.DurationMinutes == 0m)
                {
                    return new AutoMarkupSettingsResponse
                    {
                        Success = false,
                        ErrorMessage = "Delay should be more then zero",
                    };
                }
                
                var pk = AutoMarkupNoSqlEntity.GeneratePartitionKey(request.Markup.ProfileId);
                var rk = AutoMarkupNoSqlEntity.GenerateRowKey(request.Markup.FromAsset, request.Markup.ToAsset);
                var autoMarkup= _autoMarkupReader.Get(pk, rk);

                if (autoMarkup?.AutoMarkup != null && autoMarkup.AutoMarkup.State == AutoMarkupState.Active)
                {
                    return new AutoMarkupSettingsResponse
                    {
                        Success = false,
                        ErrorMessage = "Active markup already exists",
                    };
                }

                var markup = _markupReader.Get(pk, rk);

                if (markup?.ConverterMarkup == null)
                {
                    return new AutoMarkupSettingsResponse
                    {
                        Success = false,
                        ErrorMessage = "Converter markup not found",
                    };
                }

                await _autoMarkupSettingWriter.InsertOrReplaceAsync(
                    AutoMarkupSettingsNoSqlEntity.Create(new AutoMarkupSettings
                    {
                        FromAsset = request.Markup.FromAsset,
                        ToAsset = request.Markup.ToAsset,
                        IncreasePercent = request.Markup.IncreasePercent,
                        DurationMinutes = request.Markup.DurationMinutes,
                        ProfileId = request.Markup.ProfileId,
                    }));

                var startTime = DateTime.UtcNow;
                var stopTime = startTime.AddMinutes(decimal.ToDouble(request.Markup.DurationMinutes));

                var newMarkup = markup.ConverterMarkup.Markup + markup.ConverterMarkup.Markup * request.Markup.IncreasePercent / 100;
                await _autoMarkupWriter.InsertAsync(AutoMarkupNoSqlEntity.Create(new AutoMarkup
                {
                    FromAsset = request.Markup.FromAsset,
                    ToAsset = request.Markup.ToAsset,
                    Percent = request.Markup.IncreasePercent,
                    Delay = request.Markup.DurationMinutes,
                    Markup = newMarkup,
                    StartTime = startTime,
                    StopTime = stopTime,
                    PrevMarkup = markup.ConverterMarkup.Markup,
                    User = "",
                    State = AutoMarkupState.Pending,
                    Fee = markup.ConverterMarkup.Fee,
                    MinMarkup = markup.ConverterMarkup.MinMarkup,
                    ProfileId = request.Markup.ProfileId,
                }));

                return new AutoMarkupSettingsResponse
                {
                    Success = true,
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to activate settings");
                return new AutoMarkupSettingsResponse
                {
                    Success = false,
                    ErrorMessage = e.Message,
                };
            }
        }

        public async Task<GetAutoMarkupSettingsResponse> GetAutoMarkupSettingsAsync()
        {
            try
            {
                var markups = _autoMarkupSettingReader.Get();

                return new GetAutoMarkupSettingsResponse
                {
                    Success = true,
                    AutoMarkupItems = markups.Select(e => e.AutoMarkupSettings).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to GetAutoMarkupSettingsAsync");
                return new GetAutoMarkupSettingsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RemoveAutoMarkupSettingsResponse> RemoveAutoMarkupSettingsAsync(
            RemoveAutoMarkupSettingsRequest request)
        {
            try
            {
                await _autoMarkupSettingWriter.DeleteAsync(AutoMarkupNoSqlEntity.GeneratePartitionKey(request.ProfileId),
                    AutoMarkupNoSqlEntity.GenerateRowKey(request.FromAsset, request.ToAsset));

                return new RemoveAutoMarkupSettingsResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to RemoveAutoMarkupSettingsAsync");
                return new RemoveAutoMarkupSettingsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<DeactivateAutoMarkupResponse> DeactivateAutoMarkupAsync(DeactivateAutoMarkupRequest request)
        {
            try
            {
                var pk = AutoMarkupNoSqlEntity.GeneratePartitionKey(request.AutoMarkup.ProfileId);
                var rk = AutoMarkupNoSqlEntity.GenerateRowKey(request.AutoMarkup.FromAsset, request.AutoMarkup.ToAsset);
                var markup= _autoMarkupReader.Get(pk, rk);
                
                if (markup?.AutoMarkup == null)
                {
                    return new DeactivateAutoMarkupResponse
                    {
                        IsError = true,
                        ErrorMessage = "Markup don't found"
                    };
                }

                markup.AutoMarkup.State = AutoMarkupState.Deactivated;
                await _autoMarkupWriter.InsertOrReplaceAsync(markup);

                return new DeactivateAutoMarkupResponse();
            }
            catch (Exception ex)
            {
                return new DeactivateAutoMarkupResponse
                {
                    IsError = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ActivateAutoMarkupSettingsResponse> ActivateAutoMarkupAsync(ActivateAutoMarkupSettingsRequest request)
        {
            try
            {
                if (request.AutoMarkupSettings.DurationMinutes == 0m)
                {
                    return new ActivateAutoMarkupSettingsResponse
                    {
                        Success = false,
                        ErrorMessage = "Delay should be more then zero",
                    };
                }
                
                var pk = AutoMarkupNoSqlEntity.GeneratePartitionKey(request.AutoMarkupSettings.ProfileId);
                var rk = AutoMarkupNoSqlEntity.GenerateRowKey(request.AutoMarkupSettings.FromAsset, request.AutoMarkupSettings.ToAsset);
                var autoMarkup = _autoMarkupReader.Get(pk, rk);

                if (autoMarkup?.AutoMarkup != null && autoMarkup.AutoMarkup.State == AutoMarkupState.Active)
                {
                    return new ActivateAutoMarkupSettingsResponse
                    {
                        Success = false,
                        ErrorMessage = "Active markup already exists",
                    };
                }

                await _autoMarkupSettingWriter.InsertOrReplaceAsync(
                    AutoMarkupSettingsNoSqlEntity.Create(new AutoMarkupSettings
                    {
                        FromAsset = request.AutoMarkupSettings.FromAsset,
                        ToAsset = request.AutoMarkupSettings.ToAsset,
                        IncreasePercent = request.AutoMarkupSettings.IncreasePercent,
                        DurationMinutes = request.AutoMarkupSettings.DurationMinutes,
                        ProfileId = request.AutoMarkupSettings.ProfileId,
                    }));

                var startTime = DateTime.UtcNow;
                var stopTime = startTime.AddMinutes(decimal.ToDouble(request.AutoMarkupSettings.DurationMinutes));

                var newMarkup = request.ConverterMarkup.Markup + request.ConverterMarkup.Markup * request.AutoMarkupSettings.IncreasePercent / 100;
                await _autoMarkupWriter.InsertAsync(AutoMarkupNoSqlEntity.Create(new AutoMarkup
                {
                    FromAsset = request.AutoMarkupSettings.FromAsset,
                    ToAsset = request.AutoMarkupSettings.ToAsset,
                    Percent = request.AutoMarkupSettings.IncreasePercent,
                    Delay = request.AutoMarkupSettings.DurationMinutes,
                    Markup = newMarkup,
                    StartTime = startTime,
                    StopTime = stopTime,
                    PrevMarkup = request.ConverterMarkup.Markup,
                    User = request.UserId,
                    State = AutoMarkupState.Pending,
                    Fee = request.ConverterMarkup.Fee,
                    MinMarkup = request.ConverterMarkup.MinMarkup,
                    ProfileId = request.ConverterMarkup.ProfileId,
                }));

                return new ActivateAutoMarkupSettingsResponse
                {
                    Success = true,
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to activate settings");
                return new ActivateAutoMarkupSettingsResponse
                {
                    Success = false,
                    ErrorMessage = e.Message,
                };
            }
        }

        public async Task<GetAutoMarkupsResponse> GetAutoMarkupStatusAsync()
        {
            try
            {
                var entities = _autoMarkupReader.Get();

                if (entities == null)
                {
                    const string errorMessage = "AutoMarkupItems is empty.";
                    _logger.LogWarning(errorMessage);

                    return new GetAutoMarkupsResponse
                    {
                        Success = false,
                        ErrorMessage = errorMessage
                    };
                }

                return new GetAutoMarkupsResponse
                {
                    Success = true,
                    AutoMarkupItems = entities.Select(e => e.AutoMarkup).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to GetAutoMarkupStatusAsync");
                return new GetAutoMarkupsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<UpsertAutoMarkupSettingsResponse> UpsertAutoMarkupSettingsAsync(
            UpsertAutoMarkupSettingsRequest request)
        {
            try
            {
                var noSqlEntities = request.MarkupSettings
                    .Select(AutoMarkupSettingsNoSqlEntity.Create)
                    .ToList();
                await _autoMarkupSettingWriter.BulkInsertOrReplaceAsync(noSqlEntities);

                return new UpsertAutoMarkupSettingsResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to UpsertAutoMarkupSettingsAsync");
                return new UpsertAutoMarkupSettingsResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}