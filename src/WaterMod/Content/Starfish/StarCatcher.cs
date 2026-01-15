using Terraria.Enums;
using Terraria.ID;

namespace WaterMod.Content;

public class StarCatcherItem : ModItem {
    public override string Texture => Assets.Images.Content.Starfish.StarCatcherItem.KEY; 

    public override void SetDefaults() {
        base.SetDefaults();

        Item.noMelee = true;

        Item.fishingPole = 15;

        Item.width = 52;
        Item.height = 44;

        Item.useTime = 8;
        Item.useAnimation = 8;
        Item.UseSound = SoundID.Item1;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.shoot = ModContent.ProjectileType<StarCatcherBobberProjectile>();
        Item.shootSpeed = 10f;

        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice());
    }

    public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor) {
        base.ModifyFishingLine(bobber, ref lineOriginOffset, ref lineColor);
        lineOriginOffset = new Vector2(46, -36);
    }

    public override void HoldItem(Player player) {
        base.HoldItem(player);
        player.accFishingLine = true;
    }

    public override void AddRecipes() {
        base.AddRecipes();

        CreateRecipe()
            .AddIngredient<EnchantedSandMaterial>(3)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public class StarCatcherBobberProjectile : ModProjectile {
    public override string Texture => Assets.Images.Content.Starfish.StarCatcherBobberProjectile.KEY; 
    
    public float Intensity { get; private set; }

    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.netImportant = true;
        Projectile.bobber = true;

        Projectile.width = 14;
        Projectile.height = 14;

        Projectile.aiStyle = ProjAIStyleID.Bobber;
    }

    public override void AI() {
        base.AI();

        if (Projectile.wet) {
            Intensity += 0.1f;
        }
        else {
            Intensity -= 0.1f;
        }
    }

    // TODO: Implement proper visuals.
    public override bool PreDraw(ref Color lightColor) {
        var texture = Assets.Images.Content.Starfish.StarCatcherBobberProjectile_Outline.Asset.Value;
        var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var offsetX = 0;
        var offsetY = 0;
        var originX = (texture.Width - Projectile.width) / 2f + Projectile.width / 2f;

        ProjectileLoader.DrawOffset(Projectile, ref offsetX, ref offsetY, ref originX);

        var x = Projectile.position.X - Main.screenPosition.X + originX + offsetX;
        var y = Projectile.position.Y - Main.screenPosition.Y + Projectile.height / 2f + Projectile.gfxOffY;

        var frame = texture.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
        var origin = new Vector2(originX, Projectile.height / 2f + offsetY);

        Main.EntitySpriteDraw
        (
            texture,
            new Vector2(x, y),
            frame,
            Projectile.GetAlpha(Color.White),
            Projectile.rotation,
            origin,
            Projectile.scale,
            effects
        );

        return true;
    }
}