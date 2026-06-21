using YetAnotherLeagueStatTracker.Services.Dtos.MatchHistory;

namespace YetAnotherLeagueStatTracker.Services.Models.MatchHistory;

public class MatchDto
{
    public MetaDataDto Metadata { get; set; }
    public MatchInfoDto Info { get; set; }
}

public class MetaDataDto
{
    public string DataVersion { get; set; }
    public string MatchId { get; set; }
    public string[] Participants { get; set; } 
}

public class MatchInfoDto
{
    public string EndOfGameResult { get; set; }
    public long GameCreation { get; set; }
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public long GameId { get; set; }
    public string GameMode { get; set; }
    public string GameName { get; set; }
    public long GameStartTimestamp { get; set; }
    public string GameType { get; set; }
    public string GameVersion { get; set; }
    public int MapId { get; set; }
    public ParticipantDto[] Participants { get; set; }
    public string PlatformId { get; set; }
    public int QueueId { get; set; }
    public TeamDto[] Teams { get; set; }
    public string TournamentCode { get; set; }
}

public class TeamDto
{
    public BanDto[] Bans { get; set; }
    public ObjectivesDto Objectives { get; set; }
    public int TeamId { get; set; }
    public bool Win { get; set; }
}

public class BanDto
{
    public int ChampionId { get; set; }
    public int PickTurn { get; set; }
}

public class ObjectivesDto
{
    public ObjectivesDto Baron { get; set; }
    public ObjectivesDto Champion { get; set; }
    public ObjectivesDto Dragon { get; set; }
    public ObjectivesDto Horde { get; set; }
    public ObjectivesDto Inhibitor { get; set; }
    public ObjectivesDto RiftHerald { get; set; }
    public ObjectivesDto Tower { get; set; }
}

public class ObjectiveDto
{
    public bool First { get; set; }
    public int Kills { get; set; }
}