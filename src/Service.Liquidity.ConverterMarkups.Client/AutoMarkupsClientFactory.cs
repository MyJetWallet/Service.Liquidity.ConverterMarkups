using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Liquidity.ConverterMarkups.Grpc;

namespace Service.Liquidity.ConverterMarkups.Client
{
    [UsedImplicitly]
    public class AutoMarkupsClientFactory: MyGrpcClientFactory
    {
        public AutoMarkupsClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IAutoMarkupService GetAutoMarkupService() => CreateGrpcService<IAutoMarkupService>();
    }
}
