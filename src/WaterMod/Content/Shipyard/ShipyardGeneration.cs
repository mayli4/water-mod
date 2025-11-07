using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using WaterMod.Utilities;

namespace WaterMod.Content.Shipyard;

internal sealed class ShipyardGenerationSystem : ModSystem {
    /// <summary>
    ///     The unique identifier for the Shipyard's <see cref="PassLegacy" /> added during world
    ///     generation in <see cref="ModifyWorldGenTasks" />.
    /// </summary>
    public const string SHIPYARD_PASS_NAME = $"{nameof(ModImpl)}:{nameof(ShipyardMicroBiome)}";

    public const int SAILBOAT_DISTANCE = 80;

    /// <summary>
    ///     Whether the Sailboat is repaired or not.
    /// </summary>
    public static bool Repaired { get; private set; }

    /// <summary>
    ///     The placement origin of the Shipyard, in tile coordinates.
    /// </summary>
    public static Point ShipyardOrigin { get; private set; }

    /// <summary>
    ///     The placement origin of the Sailboat, in tile coordinates.
    /// </summary>
    public static Point SailboatOrigin { get; private set; }

    /// <summary>
    ///     Callback for when the ship is repaired.
    /// </summary>
    public event Action OnShipRepair;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        base.ModifyWorldGenTasks(tasks, ref totalWeight);

        var index = tasks.FindIndex(static pass => pass.Name == "Final Cleanup");

        if (index == -1)
        {
            return;
        }

        tasks.Insert(index + 1, new PassLegacy(SHIPYARD_PASS_NAME, GenerateShipyard));
    }

    public override void ClearWorld()
    {
        base.ClearWorld();

        Repaired = false;

        ShipyardOrigin = Point.Zero;
        SailboatOrigin = Point.Zero;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        base.SaveWorldData(tag);

        tag["repaired"] = Repaired;

        tag["shipyardOrigin"] = ShipyardOrigin;
        tag["sailboatOrigin"] = SailboatOrigin;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        base.LoadWorldData(tag);

        Repaired = tag.GetBool("repaired");

        ShipyardOrigin = tag.Get<Point>("shipyardOrigin");
        SailboatOrigin = tag.Get<Point>("sailboatOrigin");
    }

    private void GenerateShipyard(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "asd";

        var foundOcean = false;

        var startX = 0;
        var startY = (int)(Main.worldSurface * 0.35f);

        while (!foundOcean)
        {
            var tile = Framing.GetTileSafely(startX, startY);

            if (tile.HasLiquidType(LiquidID.Water) && tile.HasLiquidAmount(byte.MaxValue))
            {
                foundOcean = true;
                break;
            }

            startY++;
        }

        var foundBeach = false;

        while (!foundBeach)
        {
            var tile = Framing.GetTileSafely(startX, startY);

            if (tile.HasTileType(TileID.Sand) && tile.IsSolid())
            {
                foundBeach = true;
                break;
            }

            startX++;
        }

        if (!foundOcean || !foundBeach)
        {
            return;
        }

        var biggestY = startY;

        for (var i = startX; i < startX + 50; i++) {
            for (var j = 0; j < Main.maxTilesY; j++) {
                var tile = Framing.GetTileSafely(i, j);

                if (tile.HasTileType(TileID.Sand) && !tile.HasAnyLiquidAmount() && j < biggestY) {
                    biggestY = j;
                    break;
                }
            }
        }

        ShipyardOrigin = new Point(startX, biggestY);
        SailboatOrigin = new Point(startX - SAILBOAT_DISTANCE, biggestY);

        var shipyard = GenVars.configuration.CreateBiome<ShipyardMicroBiome>();
        var sailboat = GenVars.configuration.CreateBiome<BrokenSailboatMicroBiome>();

        // shipyard.Place(ShipyardOrigin, GenVars.structures);
        // sailboat.Place(SailboatOrigin, GenVars.structures);
    }
}

public sealed class ShipyardMicroBiome : MicroBiome
{
    /// <summary>
    ///     The path to the Shipyard structure file, not qualified by the mod's internal name.
    /// </summary>
    public const string SHIPYARD_ASSET_PATH = "Assets/Structures/Shipyard.shstruct";

    /// <summary>
    ///     The width of each pillar, in tiles.
    /// </summary>
    public const int PILLAR_WIDTH = 2;

    /// <summary>
    ///     The horizontal offset to the first deck pillar, in tiles, relative to the origin.
    /// </summary>
    public const int FIRST_DECK_PILLAR_OFFSET_X = 4;

    /// <summary>
    ///     The horizontal offset to the second deck pillar, in tiles, relative to the origin.
    /// </summary>
    public const int SECOND_DECK_PILLAR_OFFSET_X = 20;

    /// <summary>
    ///     The horizontal offset to the third deck pillar, in tiles, relative to the origin.
    /// </summary>
    public const int THIRD_DECK_PILLAR_OFFSET_X = 36;

    /// <summary>
    ///     The horizontal offset to the first house pillar, in tiles, relative to the origin.
    /// </summary>
    public const int FIRST_HOUSE_PILLAR_OFFSET_X = 56;

