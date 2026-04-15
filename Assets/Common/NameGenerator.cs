using System.Threading;

public static class NameGenerator
{
    private static readonly string[] syllables =
    {
        "zen", "vel", "cry", "om", "tri", "nov", "sol", "astra", "neo", "lux",
        "ta", "ra", "ni", "lo", "va", "zen", "cor", "dyn", "tek", "sys",
        "na", "on", "is", "a", "ex", "or", "us", "ix", "ium", "os"
    };

    private static readonly string[] populationSyllables = new[]
    {
        "ka","lo","ma","ra","ta","na","sa","vi","re","do",
        "pa","li","no","se","ti","va","mi","ro","ba","ke",
        "zu","an","el","or","un","is","ar","en","ul","to",
        "da","ko","le","su","ri","mo","ni","fa","ze","tu",
        "bel","cor","dan","fel","gar","hal","jor","kel","lor","mar",
        "nel","por","ran","sel","tor","var","wel","yor","zan"
    };

    private static readonly string[] populationDescriptors = new[]
    {
        "Barrio",
        "Punta",
        "City of",
        "Greater",
        "Port",
        "Distrito",
        "New",
        "Old",
        "Upper",
        "Lower",
        "Zona",
        "Sector",
        "Ward",
        "Province of",
        "Republic of",
        "Territory of",
        "Free City of",
        "Municipality of",
        "Outskirts of",
        "Industrial Zone",
        "Settlement of",
        "Market of"
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

    public static string GenerateCompanyName()
    {
        var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        var numberOfSyllables = UnityEngine.Random.Range(1,4);
        var propNoun = string.Empty;
        var descriptor = Pick(descriptors);
        var suffix = Pick(suffixes);

        for(var i = 0; i<numberOfSyllables;i++)
            propNoun+=Pick(syllables);
        
        var stringToReturn = $"{textInfo.ToTitleCase(propNoun)} {descriptor} {suffix}";

        return stringToReturn;
    }

    public static string GeneratePopulationName()
    {
        var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        var numberOfSyllables = UnityEngine.Random.Range(1,4);
        var populationName = string.Empty;
        var prefix = Pick(populationDescriptors);

        for(var i=0; i<numberOfSyllables;i++)
            populationName+=Pick(populationSyllables);
        
        var stringToReturn = $"{textInfo.ToTitleCase(prefix)} {textInfo.ToTitleCase(populationName)}";

        return stringToReturn;
    }
    private static string Pick(string[] strings)
    {
        return strings[UnityEngine.Random.Range(0,strings.Length)];
    }
}