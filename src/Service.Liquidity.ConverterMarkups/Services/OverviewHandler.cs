using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.MyNoSql;
using Service.Liquidity.ConverterMarkups.Domain.Models;

namespace Service.Liquidity.ConverterMarkups.Services
{
    public class OverviewHandler
    {
        private readonly ILogger<OverviewHandler> _logger;
        private readonly IMyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity> _markupWriter;
        private readonly IMyNoSqlServerDataWriter<ConverterMarkupOverviewNoSqlEntity> _overviewWriter;
        private readonly IMyNoSqlServerDataReader<AssetNoSqlEntity> _assetsReader;
        private const string AllSymbol = "*";

        public OverviewHandler(IMyNoSqlServerDataWriter<ConverterMarkupOverviewNoSqlEntity> overviewWriter,
            ILogger<OverviewHandler> logger, 
            IMyNoSqlServerDataWriter<ConverterMarkupNoSqlEntity> markupWriter, 
            IMyNoSqlServerDataReader<AssetNoSqlEntity> assetsReader)
        {
            _overviewWriter = overviewWriter;
            _logger = logger;
            _markupWriter = markupWriter;
            _assetsReader = assetsReader;
        }

        public async Task<MarkupOverview> GetOverview()
        {
            var overviewNoSql = await _overviewWriter.GetAsync();
            var overEntity = overviewNoSql.FirstOrDefault()?.MarkupOverview;
            return overEntity;
        }
        
        public async Task UpdateOverview()
        {
            var markupSettings = await _markupWriter.GetAsync();
            var assets = _assetsReader.Get();
            
            await UpdateOverview(markupSettings.Select(e => e.ConverterMarkup).ToList(), 
                assets.Select(e => e.Symbol).Distinct().ToList());
        }
        
        public async Task UpdateOverview(List<string> assets)
        {
            var markupSettings = await _markupWriter.GetAsync();
            await UpdateOverview(markupSettings.Select(e => e.ConverterMarkup).ToList(), assets);
        }
        public async Task UpdateOverview(List<ConverterMarkup> markupSettings)
        {
            var assets = _assetsReader.Get();
            await UpdateOverview(markupSettings, assets.Select(e => e.Symbol).Distinct().ToList());
        }

        private async Task UpdateOverview(IReadOnlyCollection<ConverterMarkup> markupSettings, IReadOnlyCollection<string> assets)
        {
            var overview = new MarkupOverview()
            {
                Overview = new List<ConverterMarkup>()
            };
            foreach (var assetFrom in assets)
            {
                foreach (var assetTo in assets)
                {
                    var markup = GetMarkup(markupSettings, assetFrom, assetTo);
                    overview.Overview.Add(new ConverterMarkup()
                    {
                        FromAsset = assetFrom,
                        ToAsset = assetTo,
                        Markup = markup
                    });
                }
            }
            if (overview.Overview.Any())
            {
                await _overviewWriter.InsertOrReplaceAsync(ConverterMarkupOverviewNoSqlEntity.Create(overview));
                _logger.LogInformation("Overview is updated: {overviewJson}", JsonConvert.SerializeObject(overview));
            }
            else
            {
                _logger.LogError("Cannot update markup overview");
            }
        }

        private decimal GetMarkup(IReadOnlyCollection<ConverterMarkup> converterMarkups, string assetFrom, string assetTo)
        {
            var directMarkUp = GetMarkUpByPair(converterMarkups, assetFrom, assetTo);
            if (directMarkUp != 0m)
            {
                return directMarkUp;
            }
            var markUpFromAssetToAll = GetMarkUpByPair(converterMarkups, assetFrom, AllSymbol);
            var markUpAllToAssetTo = GetMarkUpByPair(converterMarkups, AllSymbol, assetTo);
            if (markUpFromAssetToAll != 0m && markUpAllToAssetTo != 0m)
            {
                return Math.Max(markUpFromAssetToAll, markUpAllToAssetTo);
            }
            if (markUpFromAssetToAll != 0m)
            {
                return markUpFromAssetToAll;
            }
            if (markUpAllToAssetTo != 0m)
            {
                return markUpAllToAssetTo;
            }
            var markUpAllToAll = GetMarkUpByPair(converterMarkups, AllSymbol, AllSymbol);
            if (markUpAllToAll != 0m)
            {
                return markUpAllToAll;
            }
            _logger.LogError($"Cannot find markup for assetFrom: {assetFrom} and assetTo: {assetTo}");
            return 0m;
        }

        private static decimal GetMarkUpByPair(IEnumerable<ConverterMarkup> converterMarkups, string assetFrom, string assetTo)
        {
            var markup = converterMarkups.FirstOrDefault(e => e.FromAsset == assetFrom && e.ToAsset == assetTo);
            return markup?.Markup ?? 0m;
        }
    }
}