using System.Collections.Generic;

public interface iCompany
{
    #region Identity
        string company_name{get;set;}
    #endregion

    #region Financials
        List<FixedCost> FixedCosts {get;set;}    
        iFixedCostStrategy FixedCostStrategy{get;set;}
        decimal Get_cash();
        decimal CalculateFixedCostsForPeriod(int period);
    #endregion

    #region Goals and strategies
        List<Goal> Goals{get;set;}
        void CheckCompanyGoals();
        void CompleteGoal(Goal goal);
    #endregion
    
    #region Inventory Management
        void ExpireGoods(int period);
        Inventory GetInventory();
    #endregion

    #region Time
        int CurrentPeriod{get;set;} 
        int StartingPeriod{get;set;}
        void UpdateCurrentPeriod(int period);
    #endregion
    
    #region Trading
        void BuyGood(Good good, int quantity, decimal price,int period);
        void SellGood(Good good, int quantity, decimal price,int period);
    #endregion
}