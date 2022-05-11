using System;
using Autofac;
using MyNoSqlServer.DataReader;
using Service.Liquidity.ConverterMarkups.Domain.Models;
using Service.Liquidity.ConverterMarkups.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Liquidity.ConverterMarkups.Client
{
    public static class AutofacHelper
    {
        [Obsolete("This property is obsolete. Use RegisterConverterMarkupClient instead.", false)]
        public static void ConverterMarkupsClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ConverterMarkupsClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetConverterMarkupService()).As<IConverterMarkupService>().SingleInstance();
        }

        public static void RegisterConverterMarkupClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ConverterMarkupsClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetConverterMarkupService())
                .As<IConverterMarkupService>().SingleInstance();
            
            builder.RegisterInstance(factory.GetProfileMarkupService())
                .As<IMarkupProfilesService>().SingleInstance();
        }

        public static void RegisterAutoMarkupClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new AutoMarkupsClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetAutoMarkupService())
                .As<IAutoMarkupService>().SingleInstance();
        }

        public static void RegisterMarkupExtractor(this ContainerBuilder builder, IMyNoSqlSubscriber myNoSqlSubscriber)
        {
            var priceSubscriber = new MyNoSqlReadRepository<ConverterMarkupOverviewNoSqlEntity>(myNoSqlSubscriber, 
                ConverterMarkupOverviewNoSqlEntity.TableName);
            
            builder
                .RegisterInstance(new MarkupExtractor(priceSubscriber))
                .As<IMarkupExtractor>().SingleInstance();
        }
    }
}
