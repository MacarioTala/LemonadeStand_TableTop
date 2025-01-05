public interface iTransactionManager
{
    public LemonadeStandResultObject ProcessTransaction(ActionContext context);
    public LemonadeStandResultObject ProcessMarketTransaction(ActionContext context);
}