    /// <summary>
    ///     The horizontal offset to the second house pillar, in tiles, relative to the origin.
    /// </summary>
    public const int SECOND_HOUSE_PILLAR_OFFSET_X = 74;

    /// <summary>
    ///     The vertical offset to each deck pillar, in tiles, relative to the origin.
    /// </summary>
    public const int DECK_PILLAR_OFFSET_Y = 38;

    /// <summary>
    ///     The vertical offset to each house pillar, in tiles, relative to the origin.
    /// </summary>
    public const int HOUSE_PILLAR_OFFSET_Y = 26;

    /// <summary>
    ///     The horizontal offset to the Sailor's room, in tiles, relative to the origin.
    /// </summary>
    public const int SAILOR_ROOM_OFFSET_X = 60;

    /// <summary>
    ///     The vertical offset to the Sailor's room, in tiles, relative to the origin.
    /// </summary>
    public const int SAILOR_ROOM_OFFSET_Y = 10;

    public override bool Place(Point origin, StructureMap structures)
    {
        var mod = ModContent.GetInstance<ModImpl>();
        var dims = Point16.Zero;

        dims = StructureHelper.API.Generator.GetStructureDimensions(SHIPYARD_ASSET_PATH, mod);

        origin -= new Point(dims.X / 2, dims.Y - dims.Y / 3);

        var canPlaceShipyard = structures.CanPlace(new Rectangle(origin.X, origin.Y, dims.X, dims.Y));

        if (!canPlaceShipyard)
        {
            return false;
        }

        StructureHelper.API.Generator.GenerateStructure(SHIPYARD_ASSET_PATH, new Point16(origin.X, origin.Y), mod);

        structures.AddProtectedStructure(new Rectangle(origin.X, origin.Y, dims.X, dims.Y));

        for (var j = origin.Y + 30; j < origin.Y + dims.Y; j++)
        {
            var offset = WorldGen.genRand.Next(-4, 4);

            var strength = WorldGen.genRand.Next(10, 17);
            var steps = WorldGen.genRand.Next(1, 4);

            WorldGen.TileRunner(origin.X + dims.X + offset, j, strength, steps, TileID.Sand, true);
        }

        for (var i = 0; i < PILLAR_WIDTH; i++) { }
        
        

        // for (var i = 0; i < PILLAR_WIDTH; i++)
        // {
        //     WorldgenUtils.ExtendDownwards(origin.X + FIRST_DECK_PILLAR_OFFSET_X + i, origin.Y + DECK_PILLAR_OFFSET_Y);
        //     WorldgenUtils.ExtendDownwards(origin.X + SECOND_DECK_PILLAR_OFFSET_X + i, origin.Y + DECK_PILLAR_OFFSET_Y);
        //     WorldgenUtils.ExtendDownwards(origin.X + THIRD_DECK_PILLAR_OFFSET_X + i, origin.Y + DECK_PILLAR_OFFSET_Y);
        //     WorldgenUtils.ExtendDownwards(origin.X + FIRST_HOUSE_PILLAR_OFFSET_X + i, origin.Y + HOUSE_PILLAR_OFFSET_Y);
        //     WorldgenUtils.ExtendDownwards(origin.X + SECOND_HOUSE_PILLAR_OFFSET_X + i, origin.Y + HOUSE_PILLAR_OFFSET_Y);
        //
        // }

        var sailorX = (int)((origin.X + SAILOR_ROOM_OFFSET_X) * 16f);
        var sailorY = (int)((origin.Y + SAILOR_ROOM_OFFSET_Y) * 16f);

        //var npc = NPC.NewNPCDirect(new EntitySource_WorldGen(), sailorX, sailorY, ModContent.NPCType<SailorNPC>());

        //npc.UpdateHomeTileState(false, (int)(sailorX / 16f), (int)(sailorY / 16f));

        return true;
    }
}

public sealed class BrokenSailboatMicroBiome : MicroBiome
{
    /// <summary>
    ///     The path to the Broken Sailboat structure file, not qualified by the mod's internal name.
    /// </summary>
    public const string BROKEN_SAILBOAT_ASSET_PATH = "Assets/Structures/BrokenSailboat.shstruct";

    public override bool Place(Point origin, StructureMap structures) {
        var mod = ModContent.GetInstance<ModImpl>();

        var sailboatSize = StructureHelper.API.Generator.GetStructureDimensions(BROKEN_SAILBOAT_ASSET_PATH, mod);

        origin -= new Point(sailboatSize.X / 2, sailboatSize.Y - 10);

        var canPlaceSailboat = structures.CanPlace(new Rectangle(origin.X, origin.Y, sailboatSize.X, sailboatSize.Y));

        if (!canPlaceSailboat)
        {
            return false;
        }

        StructureHelper.API.Generator.GenerateStructure(BROKEN_SAILBOAT_ASSET_PATH, new Point16(origin.X, origin.Y), mod);

        structures.AddProtectedStructure(new Rectangle(origin.X, origin.Y, sailboatSize.X, sailboatSize.Y));

        return true;
    }
}