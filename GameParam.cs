using System.Buffers.Binary;

namespace OriginalCampaign;

public sealed class MissionRow {
    public int Number;
    public bool AwardP1;
    // in an attempt to make it a better 2+ player experience...
    public int[] SlotsP1 = new int[8], SlotsP2 = new int[8];
    public int MapMinP1, MapMaxP1, MapMinP2, MapMaxP2;
}

// i can't really find a reason to implement this into rebirth itself.
// this code is largely inspired by blitzkrieg technology
public sealed class GameParam {
    public const int MissionCount = 100;
    const int TANKS_OFFSET = 4, TANK_SIZE = 0xA8, TANK_COUNT = 10;
    const int MISSIONS_OFFSET = TANKS_OFFSET + TANK_SIZE * TANK_COUNT; // 0x694
    const int MISSION_SIZE = 0x58;

    public int Lives;
    public MissionRow[] Missions = new MissionRow[MissionCount];

    public static GameParam Parse(byte[] d) {
        // in case anyone wants to place their own stuff into the mod
        if (d.Length < MISSIONS_OFFSET + MISSION_SIZE * MissionCount)
            throw new InvalidDataException($"TnkGameParam.bin is too short ({d.Length} bytes).");

        int ReadBigInt32(int o) => BinaryPrimitives.ReadInt32BigEndian(d.AsSpan(o, 4));
        var gp = new GameParam { Lives = ReadBigInt32(0) };

        // mission array
        for (int i = 0; i < MissionCount; i++) {
            int b = MISSIONS_OFFSET + i * MISSION_SIZE;
            var row = new MissionRow {
                Number = i + 1,
                AwardP1 = ReadBigInt32(b) != 0,
                // skipping awardp2 cuz useless
                MapMinP1 = ReadBigInt32(b + 0x48), MapMaxP1 = ReadBigInt32(b + 0x4C),
                MapMinP2 = ReadBigInt32(b + 0x50), MapMaxP2 = ReadBigInt32(b + 0x54),
            };

            // tank array
            for (int k = 0; k < 8; k++) {
                row.SlotsP1[k] = ReadBigInt32(b + 8 + 4 * k);
                row.SlotsP2[k] = ReadBigInt32(b + 0x28 + 4 * k);
            }
            gp.Missions[i] = row;
        }
        return gp;
    }
}
