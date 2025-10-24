using Terraria.ID;

namespace WaterMod.Content.Reefs;

internal sealed class CoralsandTile : ModTile {
    public override string Texture => Assets.Textures.Tiles.Reefs.Shoals.CoralsandTile.KEY;
    
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        Main.tileMergeDirt[Type] = false;
        Main.tileSolid[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileBlockLight[Type] = true;

        TileID.Sets.Conversion.Sand[Type] = true;

        AddMapEntry(new Color(179, 116, 65));

        HitSound = SoundID.Dig;
        DustType = DustID.Sand;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) {
        base.NumDust(i, j, fail, ref num);

        num = fail ? 1 : 3;
    }
}

internal sealed class CoralsandItem : ModItem {
    public override string Texture => Assets.Textures.Tiles.Reefs.Shoals.CoralsandItem.KEY;

    public override void SetDefaults() {
        base.SetDefaults();

        Item.DefaultToPlaceableTile(ModContent.TileType<CoralsandTile>());

        Item.width = 16;
        Item.height = 16;
    }
}