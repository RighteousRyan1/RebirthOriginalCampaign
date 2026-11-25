using TanksRebirth.Internals.Common;

namespace OriginalCampaign;

public readonly record struct RngMission(int Mission, bool TwoPlayer, int Map, int[] Tanks, int[] PlayerSpawns) {
    public static RngMission Create(int runSeed, int missionNumber, bool twoPlayer, int? previousMap = null) {
        var row = OriginalData.Param.Missions[missionNumber - 1];
        var (mapMin, mapMax) = twoPlayer ? (row.MapMinP2, row.MapMaxP2) : (row.MapMinP1, row.MapMaxP1);
        var slots = twoPlayer ? row.SlotsP2 : row.SlotsP1;

        var (a, b) = PRNG.GetState();
        PRNG.Seed((runSeed * 31 + missionNumber) * 2 + (twoPlayer ? 1 : 0));

        int map = PRNG.Next(mapMin, mapMax + 1);

        // a fixed mission only has one map, so it cant change
        while (map == previousMap && mapMax > mapMin)
            map = PRNG.Next(mapMin, mapMax + 1);

        var tanks = slots.Select(s => s < 10 ? s : PRNG.Next(s / 10, s % 10 + 1)).ToArray();

        int[] spawns = [0, 1, 2, 3];
        Span<int> spawnsSpan = new Span<int>(spawns);
        PRNG.Shuffle(spawnsSpan);

        PRNG.SetState(a, b);
        return new RngMission(missionNumber, twoPlayer, map, tanks, spawns);
    }
}
