using TanksRebirth;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.Globals;
using TanksRebirth.GameContent.ModSupport;
using TanksRebirth.GameContent.Systems.LevelSystem;
using TanksRebirth.GameContent.Tanks;
using TanksRebirth.Internals;
using TanksRebirth.Net;

namespace OriginalCampaign;

public class OriginalCampaignMod : TanksMod {
    public override void OnLoad() {
        TankGame.MainThreadTasks.Enqueue(() => {
            try {
                OriginalData.DataFolder = Path.Combine(ModPath, "Data");
                LevelFiles.WriteCampaign(CampaignFile.FilePath, CampaignFile.Create());
            } 
            catch (Exception e) { 
                TankGame.ClientLog.Write($"Failed to install campaign... For whatever reason: {e.Message}\n{e.StackTrace}", RebirthLogLevel.ErrorSilent); 
            }
            GameHandler.OnPostUpdate += OnPostUpdate;
        });
    }

    public override void OnUnload() {
        GameHandler.OnPostUpdate -= OnPostUpdate;
        Reset();
    }

    void OnPostUpdate() {
        try {
            Update();
        } catch (Exception e) {
            TankGame.ClientLog.Write($"The mod encountered an error: {e.Message}\n{e.StackTrace}", RebirthLogLevel.ErrorSilent);
        }
    }

    // functionality

    static Campaign? _campaign;
    static int _runSeed;
    static readonly HashSet<int> _rolled = [];
    public static void Reset() {
        _campaign = null;
        _rolled.Clear();
    }

    // only updates original campaign stuff when in the original campaign
    public static void Update() {
        var camp = CampaignGlobals.LoadedCampaign;
        if (camp is null || camp.MetaData.Name != CampaignFile.NAME) { if (_campaign is not null) Reset(); return; }

        if (!ReferenceEquals(camp, _campaign)) {
            _campaign = camp;
            _rolled.Clear();
            // fixed for the whole run
            // in multiplayer the host's session seed came with the campaign, so every client has the same one
            _runSeed = Client.IsConnected() ? Server.SessionSeed : Environment.TickCount ^ Guid.NewGuid().GetHashCode();
        }

        if (CampaignGlobals.InMission) return;
        int id = camp.CurrentMissionId;
        if (id < 0 || id >= camp.CachedMissions.Length || id >= GameParam.MissionCount || _rolled.Contains(id)) return;

        bool twoPlayer = PlayerCount() >= 2;
        var roll = RngMission.Create(_runSeed, id + 1, twoPlayer);

        // original doesnt grant bonus lives in 2p but we do :)
        bool life = !twoPlayer && OriginalData.Param.Missions[id].AwardP1;
        camp.CachedMissions[id] = MissionBuilder.Build(roll, life);
        camp.LoadMission(id);
        _rolled.Add(id);
    }

    static int PlayerCount() {
        if (Client.IsConnected()) {
            int n = 0;
            for (int i = 0; i < MissionBuilder.PLAYER_SLOTS; i++) if (Server.IsOccupied(i)) n++;
            return Math.Max(n, 1);
        }
        return Math.Max(PlayerTank.NumLocalPlayers, 1);
    }
}