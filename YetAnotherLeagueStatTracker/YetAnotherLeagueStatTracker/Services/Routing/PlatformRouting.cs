namespace YetAnotherLeagueStatTracker.Services.Routing;

public class PlatformRouting
{
    public const string Brazil = "br1";
    public const string EuNe = "eun1";
    public const string EuW = "euw1";
    public const string Japan = "jp1";
    public const string Korea = "kr";
    public const string LatinAmerica1 = "la1";
    public const string LatinAmerica2 = "la2";
    public const string NorthAmerica = "na1";
    public const string Oceania = "oc1";
    public const string Turkey = "tr1";
    public const string Russia = "ru";
    public const string Philippines = "ph2";
    public const string SG = "sg2";
    public const string TH = "th2";
    public const string TW = "tw2";
    public const string VN = "vn2";

    public static bool IsValid(string input)
    {
        return input is Brazil or EuNe or EuW or Japan or Korea or LatinAmerica1 or LatinAmerica2 or NorthAmerica
            or Oceania or Turkey or Russia or Philippines or SG or TH or TW or VN;
    }
}