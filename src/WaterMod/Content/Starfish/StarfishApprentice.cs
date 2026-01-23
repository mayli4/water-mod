using Daybreak.Common.Mathematics;
using Daybreak.Common.Rendering;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using WaterMod.Utilities;

namespace WaterMod.Content;

/* todo
 - recipes
 - set bonus
 */

internal sealed class StarfishApprentice {
    [AutoloadEquip(EquipType.Head)]
    internal class Mask : ModItem {
        public override string Texture => Assets.Images.Content.Starfish.StarfishApprenticeMask.KEY;
        public override string Name => "StarfishApprenticeMask";
        
        public static int HeadSlot { get; private set; }
        
        public override void SetStaticDefaults() {
            HeadSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            
            ArmorIDs.Head.Sets.DrawHead[HeadSlot] = true;
            ArmorIDs.Head.Sets.DrawFullHair[HeadSlot] = true;
        }
        
        public override void SetDefaults() {
            (Item.width, Item.height) = (18, 18);
            
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 6;
        }

        public override void UpdateEquip(Player player) {
            
        }

        public override void AddRecipes() => CreateRecipe().AddIngredient<EnchantedSandMaterial>(15)
                                                           .AddTile(TileID.WorkBenches)
                                                           .Register();
        
        internal class DrawLayer : PlayerDrawLayer {
            public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

            public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
                return drawInfo.drawPlayer.head == HeadSlot;
            }


            protected override void Draw(ref PlayerDrawSet drawInfo) {
                var drawPlayer = drawInfo.drawPlayer;
                var headTexture = Assets.Images.Content.Starfish.StarfishApprenticeMask_HeadLayer.Asset.Value;

                var eyeFrameIndex = (int)drawPlayer.eyeHelper.CurrentEyeFrame;
                var eyelidSource = headTexture.Frame(1, 3, 0, eyeFrameIndex);
                var headPos = drawInfo.HeadPosition(true) - Vector2.UnitY * 2 * drawPlayer.gravDir;

                var origin = drawInfo.headVect;

                origin.X -= 2;

                if (drawPlayer.direction == -1) {
                    origin.X = eyelidSource.Width - origin.X;
                }

                drawInfo.DrawDataCache.Add(new DrawParameters(headTexture) {
                    Position = headPos,
                    Source = eyelidSource,
                    Color = drawInfo.colorArmorHead,
                    Rotation = Angle.FromRadians(drawPlayer.headRotation),
                    Origin = origin,
                    Effects = drawInfo.playerEffect,
                }.ToDrawData() with { shader = drawInfo.cHead });
            }
        }
    }
    
    [AutoloadEquip(EquipType.Head)]
    internal class Helmet : ModItem {
        public override string Texture => Assets.Images.Content.Starfish.StarfishApprenticeHat.KEY;
        public override string Name => "StarfishApprenticeHat";

        public const int MAX_MANA_INCREASE = 20;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MAX_MANA_INCREASE);
        
        public override void SetDefaults() {
            (Item.width, Item.height) = (18, 18);
            
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 6;
        }

        public override void UpdateEquip(Player player) {
            player.statManaMax2 += MAX_MANA_INCREASE;
        }

        public override void AddRecipes() => CreateRecipe().AddIngredient<EnchantedSandMaterial>(15)
                                                           .AddTile(TileID.WorkBenches)
                                                           .Register();
    }
    
    [AutoloadEquip(EquipType.Body)]
    internal class Chestplate : ModItem {
        public override string Texture => Assets.Images.Content.Starfish.StarfishApprenticeChestplate.KEY;
        public override string Name => "StarfishApprenticeChestplate";
        
        public override void SetDefaults() {
            (Item.width, Item.height) = (18, 18);
            
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 6;
        }
    }
    
    [AutoloadEquip(EquipType.Legs)]
    internal class Greaves : ModItem {
        public override string Texture => Assets.Images.Content.Starfish.StarfishApprenticeGreaves.KEY;
        public override string Name => "StarfishApprenticeGreaves";
        
        public override void SetDefaults() {
            (Item.width, Item.height) = (18, 18);
            
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.defense = 6;
        }
    }
}