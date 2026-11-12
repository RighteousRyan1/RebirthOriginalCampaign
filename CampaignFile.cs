using TanksRebirth;
using TanksRebirth.GameContent.Systems.LevelSystem;

namespace OriginalCampaign;

// makes the placeholder missions
public static class CampaignFile {
    public const string NAME = "Original Campaign";
    public static string FilePath => Path.Combine(TankGame.SaveDirectory, "Campaigns", NAME + ".campaign");

    // constructs the campaign
    public static Campaign Create() {
        var param = OriginalData.Param;
        var meta = Campaign.CampaignMetaData.GetDefault();
        meta.Name = NAME;
        meta.Description = "The original Wii Tanks campaign.";
        meta.Author = "Nintendo (2006)";
        meta.Version = "1.0";
        meta.Tags = ["Original", "100 missions"];
        meta.StartingLives = 3;
        meta.HasMajorVictory = true;

        var campaign = new Campaign { MetaData = meta };
        var missions = new Mission[GameParam.MissionCount];
        for (int i = 0; i < missions.Length; i++) {
            var roll = RngMission.Create(0, i + 1, false);
            missions[i] = MissionBuilder.Build(roll, param.Missions[i].AwardP1);
        }
        campaign.CachedMissions = missions;
        return campaign;
    }
}
