public static class HistoricalRecordHelper
{
    public static void JournalSuccessEntry(Market market,Order order, HistoricalRecord record)
    {
        record.Result=order.IsFullyFilled?OrderResultEnum.Filled:OrderResultEnum.PartiallyFilled;
        market.LogHistoricalRecord(record);

        order.OrderStatus = LemonadeStandResultObject.Success();
    }
    public static void JournalRejectEntry(Market market,Order order, HistoricalRecord record, string message, ResultTypeEnum resultType)
    {
        record.Result = OrderResultEnum.Rejected;
        record.Message = message;
        market.LogHistoricalRecord(record);

        order.OrderStatus = LemonadeStandResultObject.Failure(resultType, message);
    }

     public static HistoricalRecord CreateLocalHistoricalRecord(Market market,Order order)
        {
            return new HistoricalRecord()
            {
                OriginalOrderSnapshot = order.ToOrderSnapshot(),
                CreatedInPeriod = market.CurrentPeriod,
                FilledQuantity = order.FilledQuantity,
                RemainingQuantity = order.RemainingQuantity
            };
        }
}