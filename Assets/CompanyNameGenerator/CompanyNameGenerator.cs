using System.Threading;

public static class CompanyNameGenerator
{
    private static readonly string[] syllables =
    {
        "zen", "vel", "cry", "om", "tri", "nov", "sol", "astra", "neo", "lux",
        "ta", "ra", "ni", "lo", "va", "zen", "cor", "dyn", "tek", "sys",
        "na", "on", "is", "a", "ex", "or", "us", "ix", "ium", "os"
    };

    private static readonly string [] descriptors =
    {
        "Citrus", "Lemon", "Lime", "Orange", "Berry", "Orchard",
        "Grove", "Harvest", "Juice", "Press", "Extracts",
        "Botanicals", "Produce", "Farms", "Collective",
        "Consortium", "Works", "Industries", "Supply",
        "Refineries", "Processors", "Syndicate",
        "AgriTech", "Hydroponics", "Cultivation",
        "Fresh", "Zest", "Pulp", "Rind", "Essence",
        "Fermentation", "Distillery", "Nectar",
        "Canning", "Preserves", "Distribution"
    };

    private static readonly string [] suffixes =
    {
        "LLC", "Corp", "Co","GMbh","Ltd."
    };

    public static string GenerateName()
    {
        var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        var numberOfSyllables = UnityEngine.Random.Range(1,4);
        var propNoun = string.Empty;
        var descriptor = Pick(descriptors);
        var suffix = Pick(suffixes);

        for(var i = 0; i<numberOfSyllables;i++)
        {
            propNoun+=Pick(syllables);
        }

        var stringToReturn = $"{textInfo.ToTitleCase(propNoun)} {descriptor} {suffix}";

        return stringToReturn;
    }
    private static string Pick(string[] strings)
    {
        return strings[UnityEngine.Random.Range(0,strings.Length)];
    }
}