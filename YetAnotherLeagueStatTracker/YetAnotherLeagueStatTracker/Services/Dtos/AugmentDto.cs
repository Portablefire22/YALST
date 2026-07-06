namespace YetAnotherLeagueStatTracker.Services.Models;

public class AugmentsDto
{
    public AugmentDto[] Augments { get; set; }
}

public class AugmentDto
{
    public string ApiName { get; set; }
    public string Desc { get; set; }
    public string IconLarge { get; set; }
    public string IconSmall { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public int Rarity { get; set; }

    public string GetLargeImageUrl(string version = "latest")
    {
        return $"https://raw.communitydragon.org/{version}/game/{IconLarge}";
    }
    public string GetSmallImageUrl(string version = "latest")
    {
        return $"https://raw.communitydragon.org/{version}/game/{IconSmall}";
    }
}