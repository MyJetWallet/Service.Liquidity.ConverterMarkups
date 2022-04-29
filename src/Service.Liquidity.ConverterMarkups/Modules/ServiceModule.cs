using System;
using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.MyNoSql;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.ConverterMarkups.Grpc;
using Service.Liquidity.ConverterMarkups.Jobs;
using Service.Liquidity.ConverterMarkups.Services;

namespace Service.Liquidity.ConverterMarkups.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterMyNoSqlReader<AssetNoSqlEntity>(noSqlClient, AssetNoSqlEntity.TableName);
            builder.RegisterMyNoSqlReader<AutoMarkupNoSqlEntity>(noSqlClient, AutoMarkupNoSqlEntity.TableName);
            builder.RegisterMyNoSqlReader<AutoMarkupSettingsNoSqlEntity>(noSqlClient, AutoMarkupSettingsNoSqlEntity.TableName);
            builder.RegisterMyNoSqlReader<ConverterMarkupNoSqlEntity>(noSqlClient, ConverterMarkupNoSqlEntity.TableName);

            builder.RegisterMyNoSqlWriter<ConverterMarkupNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                ConverterMarkupNoSqlEntity.TableName);
            builder.RegisterMyNoSqlWriter<ConverterMarkupOverviewNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                ConverterMarkupOverviewNoSqlEntity.TableName);
            builder.RegisterMyNoSqlWriter<AutoMarkupNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                AutoMarkupNoSqlEntity.TableName);
            builder.RegisterMyNoSqlWriter<AutoMarkupSettingsNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                AutoMarkupSettingsNoSqlEntity.TableName);

            builder.RegisterType<AssetDictionaryHandlerJob>().As<IStartable>().AutoActivate().SingleInstance();
            builder.RegisterType<AutoMarkupBackgroundJob>().As<IStartable>().As<IDisposable>().AutoActivate().SingleInstance();
            builder.RegisterType<ConverterMarkupService>().As<IConverterMarkupService>();
            builder.RegisterType<OverviewHandler>().AsSelf().SingleInstance();
        }
    }
}