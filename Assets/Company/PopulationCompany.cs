public class PopulationCompany : Company
{
    private PopulationCompany() {}

    public static class PopulationCompanyBuilder
    {
        public static CompanyBuilder<PopulationCompany> Create()
                             => CompanyBuilder.For<PopulationCompany>();
    }
}