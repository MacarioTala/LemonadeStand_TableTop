public class OrderSummaryLineItem 
{
    public string GoodName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }    

    public override string ToString()
    {
        return $"{Quantity} {GoodName}  @ {Price}";
    }
}
