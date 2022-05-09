namespace Service.Liquidity.ConverterMarkups.Client
{
    public interface IMarkupExtractor
    {
        decimal GetMarkup(string fromAsset, string toAsset, string profile);
        decimal GetMinMarkup(string fromAsset, string toAsset, string profile);
        decimal GetFee(string fromAsset, string toAsset, string profile);
    }
}