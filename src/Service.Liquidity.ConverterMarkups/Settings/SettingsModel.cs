using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.ConverterMarkups.Settings
{
    public class SettingsModel
    {
        [YamlProperty("Liquidity.ConverterMarkups.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("Liquidity.ConverterMarkups.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("Liquidity.ConverterMarkups.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}
