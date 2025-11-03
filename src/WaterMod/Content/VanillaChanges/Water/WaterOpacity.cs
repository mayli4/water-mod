using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using System;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;

namespace WaterMod.Content.VanillaChanges;

internal sealed class WaterAlpha {
	private static bool _isLiquid;

    [OnLoad]
    [UsedImplicitly]
	static void DoPatches() {
		On_LiquidRenderer.DrawNormalLiquids += CheckLiquid;
		On_TileDrawing.DrawPartialLiquid += On_TileDrawing_DrawPartialLiquid;
		On_Lighting.GetCornerColors += ModifyWater;
	}

	private static void CheckLiquid(On_LiquidRenderer.orig_DrawNormalLiquids orig, LiquidRenderer self, SpriteBatch batch, Vector2 off, int style, float alpha, bool bg) {
		_isLiquid = true;
		orig(self, batch, off, style, alpha, bg);
		_isLiquid = false;
	}

	private static void On_TileDrawing_DrawPartialLiquid(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors) {
		ModifyColors((int)position.X, (int)position.Y, ref colors, true);
		orig(self, behindBlocks, tileCache, ref position, ref liquidSize, liquidType, ref colors);
	}

	private static void ModifyWater(On_Lighting.orig_GetCornerColors orig, int centerX, int centerY, out VertexColors vertices, float scale) {
		orig(centerX, centerY, out vertices, scale);

		if (_isLiquid)
			ModifyColors(centerX, centerY, ref vertices);
	}

	private static void ModifyColors(int x, int y, ref VertexColors colors, bool isPartial = false) {
		if (!Main.LocalPlayer.ZoneBeach)
			return;

		Clamp(ref colors.TopLeftColor, x, y);
		Clamp(ref colors.TopRightColor, x, y);
		Clamp(ref colors.BottomLeftColor, x, y);
		Clamp(ref colors.BottomRightColor, x, y);

		void Clamp(ref Color color, int x, int y) => color.A = (byte)Math.Min(color.A, 160f);
	}
}