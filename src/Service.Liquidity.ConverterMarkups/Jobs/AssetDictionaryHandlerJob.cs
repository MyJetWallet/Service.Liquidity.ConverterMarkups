using System.Collections.Generic;
using System.Linq;
using Autofac;
using MyNoSqlServer.Abstractions;
using Service.AssetsDictionary.MyNoSql;
using Service.Liquidity.ConverterMarkups.Services;

namespace Service.Liquidity.ConverterMarkups.Jobs
{
    public class AssetDictionaryHandlerJob : IStartable
    {
        private readonly IMyNoSqlServerDataReader<AssetNoSqlEntity> _myNoSqlServerDataReader;
        private readonly OverviewHandler _overviewHandler;

        public AssetDictionaryHandlerJob(IMyNoSqlServerDataReader<AssetNoSqlEntity> myNoSqlServerDataReader,
            OverviewHandler overviewHandler)
        {
            _myNoSqlServerDataReader = myNoSqlServerDataReader;
            _overviewHandler = overviewHandler;
        }
        
        public void Start()
        {
            _myNoSqlServerDataReader.SubscribeToUpdateEvents(HandleUpdate, HandleDelete);
        }

        private void HandleDelete(IReadOnlyList<AssetNoSqlEntity> assets)
        {
            _overviewHandler
                .UpdateOverview(assets
                    .Select(e => e.Symbol)
                    .Distinct()
                    .ToList())
                .GetAwaiter()
                .GetResult();
        }

        private void HandleUpdate(IReadOnlyList<AssetNoSqlEntity> assets)
        {
            _overviewHandler
                .UpdateOverview(assets
                    .Select(e => e.Symbol)
                    .Distinct()
                    .ToList())
                .GetAwaiter()
                .GetResult();
        }
    }
}