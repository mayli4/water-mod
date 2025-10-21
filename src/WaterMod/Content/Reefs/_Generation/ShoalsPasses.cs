using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using WaterMod.Utilities;

namespace WaterMod.Content.Reefs;

internal sealed class InitialShoalsSurfacePass : GenPass {
    public InitialShoalsSurfacePass(string name, double loadWeight) : base(name, loadWeight) {}
    
    public override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = "Shoveling shoals...";
    }
}