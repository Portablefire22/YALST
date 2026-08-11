namespace YetAnotherLeagueStatTracker.Services.Routing;

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

   /// <summary>
   /// Determines RegionalRouting value to use for a given PlatformRouting value
   /// </summary>
   /// <param name="platform">PlatformRouting value to translate to RegionalRouting</param>
   /// <returns>PlatformRouting's associated RegionalRouting value</returns>
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
         _ => throw new ArgumentOutOfRangeException(platform)
      };
   }
}