﻿using System;
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
                    case State.None:
                        await SetUpNewMarkup(item);
                        break;

                    case State.InProgress:
                        if (IsNeedToSetPrevValue(item))
                        {
                            await SetUpPrevMarkup(item);
                        }
                        break;

                    case State.Done:
                        await RemoveNewMarkup(item);
                        break;
                }

            }
        }

        private async Task RemoveNewMarkup(AutoMarkup item)
        {
            var currTime = DateTime.UtcNow;
            if ((item.StopTime <= currTime && item.State == State.Done))
            {
                await _autoMarkupWriter.DeleteAsync(item.FromAsset, item.ToAsset);
                _logger.LogInformation($"Remove new markup task successfully {item.ToJson()}");
                return;
            }
        }

        private bool IsNeedToSetPrevValue(AutoMarkup item)
        {
            var currTime = DateTime.UtcNow;
            return (item.StopTime <= currTime && item.State == State.InProgress);
        }

        private async Task<bool> SetUpNewMarkup(AutoMarkup item)
        {
            var update = AutoMarkupNoSqlEntity.Create(item);
            update.AutoMarkup.State = State.InProgress;
            //--- For overview
            var allMarkupsResponse = await _markupService.GetMarkupSettingsAsync();
            var allMarkups = allMarkupsResponse?.MarkupSettings ?? new List<ConverterMarkup>();
            var newMarkups = new List<ConverterMarkup>();
            foreach (var markup in allMarkups)
            {
                var newValue = markup.Markup;
                if (markup.FromAsset == item.FromAsset && markup.ToAsset == item.ToAsset && markup.ProfileId == item.ProfileId)
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
            //--- For overview
            var result = await _markupService.UpsertMarkupSettingsAsync(new UpsertMarkupSettingsRequest
            {
                MarkupSettings = newMarkups
            });

            if (result == null || !result.Success)
            {
                _logger.LogError($"Cant setup new markup {update.AutoMarkup.ToJson()}");
                return false;
            }

            await _autoMarkupWriter.InsertOrReplaceAsync(update);
            var prev = update.AutoMarkup.PrevMarkup;
            var curr = update.AutoMarkup.Markup;
            _logger.LogInformation($"Setup new markup {prev}->{curr} task successfully {update.AutoMarkup.ToJson()}");
            return true;
        }

        private async Task<bool> SetUpPrevMarkup(AutoMarkup item)
        {
            var result = await _markupService
                .UpsertMarkupSettingsAsync(new UpsertMarkupSettingsRequest
            {
                    MarkupSettings = new List<ConverterMarkup>()
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

            if (result == null || !result.Success)
            {
                _logger.LogError($"Cant setup prev markup {item.ToJson()}");
                return false;
            }

            var update = AutoMarkupNoSqlEntity.Create(item);
            update.AutoMarkup.State = State.Done;
            var prev = update.AutoMarkup.PrevMarkup;
            var curr = update.AutoMarkup.Markup;
            await _autoMarkupWriter.InsertOrReplaceAsync(update);

            _logger.LogInformation($"Setup prev markup {curr}->{prev} task successfully {item.ToJson()}");
            return true;
        }

    }
}
