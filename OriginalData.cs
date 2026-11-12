using TanksRebirth.Internals.Common.Utilities;

namespace OriginalCampaign;

#pragma warning disable
public static class OriginalData {
    public const int MapCount = 30;
    public static string DataFolder = "Data";
    static GameParam? _param;
    static readonly WiiMap?[,] _maps = new WiiMap?[2, MapCount];

    public static GameParam Param => _param ??= GameParam.Parse(File.ReadAllBytes(Path.Combine(DataFolder, "TnkGameParam.bin")));

    public static WiiMap Map(bool twoPlayer, int id) {
        if (id < 0 || id >= MapCount) throw new ArgumentOutOfRangeException(nameof(id), id, "map id");
        int set = twoPlayer ? 1 : 0;
        return _maps[set, id] ??= new WiiMap(Path.Combine(DataFolder, "Maps", $"P{set + 1}_{id:00}.bin"));
    }
}
