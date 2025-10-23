using System.Collections.Generic;
using System.Linq;

public partial class DemographicPropertyBag
{
    readonly List<DemographicProperty> Properties = new();

    public DemographicProperty GetProperty(string propertyName)
    {
        var returnValue = Properties
                             .Where(property => property.PropertyName == propertyName)
                             .First();
        return returnValue;
    }

    internal void AddProperty(DemographicProperty propertyToAdd)
    {
        var property = Properties.Where(x => x.PropertyName == propertyToAdd.PropertyName).FirstOrDefault();
        if (property is null)
            Properties.Add(propertyToAdd);
    }

    internal void RemoveProperty(string propertyName)
    {
        var property = Properties.Where(x => x.PropertyName == propertyName).FirstOrDefault();
        if (property is not null)
            Properties.Remove(property);
    }
}
