using Daybreak.Common.Features.Rendering;
using System;
using Terraria.Enums;
using Terraria.ID;

namespace WaterMod.Content;

internal class EnchantedSandMaterial : ModItem, IPreRenderedItem {
    public override string Texture => Assets.Images.Content.Starfish.EnchantedSandMaterial.KEY;

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
        var glow = Assets.Images.Content.Starfish.EnchantedSandMaterial_Glow.Asset.Value;

        var sine = (float)Math.Sin(Main.timeForVisualEffects * 0.07f);
        var intensity = 0.5f + (sine * 0.1f); 
    
        var color = Color.Yellow * intensity;
        
        Main.spriteBatch.Draw(sourceTexture, Vector2.Zero, Color.White);
        Main.spriteBatch.Draw(glow, Vector2.Zero, color);
    }
}