using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.ConverterMarkups.Settings
{
    public class SettingsModel
    {
        [YamlProperty("ConverterMarkups.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("ConverterMarkups.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("ConverterMarkups.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("ConverterMarkups.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("ConverterMarkups.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
        
        [YamlProperty("ConverterMarkups.LiquidityVelocityGrpcServiceUrl")]
        public string LiquidityVelocityGrpcServiceUrl { get; set; }
    }
}
