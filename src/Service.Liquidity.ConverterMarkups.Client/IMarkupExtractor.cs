namespace Service.Liquidity.ConverterMarkups.Client
{
    public interface IMarkupExtractor
    {
        decimal GetMarkup(string fromAsset, string toAsset);
        decimal GetFee(string fromAsset, string toAsset);
    }
}