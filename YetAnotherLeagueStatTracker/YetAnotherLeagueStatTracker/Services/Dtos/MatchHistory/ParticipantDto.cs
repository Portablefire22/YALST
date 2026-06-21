namespace YetAnotherLeagueStatTracker.Services.Dtos.MatchHistory;

public class ParticipantDto
{
    public PlayerBehavior PlayerBehavior { get; set; }
    public int PlayerScore0 { get; set; }
    public int PlayerScore1 { get; set; }
    public int PlayerScore10 { get; set; }
    public int PlayerScore11 { get; set; }
    public double PlayerScore2 { get; set; }
    public double PlayerScore3 { get; set; }
    public double PlayerScore4 { get; set; }
    public double PlayerScore5 { get; set; }
    public double PlayerScore6 { get; set; }
    public double PlayerScore7 { get; set; }
    public double PlayerScore8 { get; set; }
    public int PlayerScore9 { get; set; }
    public int AllInPings { get; set; }
    public int AssistMePings { get; set; }
    public int Assists { get; set; }
    public int BaronKills { get; set; }
    public int BasicPings { get; set; }
    public bool CausedGameEndFromIgnbSurrender { get; set; }
    public Challenges Challenges { get; set; }
    public int ChampExperience { get; set; }
    public int ChampLevel { get; set; }
    public int ChampionId { get; set; }
    public string ChampionName { get; set; }
    public int ChampionTransform { get; set; }
    public int CommandPings { get; set; }
    public int ConsumablesPurchased { get; set; }
    public int DamageDealtToBuildings { get; set; }
    public int DamageDealtToEpicMonsters { get; set; }
    public int DamageDealtToObjectives { get; set; }
    public int DamageDealtToTurrets { get; set; }
    public int DamageSelfMitigated { get; set; }
    public int DangerPings { get; set; }
    public int Deaths { get; set; }
    public int DetectorWardsPlaced { get; set; }
    public int DoubleKills { get; set; }
    public int DragonKills { get; set; }
    public bool EligibleForProgression { get; set; }
    public int EnemyMissingPings { get; set; }
    public int EnemyVisionPings { get; set; }
    public bool FirstBloodAssist { get; set; }
    public bool FirstBloodKill { get; set; }
    public bool FirstTowerAssist { get; set; }
    public bool FirstTowerKill { get; set; }
    public bool GameEndedInEarlySurrender { get; set; }
    public bool GameEndedInIgnbSurrender { get; set; }
    public bool GameEndedInSurrender { get; set; }
    public int GetBackPings { get; set; }
    public int GoldEarned { get; set; }
    public int GoldSpent { get; set; }
    public int HoldPings { get; set; }
    public string IndividualPosition { get; set; }
    public int InhibitorKills { get; set; }
    public int InhibitorTakedowns { get; set; }
    public int InhibitorsLost { get; set; }
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
    public int ItemsPurchased { get; set; }
    public int KillingSprees { get; set; }
    public int Kills { get; set; }
    public string Lane { get; set; }
    public int LargestCriticalStrike { get; set; }
    public int LargestKillingSpree { get; set; }
    public int LargestMultiKill { get; set; }
    public int LongestTimeSpentLiving { get; set; }
    public int MagicDamageDealt { get; set; }
    public int MagicDamageDealtToChampions { get; set; }
    public int MagicDamageTaken { get; set; }
    public Missions Missions { get; set; }
    public int NeedVisionPings { get; set; }
    public int NeutralMinionsKilled { get; set; }
    public int NexusKills { get; set; }
    public int NexusLost { get; set; }
    public int NexusTakedowns { get; set; }
    public int ObjectivesStolen { get; set; }
    public int ObjectivesStolenAssists { get; set; }
    public int OnMyWayPings { get; set; }
    public int ParticipantId { get; set; }
    public int PentaKills { get; set; }
    public Perks Perks { get; set; }
    public int PhysicalDamageDealt { get; set; }
    public int PhysicalDamageDealtToChampions { get; set; }
    public int PhysicalDamageTaken { get; set; }
    public int Placement { get; set; }
    public int PlayerAugment1 { get; set; }
    public int PlayerAugment2 { get; set; }
    public int PlayerAugment3 { get; set; }
    public int PlayerAugment4 { get; set; }
    public int PlayerAugment5 { get; set; }
    public int PlayerAugment6 { get; set; }
    public int PlayerSubteamId { get; set; }
    public string PositionAssignedByMatchmaking { get; set; }
    public int ProfileIcon { get; set; }
    public int PushPings { get; set; }
    public string Puuid { get; set; }
    public int QuadraKills { get; set; }
    public int RetreatPings { get; set; }
    public string RiotIdGameName { get; set; }
    public string RiotIdTagline { get; set; }
    public string Role { get; set; }
    public int RoleBoundItem { get; set; }
    public string SelectedRolePreferences { get; set; }
    public int SightWardsBoughtInGame { get; set; }
    public int Spell1Casts { get; set; }
    public int Spell2Casts { get; set; }
    public int Spell3Casts { get; set; }
    public int Spell4Casts { get; set; }
    public int SubteamPlacement { get; set; }
    public int Summoner1Casts { get; set; }
    public int Summoner1Id { get; set; }
    public int Summoner2Casts { get; set; }
    public int Summoner2Id { get; set; }
    public string SummonerId { get; set; }
    public int SummonerLevel { get; set; }
    public string SummonerName { get; set; }
    public bool TeamEarlySurrendered { get; set; }
    public bool TeamIgnbSurrendered { get; set; }
    public int TeamId { get; set; }
    public string TeamPosition { get; set; }
    public int TimeCCingOthers { get; set; }
    public int TimePlayed { get; set; }
    public int TotalAllyJungleMinionsKilled { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageDealtToChampions { get; set; }
    public int TotalDamageShieldedOnTeammates { get; set; }
    public int TotalDamageTaken { get; set; }
    public int TotalEnemyJungleMinionsKilled { get; set; }
    public int TotalHeal { get; set; }
    public int TotalHealsOnTeammates { get; set; }
    public int TotalMinionsKilled { get; set; }
    public int TotalTimeCcDealt { get; set; }
    public int TotalTimeSpentDead { get; set; }
    public int TotalUnitsHealed { get; set; }
    public int TripleKills { get; set; }
    public int TrueDamageDealt { get; set; }
    public int TrueDamageDealtToChampions { get; set; }
    public int TrueDamageTaken { get; set; }
    public int TurretKills { get; set; }
    public int TurretTakedowns { get; set; }
    public int TurretsLost { get; set; }
    public int UnrealKills { get; set; }
    public int VisionClearedPings { get; set; }
    public int VisionScore { get; set; }
    public int VisionWardsBoughtInGame { get; set; }
    public int WardsKilled { get; set; }
    public int WardsPlaced { get; set; }
    public bool WasPremadeWithIgnbGameEndCauser { get; set; }
    public bool WasPremadeWithSevereTransgressor { get; set; }
    public bool WasSevereTransgressor { get; set; }
    public bool Win { get; set; }
}

public class PlayerBehavior
{
    public int PlayerBehaviorIsHeroInCombat { get; set; }
}

public class Challenges
{
    public double _2AssistStreakCount { get; set; }
    public double HealFromMapSources { get; set; }
    public double InfernalScalePickup { get; set; }
    public double SwarmDefeatAatrox { get; set; }
    public double SwarmDefeatBriar { get; set; }
    public double SwarmDefeatMiniBosses { get; set; }
    public double SwarmEvolveWeapon { get; set; }
    public double SwarmHave3Passives { get; set; }
    public double SwarmKillEnemy { get; set; }
    public double SwarmPickupGold { get; set; }
    public double SwarmReachLevel50 { get; set; }
    public double SwarmSurvive15Min { get; set; }
    public double SwarmWinWith5EvolvedWeapons { get; set; }
    public double AbilityUses { get; set; }
    public double AcesBefore15Minutes { get; set; }
    public double AlliedJungleMonsterKills { get; set; }
    public double BaronTakedowns { get; set; }
    public double BlastConeOppositeOpponentCount { get; set; }
    public double BountyGold { get; set; }
    public double BuffsStolen { get; set; }
    public double CompleteSupportQuestInTime { get; set; }
    public double ControlWardsPlaced { get; set; }
    public double DamagePerMinute { get; set; }
    public double DamageTakenOnTeamPercentage { get; set; }
    public double DancedWithRiftHerald { get; set; }
    public double DeathsByEnemyChamps { get; set; }
    public double DodgeSkillShotsSmallWindow { get; set; }
    public double DoubleAces { get; set; }
    public double DragonTakedowns { get; set; }
    public double EffectiveHealAndShielding { get; set; }
    public double ElderDragonKillsWithOpposingSoul { get; set; }
    public double ElderDragonMultikills { get; set; }
    public double EnemyChampionImmobilizations { get; set; }
    public double EnemyJungleMonsterKills { get; set; }
    public double EpicMonsterKillsNearEnemyJungler { get; set; }
    public double EpicMonsterKillsWithin30SecondsOfSpawn { get; set; }
    public double EpicMonsterSteals { get; set; }
    public double EpicMonsterStolenWithoutSmite { get; set; }
    public double FirstTurretKilled { get; set; }
    public double FistBumpParticipation { get; set; }
    public double FlawlessAces { get; set; }
    public double FullTeamTakedown { get; set; }
    public double GameLength { get; set; }
    public double GetTakedownsInAllLanesEarlyJungleAsLaner { get; set; }
    public double GoldPerMinute { get; set; }
    public double HadOpenNexus { get; set; }
    public double ImmobilizeAndKillWithAlly { get; set; }
    public double InitialBuffCount { get; set; }
    public double InitialCrabCount { get; set; }
    public double JungleCsBefore10Minutes { get; set; }
    public double JunglerTakedownsNearDamagedEpicMonster { get; set; }
    public double KTurretsDestroyedBeforePlatesFall { get; set; }
    public double Kda { get; set; }
    public double KillAfterHiddenWithAlly { get; set; }
    public double KillParticipation { get; set; }
    public double KilledChampTookFullTeamDamageSurvived { get; set; }
    public double KillingSprees { get; set; }
    public double KillsNearEnemyTurret { get; set; }
    public double KillsOnOtherLanesEarlyJungleAsLaner { get; set; }
    public double KillsOnRecentlyHealedByAramPack { get; set; }
    public double KillsUnderOwnTurret { get; set; }
    public double KillsWithHelpFromEpicMonster { get; set; }
    public double KnockEnemyIntoTeamAndKill { get; set; }
    public double LandSkillShotsEarlyGame { get; set; }
    public double LaneMinionsFirst10Minutes { get; set; }
    public double LegendaryCount { get; set; }
    public double[] LegendaryItemUsed { get; set; }
    public double LostAnInhibitor { get; set; }
    public double MaxKillDeficit { get; set; }
    public double MejaisFullStackInTime { get; set; }
    public double MoreEnemyJungleThanOpponent { get; set; }
    public double MultiKillOneSpell { get; set; }
    public double MultiTurretRiftHeraldCount { get; set; }
    public double Multikills { get; set; }
    public double MultikillsAfterAggressiveFlash { get; set; }
    public double OuterTurretExecutesBefore10Minutes { get; set; }
    public double OutnumberedKills { get; set; }
    public double OutnumberedNexusKill { get; set; }
    public double PerfectDragonSoulsTaken { get; set; }
    public double PerfectGame { get; set; }
    public double PickKillWithAlly { get; set; }
    public double PoroExplosions { get; set; }
    public double QuickCleanse { get; set; }
    public double QuickFirstTurret { get; set; }
    public double QuickSoloKills { get; set; }
    public double RiftHeraldTakedowns { get; set; }
    public double SaveAllyFromDeath { get; set; }
    public double ScuttleCrabKills { get; set; }
    public double SkillshotsDodged { get; set; }
    public double SkillshotsHit { get; set; }
    public double SnowballsHit { get; set; }
    public double SoloBaronKills { get; set; }
    public double SoloKills { get; set; }
    public double StealthWardsPlaced { get; set; }
    public double SurvivedSingleDigitHpCount { get; set; }
    public double SurvivedThreeImmobilizesInFight { get; set; }
    public double TakedownOnFirstTurret { get; set; }
    public double Takedowns { get; set; }
    public double TakedownsAfterGainingLevelAdvantage { get; set; }
    public double TakedownsBeforeJungleMinionSpawn { get; set; }
    public double TakedownsFirstXMinutes { get; set; }
    public double TakedownsInAlcove { get; set; }
    public double TakedownsInEnemyFountain { get; set; }
    public double TeamBaronKills { get; set; }
    public double TeamDamagePercentage { get; set; }
    public double TeamElderDragonKills { get; set; }
    public double TeamRiftHeraldKills { get; set; }
    public double TookLargeDamageSurvived { get; set; }
    public double TurretPlatesTaken { get; set; }
    public double TurretTakedowns { get; set; }
    public double TurretsTakenWithRiftHerald { get; set; }
    public double TwentyMinionsIn3SecondsCount { get; set; }
    public double TwoWardsOneSweeperCount { get; set; }
    public double UnseenRecalls { get; set; }
    public double VisionScorePerMinute { get; set; }
    public double VoidMonsterKill { get; set; }
    public double WardTakedowns { get; set; }
    public double WardTakedownsBefore20M { get; set; }
    public double WardsGuarded { get; set; }
}

public class Missions
{
    public int PlayerScore0 { get; set; }
    public int PlayerScore1 { get; set; }
    public double PlayerScore2 { get; set; }
    public double PlayerScore3 { get; set; }
    public double PlayerScore4 { get; set; }
    public double PlayerScore5 { get; set; }
    public double PlayerScore6 { get; set; }
    public double PlayerScore7 { get; set; }
    public double PlayerScore8 { get; set; }
    public int PlayerScore9 { get; set; }
    public int PlayerScore10 { get; set; }
    public int PlayerScore11 { get; set; }
}

public class Perks
{
    public StatPerks StatPerks { get; set; }
    public Styles[] Styles { get; set; }
}

public class StatPerks
{
    public int Defense { get; set; }
    public int Flex { get; set; }
    public int Offense { get; set; }
}

public class Styles
{
    public string Description { get; set; }
    public Selections[] Selections { get; set; }
    public int Style { get; set; }
}

public class Selections
{
    public int Perk { get; set; }
    public int Var1 { get; set; }
    public int Var2 { get; set; }
    public int Var3 { get; set; }
}


