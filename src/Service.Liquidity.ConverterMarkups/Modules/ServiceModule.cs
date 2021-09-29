using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.Liquidity.ConverterMarkups.Domain.Models;
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
            builder.RegisterAssetsDictionaryClients(noSqlClient);

            builder.RegisterType<OverviewHandler>().AsSelf().SingleInstance();
        }
    }
}