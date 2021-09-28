using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.Liquidity.ConverterMarkups.Domain.Models;

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
        }
    }
}