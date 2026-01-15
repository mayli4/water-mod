using System;
using Terraria.Enums;
using Terraria.ID;
using WaterMod.Common.Rendering;
using WaterMod.Content.Particles;

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
        lineColor = Color.Gold;

        if (bobber.ai[1] < 0) {
            var sine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            lineColor = Color.Lerp(Color.Cyan, Color.Gold, sine);
        }
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

    private bool wasBiting;

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

        Intensity = MathHelper.Clamp(Intensity + (Projectile.wet ? 0.1f : -0.1f), 0f, 1f);

        var isBiting = Projectile.ai[1] < 0;

        if (isBiting && !wasBiting) {
            for (var i = 0; i < 15; i++) {

                var color = Main.rand.NextBool(2) ? Color.Cyan : Color.HotPink;

                var speed = Main.rand.NextVector2Circular(6f, 6f);
                var p = GenericSmallSparkle.RequestNew(
                    Projectile.Center + new Vector2(10, 10),
                    speed,
                    color,
                    Main.rand.NextFloat(0.8f, 1.5f),
                    30,
                    0.3f
                );
                ParticleEngine.PARTICLES.Add(p);
            }
        }
        wasBiting = isBiting;

        if (!Main.rand.NextBool(50)) return;

        if (isBiting) {
            var speed = Main.rand.NextVector2Circular(3f, 3f);
            var p = GenericSmallSparkle.RequestNew(
                Projectile.Center + new Vector2(10, 10),
                speed,
                Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat()),
                1.4f,
                20
            );
            ParticleEngine.PARTICLES.Add(p);
        }
        else {
            var speed = Main.rand.NextVector2Circular(3f, 3f);
            var p = GenericSmallSparkle.RequestNew(
                Projectile.Center + new Vector2(10, 10),
                speed,
                Color.Goldenrod,
                1.4f,
                20
            );
            ParticleEngine.PARTICLES.Add(p);
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

        var drawColor = Color.Goldenrod;
        if (Projectile.ai[1] < 0) {
            var sine = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            drawColor = Color.Lerp(Color.Cyan, Color.White, sine);
        }

        Main.EntitySpriteDraw
        (
            texture,
            new Vector2(x, y),
            frame,
            Projectile.GetAlpha(drawColor) * Intensity,
            Projectile.rotation,
            origin,
            Projectile.scale,
            effects
        );

        return true;
    }
}