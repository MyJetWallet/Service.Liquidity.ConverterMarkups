using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.Velocity.Grpc;
using Service.Liquidity.Velocity.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Jobs
{
    public class ActivateAutoMarkupJob : IStartable
    {
        private const int TimerSpanSec = 25;
        private readonly MyTaskTimer _operationsTimer;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> _autoMarkupWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> _autoMarkupReader;
        private readonly IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> _autoMarkupSettingReader;
        private readonly IMarkupVelocityService _markupVelocityService;
        private readonly IMyNoSqlServerDataReader<ConverterMarkupNoSqlEntity> _markupReader;
        private readonly ILogger<ActivateAutoMarkupJob> _logger;

        public ActivateAutoMarkupJob(
            ILogger<ActivateAutoMarkupJob> logger,
            IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> autoMarkupWriter,
            IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> autoMarkupReader,
            IMyNoSqlServerDataReader<AutoMarkupSettingsNoSqlEntity> autoMarkupSettingReader,
            IMarkupVelocityService markupVelocityService,
            IMyNoSqlServerDataReader<ConverterMarkupNoSqlEntity> markupReader
        )
        {
            _logger = logger;
            _autoMarkupWriter = autoMarkupWriter;
            _autoMarkupReader = autoMarkupReader;
            _autoMarkupSettingReader = autoMarkupSettingReader;
            _markupVelocityService = markupVelocityService;
            _markupReader = markupReader;

            _operationsTimer = new MyTaskTimer(nameof(AutoMarkupBackgroundJob),
                TimeSpan.FromSeconds(TimerSpanSec), logger, Process);
        }

        public void Start()
        {
            _operationsTimer.Start();
        }

        private async Task Process()
        {
            try
            {
                var settings = _autoMarkupSettingReader.Get()?
                    .Select(s => s.AutoMarkupSettings) ?? new List<AutoMarkupSettings>();
                var velocityResp = await _markupVelocityService.GetAllVelocitiesAsync(new GetVelocityRequest());

                if (velocityResp.IsError)
                {
                    _logger.LogWarning("Can't do ActivateAutoMarkupJob. Failed to get velocity: {@Mess}", velocityResp.ErrorMessage);
                    return;
                }

                foreach (var setting in settings.Where(s => s.VelocityActivationCondition != 0 &&
                                                            s.DurationMinutes != 0 &&
                                                            s.IncreasePercent != 0))
                {
                    var velocity = velocityResp.Items?.FirstOrDefault(i => i.Asset == setting.FromAsset);

                    if (velocity != null && velocity.Velocity >= setting.VelocityActivationCondition)
                    {
                        var pk = AutoMarkupNoSqlEntity.GeneratePartitionKey(setting.ProfileId);
                        var rk = AutoMarkupNoSqlEntity.GenerateRowKey(setting.FromAsset, setting.ToAsset);
                        var autoMarkup= _autoMarkupReader.Get(pk, rk);
                        
                        if (autoMarkup?.AutoMarkup != null && autoMarkup.AutoMarkup.State == AutoMarkupState.Active)
                        {
                            continue;
                        }
                        
                        var markup = _markupReader.Get(pk, rk);

                        if (markup?.ConverterMarkup == null)
                        {
                            _logger.LogWarning(
                                "Can't check auto markup for {@AssetFrom}{@AssetTo}. Converter markup not found",
                                setting.FromAsset, setting.ToAsset);
                            continue;
                        }
                        
                        var startTime = DateTime.UtcNow;
                        var stopTime = startTime.AddMinutes(decimal.ToDouble(setting.DurationMinutes));

                        var newMarkup = markup.ConverterMarkup.Markup + markup.ConverterMarkup.Markup * setting.IncreasePercent / 100;
                        
                        await _autoMarkupWriter.InsertAsync(AutoMarkupNoSqlEntity.Create(new AutoMarkup
                        {
                            FromAsset = setting.FromAsset,
                            ToAsset = setting.ToAsset,
                            Percent = setting.IncreasePercent,
                            Delay = setting.DurationMinutes,
                            Markup = newMarkup,
                            StartTime = startTime,
                            StopTime = stopTime,
                            PrevMarkup = markup.ConverterMarkup.Markup,
                            User = "ConverterMarkupsService",
                            State = AutoMarkupState.Pending,
                            Fee = markup.ConverterMarkup.Fee,
                            MinMarkup = markup.ConverterMarkup.MinMarkup,
                            ProfileId = setting.ProfileId,
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to do ActivateAutoMarkupJob");
            }
        }
    }
}