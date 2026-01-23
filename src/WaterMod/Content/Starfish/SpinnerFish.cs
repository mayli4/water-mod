using System;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

namespace WaterMod.Content;

public class SpinnerFishItem : ModItem {
    public override string Texture => Assets.Images.Content.Starfish.SpinnerFishItem.KEY;
    
    public override void SetDefaults() {
        base.SetDefaults();

        Item.consumable = false;
        Item.noMelee = true;
        Item.channel = true;
        Item.noUseGraphic = true;

        Item.maxStack = Item.CommonMaxStack;

        Item.DamageType = DamageClass.Melee;
        Item.SetWeaponValues(9, 1f);

        Item.width = 26;
        Item.height = 26;

        Item.shoot = ModContent.ProjectileType<SpinnerFishHeldProjectile>();
        Item.shootSpeed = 8f;
        Item.useTime = Item.useAnimation = 25;
        
        Item.useStyle = ItemUseStyleID.Swing;

        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice());
    }
    
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (player.ItemAnimationJustStarted) {
            if (Item.stack > 0) {
                Item.stack--;
            }
        }
        return true;
    }
    
    public override bool CanUseItem(Player player) {
        return player.ownedProjectileCounts[Item.shoot] <= 0;
    }

    public override void AddRecipes() {
        base.AddRecipes();

        CreateRecipe(50)
           .AddIngredient<EnchantedSandMaterial>()
           .AddTile(TileID.WorkBenches)
           .Register();
    }
}


public class SpinnerFishHeldProjectile : ModProjectile {
    public override string Texture => Assets.Images.Content.Starfish.SpinnerFishHeld.KEY;

    private const int shot_count = 2;
    private bool released;

    public int Charge {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public int ChargeMax => (int)(Main.player[Projectile.owner].itemTimeMax * 3f);
    
    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 4;
    }

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
    }

    public override void AI() {
        var player = Main.player[Projectile.owner];
        var timePerThrow = player.itemAnimationMax / shot_count;
        var shoulderPos = player.MountedCenter + new Vector2(-6 * player.direction, 2);
        
        if (!released) {
            if (!player.channel) {
                released = true;
            }

            if (player.whoAmI == Main.myPlayer) {
                Projectile.velocity = (Vector2.UnitX * Projectile.velocity.Length()).RotatedBy(player.AngleTo(Main.MouseWorld));
                Projectile.netUpdate = true;
            }

            player.ChangeDir(Math.Sign(Projectile.velocity.X));
            player.heldProj = Projectile.whoAmI;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Center = shoulderPos - (Vector2.Normalize(Projectile.velocity) * 14);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + player.AngleTo(Projectile.Center));

            var readyCount = (int)((float)Charge / ChargeMax * shot_count) + 1;
            player.itemAnimation = player.itemTime = timePerThrow * readyCount;
            
            var framesPerShot = ChargeMax / shot_count;
            if (Charge > 0 && Charge < ChargeMax && Charge % framesPerShot == 0) {
                SoundEngine.PlaySound(SoundID.MenuTick with { Pitch = 0.5f }, player.Center);

                for (int i = 0; i < 5; i++) {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.ShimmerSpark);
                    d.velocity *= 2f;
                    d.noGravity = true;
                }
            }

            Charge = Math.Min(Charge + 1, ChargeMax);
        }
        else {
            Projectile.Center = player.Center + (Vector2.Normalize(Projectile.velocity) * 10);
            Projectile.alpha = 255;

            if (((player.itemAnimation + 1) % timePerThrow) == 0) {
                if (player.whoAmI == Main.myPlayer) {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<SpinnerFishProjectile>(), Projectile.damage, Projectile.knockBack, player.whoAmI);
                }

                SoundEngine.PlaySound(SoundID.Item1, player.Center);
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f + Projectile.velocity.ToRotation() - (player.itemAnimation % timePerThrow) * .5f * Projectile.direction);
        }

        if (((player.itemAnimation > 2) || !released) && player is { active: true, dead: false }) {
            Projectile.timeLeft = 2;
        }
    }
    
    public override bool PreDraw(ref Color lightColor) {
        var player = Main.player[Projectile.owner];

        var texture = ModContent.Request<Texture2D>(Texture).Value;
    
        Main.EntitySpriteDraw(
            texture, 
            Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), 
            null, 
            Projectile.GetAlpha(lightColor), 
            Projectile.rotation, 
            texture.Size() / 2, 
            Projectile.scale, 
            SpriteEffects.None
            );

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;
}

