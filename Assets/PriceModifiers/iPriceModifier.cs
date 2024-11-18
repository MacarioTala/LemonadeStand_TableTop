public interface iPriceModifier
{
    decimal Apply(decimal base_price, Good good, Market market);
}