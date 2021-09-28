using Autofac;
using Service.Liquidity.ConverterMarkups.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Liquidity.ConverterMarkups.Client
{
    public static class AutofacHelper
    {
        public static void ConverterMarkupsClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ConverterMarkupsClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IConverterMarkupService>().SingleInstance();
        }
    }
}