public class SpinnerFishProjectile : ModProjectile {
    public override string Texture => Assets.Images.Content.Starfish.SpinnerFishProjectile.KEY;

    public ref float Target => ref Projectile.ai[0];
    public ref float Timer => ref Projectile.ai[1];

    public bool StickingToNpc { get; private set; }
    public bool StickingToTile { get; private set; }

    public bool StickingToAnything => StickingToNpc || StickingToTile;

    private Vector2 offset;

    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.usesLocalNPCImmunity = true;
        Projectile.friendly = true;

        Projectile.width = 10;
        Projectile.height = 10;

        Projectile.aiStyle = -1;

        Projectile.timeLeft = 180;
        Projectile.penetrate = -1;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        base.SendExtraAI(writer);

        writer.Write(StickingToNpc);
        writer.Write(StickingToTile);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        base.ReceiveExtraAI(reader);

        StickingToNpc = reader.ReadBoolean();
        StickingToTile = reader.ReadBoolean();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Projectile.scale = 1.25f;
        Projectile.rotation = (Projectile.Center - target.Center).ToRotation() + MathHelper.PiOver2;
        
        Projectile.frame = 1;

        if (StickingToAnything) {
            return;
        }

        offset = target.Center - Projectile.Center + Projectile.velocity;

        Target = target.whoAmI;
        StickingToNpc = true;

        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (StickingToAnything) {
            return false;
        }
        
        Projectile.scale = 1.25f;
        
        Projectile.frame = 1;
        Projectile.velocity = Vector2.Zero;
        
        var vel = Collision.TileCollision(Projectile.position, oldVelocity, Projectile.width, Projectile.height);

        if (vel.Y != oldVelocity.Y) {
            Projectile.rotation = (oldVelocity.Y > 0) ? 0f : MathHelper.Pi;
        }
        else if (vel.X != oldVelocity.X) {
            Projectile.rotation = (oldVelocity.X > 0) ? -MathHelper.PiOver2 : MathHelper.PiOver2;
        }

        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

        StickingToTile = true;

        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);

        return false;
    }

    public override void AI() {
        base.AI();

        Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.2f);

        if (Projectile.timeLeft < 255 / 25) {
            Projectile.alpha += 25;
        }

        UpdateTargetStick();
        UpdateTileStick();

        if (StickingToAnything) {
            return;
        }

        UpdateGravity();

        Projectile.rotation += Projectile.velocity.X * 0.1f;
    }

    public override bool PreDraw(ref Color lightColor) {
        var tex = ModContent.Request<Texture2D>(Texture).Value;
        var frame = tex.Frame(1, 2, 0, Projectile.frame);
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, 
            Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() / 2, 
            Projectile.scale, SpriteEffects.None);
        return false;
    }

    private void UpdateTargetStick() {
        if (!StickingToNpc) {
            return;
        }

        var target = Main.npc[(int)Target];

        if (!target.active) {
            Projectile.Kill();
            return;
        }

        Projectile.tileCollide = false;

        Projectile.Center = target.Center - offset;
        Projectile.gfxOffY = target.gfxOffY;
    }

    private void UpdateTileStick() {
        if (!StickingToTile) {
            return;
        }

        Projectile.velocity *= 0.5f;
    }

    private void UpdateGravity() {
        if (++Timer >= 20f) 
            Projectile.velocity.Y += 0.6f;
    }
}