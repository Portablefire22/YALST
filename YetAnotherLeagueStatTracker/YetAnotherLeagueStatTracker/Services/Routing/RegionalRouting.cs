namespace YetAnotherLeagueStatTracker.Services;

public class RegionalRouting
{
   public const string America = "americas";
   public const string Asia = "asia";
   public const string Europe = "europe";
   public const string Sea = "Sea";

   public static bool IsValid(string input)
   {
      return input is America or Europe or Asia or Sea;
   }

   public static string FromRegion(string platform)
   {
      return platform.ToLowerInvariant() switch
      {
         PlatformRouting.NorthAmerica => America,
         PlatformRouting.Brazil => America,
         PlatformRouting.LatinAmerica1 => America,
         PlatformRouting.LatinAmerica2 => America,
         
         PlatformRouting.Korea => Asia,
         PlatformRouting.Japan => Asia,
         
         PlatformRouting.EuNe => Europe,
         PlatformRouting.EuW => Europe,
         PlatformRouting.Turkey => Europe,
         PlatformRouting.Russia => Europe,
         
         PlatformRouting.Oceania => Sea,
         PlatformRouting.SG => Sea,
         PlatformRouting.TW => Sea,
         PlatformRouting.VN => Sea,
      };
   }
}