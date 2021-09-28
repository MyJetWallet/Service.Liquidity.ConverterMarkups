using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Liquidity.ConverterMarkups.Grpc;

namespace Service.Liquidity.ConverterMarkups.Client
{
    [UsedImplicitly]
    public class ConverterMarkupsClientFactory: MyGrpcClientFactory
    {
        public ConverterMarkupsClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IConverterMarkupService GetHelloService() => CreateGrpcService<IConverterMarkupService>();
    }
}
