// Only classes that implement iPriceSetter can set the price of a good
public interface iPriceSetter
{
    void SetPrice(Good good,float newPrice);
}