using Microsoft.Xna.Framework;
using TanksRebirth.GameContent.Systems.Coordinates;
using TanksRebirth.GameContent;
using TanksRebirth.GameContent.ID;
using TanksRebirth.GameContent.Systems.LevelSystem;
using TanksRebirth.GameContent.Tanks;
using TanksRebirth.Internals.Common.Utilities;
using TanksRebirth;

namespace OriginalCampaign;

public static class MissionBuilder {
    public const int PLAYER_SLOTS = 4;
    public const int PLAYER_TEAM = TeamID.Blue, ENEMY_TEAM = TeamID.Red;

    // localize at some point?
    public static string NameOf(int mission) => mission == 100 ? "Final Mission" : $"Mission {mission}";

    public static Mission Build(RngMission rngMission, bool grantsLife) {
        var map = OriginalData.Map(rngMission.TwoPlayer, rngMission.Map);

        var blocks = new List<BlockTemplate>();
        var taken = new HashSet<Point>();
        var players = new Point[PLAYER_SLOTS];
        var enemySpawns = new Point[8];
        bool hasPlayer2 = false;

        foreach (var tile in map.MapItems) {
            switch (tile.Stack) {
                case WiiMap.PLAYER_TANK_ID:
                    players[tile.Type] = tile.Position;
                    hasPlayer2 |= tile.Type == 1;
                    taken.Add(tile.Position);
                    break;
                case WiiMap.ENEMY_TANK_ID:
                    enemySpawns[tile.Type] = tile.Position;
                    taken.Add(tile.Position);
                    break;
                default:
                    if (tile.Type < 0) break;
                    blocks.Add(new BlockTemplate { Type = tile.Type, Stack = (byte)Math.Max(tile.Stack, 1), Position = tile.Position });
                    taken.Add(tile.Position);
                    break;
            }
        }

        // players 3 and 4 take free spots next to the ones the map gives. NOT making my own spots cuz fuk dat
        Point Place(Point near) {
            var cell = FreeNear(near, taken);
            taken.Add(cell);
            return cell;
        }
        if (!hasPlayer2) players[1] = Place(players[0]);
        players[2] = Place(players[0]);
        players[3] = Place(players[1]);

        var tanks = new List<TankTemplate>();
        for (int i = 0; i < PLAYER_SLOTS; i++)
            tanks.Add(new TankTemplate { 
                IsPlayer = true, 
                PlayerType = i, 
                Position = players[i], 
                Facing = FacingFor(players[i]), 
                Team = PLAYER_TEAM 
            });

        for (int slot = 0; slot < enemySpawns.Length; slot++) {
            int id = rngMission.Tanks[slot];
            if (id <= 0) continue;
            tanks.Add(new TankTemplate { 
                AIType = id, 
                Position = enemySpawns[slot], 
                Facing = FacingFor(enemySpawns[slot]), 
                Team = ENEMY_TEAM 
            });
        }

        return new Mission([.. tanks], [.. blocks]) { Name = NameOf(rngMission.Mission), GrantsExtraLife = grantsLife };
    }

    static Facing FacingFor(Point pos)
        => WiiMap.GetAutoTankRotation(pos.ToVector2() - new Vector2(MapGrid.STD_WIDTH - 1, MapGrid.STD_HEIGHT - 1) / 2f);

    static readonly Point[] Nearby = [
        new(0, 1), new(1, 0), new(0, -1), new(-1, 0),
        new(1, 1), new(-1, 1), new(1, -1), new(-1, -1),
        new(0, 2), new(2, 0), new(0, -2), new(-2, 0),
    ];

    static Point FreeNear(Point origin, HashSet<Point> taken) {
        foreach (var offset in Nearby) {
            var cell = origin + offset;
            if (cell.X >= 0 && cell.Y >= 0 && MapGrid.Current.TryGetCell(cell.X, cell.Y, out _) && !taken.Contains(cell))
                return cell;
        }
        return origin;
    }
}
