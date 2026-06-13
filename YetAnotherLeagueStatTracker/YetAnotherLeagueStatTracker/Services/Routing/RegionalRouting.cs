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
}