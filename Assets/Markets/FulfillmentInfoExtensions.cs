using System.Collections.Generic;
using System.Linq;

public static class FulfillmentInfoExtensions
{
    public static float FulfillmentRateFor(this List<FulfillmentInfo> fulfillmentInfoList,Good good)
    {
        return fulfillmentInfoList.FirstOrDefault(x=> x.Good == good)?.FulfillmentRate ?? 0f;
    }
}