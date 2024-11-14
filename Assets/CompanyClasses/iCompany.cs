public interface iCompany
{
    string company_name{get;set;}
    void BuyGood(Good good, int quantity, float price,int period);
    void SellGood(Good good, int quantity, float price,int period);
    void Initialize(string company_name, CompanyLevelEnum company_level);
    Inventory Get_inventory();
    float Get_cash();
}