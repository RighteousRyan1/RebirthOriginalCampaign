using TanksRebirth.Internals.Common;

namespace OriginalCampaign;

// struct?
public readonly record struct RngMission(int Mission, bool TwoPlayer, int Map, int[] Tanks) {
    public static RngMission Create(int runSeed, int missionNumber, bool twoPlayer) {
        var row = OriginalData.Param.Missions[missionNumber - 1];
        var (mapMin, mapMax) = twoPlayer ? (row.MapMinP2, row.MapMaxP2) : (row.MapMinP1, row.MapMaxP1);
        var slots = twoPlayer ? row.SlotsP2 : row.SlotsP1;

        var (a, b) = PRNG.GetState();
        PRNG.Seed(unchecked((runSeed * 31 + missionNumber) * 2 + (twoPlayer ? 1 : 0)));

        int map = PRNG.Next(mapMin, mapMax + 1);
        var tanks = slots.Select(s => s < 10 ? s : PRNG.Next(s / 10, s % 10 + 1)).ToArray();

        PRNG.SetState(a, b);
        return new RngMission(missionNumber, twoPlayer, map, tanks);
    }
    public override string ToString() => $"M{Mission} {(TwoPlayer ? "2P" : "1P")} map {Map} tanks [{string.Join(",", Tanks)}]";
}