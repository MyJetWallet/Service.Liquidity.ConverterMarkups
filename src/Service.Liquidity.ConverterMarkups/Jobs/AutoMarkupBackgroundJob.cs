using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.ConverterMarkups.Grpc;
using Service.Liquidity.ConverterMarkups.Grpc.Models;

namespace Service.Liquidity.ConverterMarkups.Jobs
{
    public class AutoMarkupBackgroundJob : IStartable, IDisposable
    {
        private readonly ILogger<AutoMarkupBackgroundJob> _logger;
        private readonly MyTaskTimer _operationsTimer;
        private readonly IConverterMarkupService _markupService;
        private readonly IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> _autoMarkupWriter;
        private readonly IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> _autoMarkupReader;
        private const int TimerSpanSec = 1;

        public AutoMarkupBackgroundJob(ILogger<AutoMarkupBackgroundJob> logger,
            IConverterMarkupService markupService,
            IMyNoSqlServerDataWriter<AutoMarkupNoSqlEntity> autoMarkupWriter,
            IMyNoSqlServerDataReader<AutoMarkupNoSqlEntity> autoMarkupReader)
        {
            _logger = logger;
            _markupService = markupService;
            _autoMarkupWriter = autoMarkupWriter;
            _autoMarkupReader = autoMarkupReader;
            _operationsTimer = new MyTaskTimer(nameof(AutoMarkupBackgroundJob),
                TimeSpan.FromSeconds(TimerSpanSec), logger, Process);
        }

        public void Start()
        {
            _operationsTimer.Start();
        }

        public void Dispose()
        {
            _operationsTimer?.Stop();
            _operationsTimer?.Dispose();
        }

        private async Task Process()
        {
            foreach (var entity in _autoMarkupReader.Get())
            {
                var item = entity.AutoMarkup;
                
                switch (item.State)
                {
                    case AutoMarkupState.Pending:
                        await SetUpNewMarkup(item);
                        break;
                    case AutoMarkupState.Active:
                        if (IsNeedToSetPrevValue(item))
                        {
                            await SetUpPrevMarkup(item);
                        }
                        break;
                    case AutoMarkupState.Done:
                        await RemoveDoneMarkup(item);
                        break;
                    case AutoMarkupState.Deactivated:
                        await SetUpPrevMarkup(item);
                        await _autoMarkupWriter.DeleteAsync(item.FromAsset, item.ToAsset);
                        break;
                }
            }
        }

        private async Task RemoveDoneMarkup(AutoMarkup item)
        {
            if (item.StopTime <= DateTime.UtcNow && item.State == AutoMarkupState.Done)
            {
                await _autoMarkupWriter.DeleteAsync(item.FromAsset, item.ToAsset);
                
                _logger.LogInformation("Remove new markup task successfully {@Item}", item);
            }
        }

        private static bool IsNeedToSetPrevValue(AutoMarkup item)
        {
            return item.StopTime <= DateTime.UtcNow && item.State == AutoMarkupState.Active;
        }

        private async Task SetUpNewMarkup(AutoMarkup item)
        {
            var update = AutoMarkupNoSqlEntity.Create(item);
            update.AutoMarkup.State = AutoMarkupState.Active;
            var allMarkupsResponse = await _markupService.GetMarkupSettingsAsync();
            var allMarkups = allMarkupsResponse?.MarkupSettings ?? new List<ConverterMarkup>();
            var newMarkups = new List<ConverterMarkup>();

            foreach (var markup in allMarkups)
            {
                var newValue = markup.Markup;
                if (markup.FromAsset == item.FromAsset && markup.ToAsset == item.ToAsset &&
                    markup.ProfileId == item.ProfileId)
                {
                    newValue = item.Markup;
                }

                newMarkups.Add(new ConverterMarkup
                {
                    FromAsset = markup.FromAsset,
                    ToAsset = markup.ToAsset,
                    Markup = newValue,
                    Fee = markup.Fee,
                    MinMarkup = markup.MinMarkup,
                    ProfileId = markup.ProfileId
                });
            }

            var result = await _markupService.UpsertMarkupSettingsAsync(new UpsertMarkupSettingsRequest
            {
                MarkupSettings = newMarkups
            });

            if (result is {Success: false})
            {
                return;
            }

            await _autoMarkupWriter.InsertOrReplaceAsync(update);
            var prev = update.AutoMarkup.PrevMarkup;
            var curr = update.AutoMarkup.Markup;

            _logger.LogInformation("Setup new markup {@Curr}->{@Prev} task successfully {@Item}", curr, prev, item);
        }

        private async Task SetUpPrevMarkup(AutoMarkup item)
        {
            var result = await _markupService.UpsertMarkupSettingsAsync(
                new UpsertMarkupSettingsRequest
                {
                    MarkupSettings = new List<ConverterMarkup>
                    {
                        new ConverterMarkup
                        {
                            FromAsset = item.FromAsset,
                            ToAsset = item.ToAsset,
                            Markup = item.PrevMarkup,
                            Fee = item.Fee,
                            MinMarkup = item.MinMarkup,
                            ProfileId = item.ProfileId
                        }
                    }
                });

            if (result is {Success: false})
            {
                return;
            }

            var update = AutoMarkupNoSqlEntity.Create(item);
            update.AutoMarkup.State = AutoMarkupState.Done;
            var prev = update.AutoMarkup.PrevMarkup;
            var curr = update.AutoMarkup.Markup;
            await _autoMarkupWriter.InsertOrReplaceAsync(update);

            _logger.LogInformation("Setup prev markup {@Curr}->{@Prev} task successfully {@Item}", curr, prev, item);
        }
    }
}