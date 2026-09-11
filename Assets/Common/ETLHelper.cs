public static class ETLHelper
{
     public static string MakeKey(string a, string b)
        => string.CompareOrdinal(a,b) < 0? $"{a}|{b}":$"{b}|{a}";
}