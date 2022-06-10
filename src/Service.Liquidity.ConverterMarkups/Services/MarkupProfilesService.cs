using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.ConverterMarkups.Grpc;
using Service.Liquidity.ConverterMarkups.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Services
{
    public class MarkupProfilesService : IMarkupProfilesService
    {
        private readonly IMyNoSqlServerDataWriter<MarkupProfilesNoSqlEntity> _profileWriter;
        private readonly IMyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity> _markupWriter;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> _autoMarkupWriter;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingWriter;

        private readonly OverviewHandler _overviewHandler;

        public MarkupProfilesService(IMyNoSqlServerDataWriter<MarkupProfilesNoSqlEntity> profileWriter, IMyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity> markupWriter, OverviewHandler overviewHandler, IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> autoMarkupWriter, IMyNoSqlServerDataWriter<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingWriter)
        {
            _profileWriter = profileWriter;
            _markupWriter = markupWriter;
            _overviewHandler = overviewHandler;
            _autoMarkupWriter = autoMarkupWriter;
            _autoMarkupSettingWriter = autoMarkupSettingWriter;
        }

        public async Task<ProfilesResponse> GetAllProfiles()
        {
            var groups = await _profileWriter.GetAsync(MarkupProfilesNoSqlEntity.GeneratePartitionKey(), MarkupProfilesNoSqlEntity.GenerateRowKey());
            return new ProfilesResponse()
            {
                Profiles = groups?.Profiles ?? new List<string>()
            };
        }

        public async Task<OperationResponse> CreateProfile(CreateProfileRequest request)
        {
            try
            {
                var groups = await _profileWriter.GetAsync(MarkupProfilesNoSqlEntity.GeneratePartitionKey(),
                    MarkupProfilesNoSqlEntity.GenerateRowKey());
                var profiles = groups?.Profiles ?? new List<string>();
                profiles.Add(request.ProfileId);
                await _profileWriter.InsertOrReplaceAsync(MarkupProfilesNoSqlEntity.Create(profiles.Distinct().ToList()));
                
                if (!string.IsNullOrWhiteSpace(request.CloneFromProfileId))
                {
                    var entities =
                        await _markupWriter.GetAsync(
                            ConverterMarkupNoSqlEntity.GeneratePartitionKey(request.CloneFromProfileId));

                    var markups = entities.Select(t => t.ConverterMarkup).ToList();
                    foreach (var markup in markups)
                    {
                        markup.ProfileId = request.ProfileId;
                    }

                    await _markupWriter.BulkInsertOrReplaceAsync(markups.Select(ConverterMarkupNoSqlEntity.Create).ToList());
                    await _overviewHandler.UpdateOverview();


                    var autoMarkups =
                        (await _autoMarkupWriter.GetAsync(
                            AutoMarkupNoSqlEntity.GeneratePartitionKey(request.CloneFromProfileId))).Select(t=>t.AutoMarkup).ToList();
                    foreach (var autoMarkup in autoMarkups)
                    {
                        autoMarkup.ProfileId = request.ProfileId;
                    }
                    await _autoMarkupWriter.BulkInsertOrReplaceAsync(autoMarkups.Select(AutoMarkupNoSqlEntity.Create).ToList());
                    
                    
                    var autoMarkupSettings =
                        (await _autoMarkupSettingWriter.GetAsync(
                            AutoMarkupSettingsNoSqlEntity.GeneratePartitionKey(request.CloneFromProfileId))).Select(t=>t.AutoMarkupSettings).ToList();
                    foreach (var setting in autoMarkupSettings)
                    {
                        setting.ProfileId = request.ProfileId;
                    }
                    await _autoMarkupSettingWriter.BulkInsertOrReplaceAsync(autoMarkupSettings.Select(AutoMarkupSettingsNoSqlEntity.Create).ToList());
                }

                return new OperationResponse()
                {
                    IsSuccess = true
                };
            }
            catch (Exception e)
            {
                return new OperationResponse()
                {
                    IsSuccess = false,
                    ErrorText = e.Message
                };
            }
        }

        public async Task<OperationResponse> DeleteProfile(DeleteProfileRequest request)
        {
            try
            {
                var groups = await _profileWriter.GetAsync(MarkupProfilesNoSqlEntity.GeneratePartitionKey(),
                    MarkupProfilesNoSqlEntity.GenerateRowKey());
                var profiles = groups?.Profiles ?? new List<string>();
                profiles.Remove(request.ProfileId);
                await _profileWriter.InsertOrReplaceAsync(MarkupProfilesNoSqlEntity.Create(profiles.Distinct().ToList()));

                var entities =
                    await _markupWriter.GetAsync(ConverterMarkupNoSqlEntity.GeneratePartitionKey(request.ProfileId));
                foreach (var entity in entities)
                {
                    await _markupWriter.DeleteAsync(entity.PartitionKey, entity.RowKey);
                }
                await _overviewHandler.UpdateOverview();

                var autoMarkups =
                    await _autoMarkupWriter.GetAsync(
                        AutoMarkupNoSqlEntity.GeneratePartitionKey(request.ProfileId));
                foreach (var autoMarkup in autoMarkups)
                {
                    await _autoMarkupWriter.DeleteAsync(autoMarkup.PartitionKey, autoMarkup.RowKey);
                }
                    
                    
                var autoMarkupSettings =
                    (await _autoMarkupSettingWriter.GetAsync(
                        AutoMarkupSettingsNoSqlEntity.GeneratePartitionKey(request.ProfileId)));
                foreach (var setting in autoMarkupSettings)
                {
                    await _autoMarkupSettingWriter.DeleteAsync(setting.PartitionKey, setting.RowKey);
                }
                
                return new OperationResponse()
                {
                    IsSuccess = true
                };
            }
            catch (Exception e)
            {
                return new OperationResponse()
                {
                    IsSuccess = false,
                    ErrorText = e.Message
                };
            }
        }
    }
}