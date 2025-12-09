using Daybreak.Common.Features.Rendering;
using System;
using Terraria.Enums;
using Terraria.ID;
using WaterMod.Content.Reefs;

namespace WaterMod.Content.EnchantedSand;

internal class EnchantedSandMaterial : ModItem, IPreRenderedItem {
    public override string Texture => Assets.Textures.Content.Starfish.EnchantedSandMaterial.KEY;

    public override void SetDefaults() {
        Item.maxStack = Item.CommonMaxStack;

        (Item.width, Item.height) = (24, 18);

        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(copper: 50));
    }
    
    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.FallenStar)
            .AddIngredient<CoralsandItem>(4)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

    public void PreRender(Texture2D sourceTexture) {
        var glow = Assets.Textures.Content.Starfish.EnchantedSandMaterial_Glow.Asset.Value;

        var color = Color.Yellow * (float)Math.Sin(Main.timeForVisualEffects * 0.07f);
        
        Main.spriteBatch.Draw(sourceTexture, Vector2.Zero, Color.White);
        Main.spriteBatch.Draw(glow, Vector2.Zero, color);
    }
}