namespace YetAnotherLeagueStatTracker.Services.Dtos;

public class SummonerSpellsDto
{
    public string Type { get; set; }
    public string Version { get; set; }
    public Dictionary<string, SummonerSpellDto> Data { get; set; } 
}

public class SummonerSpellDto
{
   public string Id { get; set; }
   public string Key { get; set; }
   public string Name { get; set; }
   public string Description { get; set; }
   public string Tooltip { get; set; }
   public int MaxRank { get; set; }
   public double[] Cooldown { get; set; }
   public string CooldownBurn { get; set; }
   public int SummonerLevel { get; set; }
   public SummonerSpellImageDto Image { get; set; }
   public string Resource { get; set; }

   public string GetImageUrl(string version)
   {
       return $"https://ddragon.leagueoflegends.com/cdn/{version}/img/spell/{Image.Full}";
   }
}

public class SummonerSpellImageDto
{
    public string Full { get; set; }
    public string Sprite { get; set; }
    public string Group { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
}

