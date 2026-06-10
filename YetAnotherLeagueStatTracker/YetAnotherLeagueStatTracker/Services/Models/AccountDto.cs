using System.Text.Json.Serialization;

namespace YetAnotherLeagueStatTracker.Services.Models;


public class AccountDto
{
    public AccountDto(string puuid, string gameName, string tagLine)
    {
        Puuid = puuid;
        GameName = gameName;
        TagLine = tagLine;
    }
    
    public string Puuid { get; set; }
    public string GameName { get; set; }
    public string TagLine { get; set; }
}