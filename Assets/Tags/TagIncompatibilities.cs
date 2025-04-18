using System.Collections.Generic;

public static class TagIncompatibilities
{
    public static readonly Dictionary<string, HashSet<string>> Incompatibilities = new()
    {
        { "WeatherWet"   , new HashSet<string> { "WeatherDry"   }  },
        { "WeatherDry"   , new HashSet<string> { "WeatherWet"   }  },
        { "WeatherCold"  , new HashSet<string> { "WeatherHot"   }  },
        { "WeatherHot"   , new HashSet<string> { "WeatherCold"  }  },
        { "EnnuiIncrease", new HashSet<string> { "EnnuiDecrease", "SuperDisaster" }},
        { "SuperDisaster", new HashSet<string> { "EnnuiIncrease" } },
        { "EnnuiDecrease", new HashSet<string> { "EnnuiIncrease" } },
    };
}