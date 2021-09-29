using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.MyNoSql;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.ConverterMarkups.Jobs;
using Service.Liquidity.ConverterMarkups.Services;

namespace Service.Liquidity.ConverterMarkups.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<ConverterMarkupNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                ConverterMarkupNoSqlEntity.TableName);
            builder.RegisterMyNoSqlWriter<ConverterMarkupOverviewNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                ConverterMarkupOverviewNoSqlEntity.TableName);
            
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterMyNoSqlReader<AssetNoSqlEntity>(noSqlClient, AssetNoSqlEntity.TableName);
            
            builder.RegisterType<OverviewHandler>().AsSelf().SingleInstance();
            builder.RegisterType<AssetDictionaryHandlerJob>().As<IStartable>().AutoActivate().SingleInstance();
        }
    }
}