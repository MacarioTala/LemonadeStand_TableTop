using System;
    public class DemographicProperty
    {
        public string PropertyName { get; }
        public decimal PropertyValue { get; }
        public string Description{ get; set; }
        public PropertyTypeEnum PropertyType { get;}

        private DemographicProperty(string propertyName, PropertyTypeEnum propertyType, decimal propertyValue, string description = "")
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            PropertyValue = propertyValue;
            Description = description;
        }

        public static DemographicProperty Money(string propertyName, decimal amount, string desc="")
            => new(propertyName, PropertyTypeEnum.Money, amount, desc);

        public static DemographicProperty Percentage(string propertyName, decimal value, string desc="")
            => new(propertyName, PropertyTypeEnum.Percentage, value, desc);

        public static DemographicProperty Raw(string propertyName, decimal value, string desc="")
            => new(propertyName, PropertyTypeEnum.Raw, value, desc);

        public float AsPercentage()
        {
            float retVal;
            if (PropertyType == PropertyTypeEnum.Percentage)
                retVal = (float)Math.Round(PropertyValue);
            else
                throw new InvalidOperationException($"{PropertyName} is not a Percentage");
            return retVal;
        }

        public decimal AsRaw()
            => PropertyType == PropertyTypeEnum.Raw
            ? PropertyValue
            : throw new InvalidOperationException($"{PropertyName} is not a Raw number");


        public decimal AsMoney()
            => PropertyType == PropertyTypeEnum.Money
            ? Math.Round(PropertyValue, 2)
            : throw new InvalidOperationException($"{PropertyName} is not Money");

    }