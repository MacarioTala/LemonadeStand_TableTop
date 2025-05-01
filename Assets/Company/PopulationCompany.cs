using System;

public class PopulationCompany : Company
{
#region Demographics
    private int _population;
    public int Population
    {
        get => _population;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Population cannot be negative.");
            _population = value;
        }
    }
#endregion
    private PopulationCompany() {}

    public static class PopulationCompanyBuilder
    {
        public static CompanyBuilder<PopulationCompany> Create()
                             => CompanyBuilder.For<PopulationCompany>();
    }
}