namespace YetAnotherLeagueStatTracker.Data;

public static class LeagueGamemode
{
   private static Dictionary<string, string> Translation = new Dictionary<string, string>()
   {
      {"CHERRY", "Arena" } 
   };

   public static string GetTranslation(string key)
   {
      if (Translation.TryGetValue(key.ToUpperInvariant(), out string? translation)) return translation;
      return key;
   }
}