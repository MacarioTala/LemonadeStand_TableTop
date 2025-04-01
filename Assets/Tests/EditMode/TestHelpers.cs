using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class TestHelpers
{
    #region Helper methods
public string ListToString(List<InventoryEntry> inventory)
{
    var sb = new StringBuilder();
    foreach (var item in inventory)
    {
        sb.AppendLine($"Good: {item.good.name}, Quantity: {item.quantity}, Price: {item.Cost}");
    }
    return sb.ToString();
}

public static ActionContext CreateActionContext(Order order, Market market,int period,List<Order> counterPartyOrders = null)
{
    return new ActionContext()
    {
        TradeToSubmit = order,
        MarketToSubmitTo = market,
        Period = period,
        CounterPartyOrders = counterPartyOrders
    };
}

public class TestComparer<T> : IEqualityComparer<T>
{
    private readonly string[] exceptions;
    public TestComparer(params string[]exceptions) {
        this.exceptions = exceptions;
     }
    public bool Equals(T x, T y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        var type = typeof(T);
      
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(
                            p => p.PropertyType != typeof(Guid)
                            && !exceptions.Contains(p.Name) 
                        )
                )
        {
            var xValue = property.GetValue(x);
            var yValue = property.GetValue(y);

            if (!NullSafeEquals(xValue,yValue)) 
            {
                return false;
            }
        }
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                        .Where(f => f.FieldType != typeof(Guid)
                            && !exceptions.Contains(f.Name) 
                        )
                )
        {
            var xValue = field.GetValue(x);
            var yValue = field.GetValue(y);

            if (!NullSafeEquals(xValue,yValue)) return false;
        }
        return true;
    }

    public int GetHashCode(T obj)
    {
        if (obj == null) return 0;

        var hash = new HashCode();

        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.PropertyType != typeof(Guid)
                            && !exceptions.Contains(p.Name)
                            )
                )
        {
            hash.Add(property.GetValue(obj));
        }
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
                        .Where(f => f.FieldType != typeof(Guid)
                            && !exceptions.Contains(f.Name)
                        )
                )
        {
            hash.Add(field.GetValue(obj));
        }
        return hash.ToHashCode();
    }
    private bool NullSafeEquals(object x, object y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        if (x is IEnumerable xEnum && y is IEnumerable yEnum)
        {
            var xList = xEnum.Cast<object>().ToList();
            var yList = yEnum.Cast<object>().ToList();

            if (xList.Count != yList.Count) return false;
            
            return xList.SequenceEqual(yList);
        }
        return x.Equals(y);
    }

    public bool ListsAreEquivalent (IEnumerable<T> Expected, IEnumerable<T> Actual,IEqualityComparer<T> comparer)
    {
        if (Expected == null && Actual == null) return true;
        if (Expected == null || Actual == null) return false;

        var expectedList = Expected.ToList();
        var actualList = Actual.ToList();

        if (expectedList.Count != actualList.Count) return false;

        return expectedList.All(expected=> actualList.Any(actual => comparer.Equals(expected, actual)))
            && actualList.All(actual => expectedList.Any(expected => comparer.Equals(actual, expected)));
    }
    
}
#endregion
}
