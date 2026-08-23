using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YetAnotherLeagueStatTracker.Client.Data.Dtos;

namespace YetAnotherLeagueStatTracker.Data.LeagueModels;

[PrimaryKey(nameof(Id))]
public class SummonerModel
{
    [Key] public int Id { get; set; }
    public string Puuid { get; set; }
    public long SummonerLevel { get; set; }
    public string GameName { get; set; }

    // Invariant lowercase to allow for easier searching whilst preserving display name
    public string? InternalName { get; set; }
    public string? InternalTag { get; set; }

    public ICollection<RankedModel> RankedModels { get; set; }
    
    public string TagLine { get; set; }
    public string Region { get; set; }
    public int ProfileIconId { get; set; }
    public long RevisionDate { get; set; }

    public RankedModel? GetRank()
    {
        var models = RankedModels?.ToArray();
        return models switch
        {
            { Length: >= 2 } => RankedModel.HighestRank(models[0], models[1]),
            { Length: 1 } => models[0],
            _ => null
        };
    }


    public static SummonerDto ToDto(SummonerModel summonerModel)
    {
        return new SummonerDto()
        {
            GameName = summonerModel.GameName,
            ProfileIconId = summonerModel.ProfileIconId,
            Puuid = summonerModel.Puuid,
            Region = summonerModel.Region,
            RevisionDate = summonerModel.RevisionDate,
            SummonerLevel = summonerModel.SummonerLevel,
            Tagline = summonerModel.TagLine
        };
    }

    public SummonerDto ToDto() => ToDto(this);

}