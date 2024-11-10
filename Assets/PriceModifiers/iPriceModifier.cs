public interface iPriceModifier
{
    float Apply(float base_price, Good good, Market market);
}