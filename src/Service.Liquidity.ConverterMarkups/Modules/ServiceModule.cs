using System;
using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.MyNoSql;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.ConverterMarkups.Grpc;
using Service.Liquidity.ConverterMarkups.Jobs;
using Service.Liquidity.ConverterMarkups.Services;
using Service.Liquidity.Velocity.Client;

namespace Service.Liquidity.ConverterMarkups.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMarkupVelocityService(Program.Settings.LiquidityVelocityGrpcServiceUrl);

            var noSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);
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
            builder.RegisterMyNoSqlWriter<MarkupProfilesNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                MarkupProfilesNoSqlEntity.TableName);
            
            builder.RegisterType<AssetDictionaryHandlerJob>().As<IStartable>().AutoActivate().SingleInstance();
            builder.RegisterType<AutoMarkupBackgroundJob>().As<IStartable>().As<IDisposable>().AutoActivate().SingleInstance();
            builder.RegisterType<ConverterMarkupService>().As<IConverterMarkupService>();
            builder.RegisterType<OverviewHandler>().AsSelf().SingleInstance();
            builder.RegisterType<ActivateAutoMarkupJob>().As<IStartable>().AutoActivate().SingleInstance();

        }
    }
}