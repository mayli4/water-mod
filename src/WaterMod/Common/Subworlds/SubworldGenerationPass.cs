using System;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace WaterMod.Common.Subworlds;

internal class SubworldGenerationPass : GenPass {
    private Action<GenerationProgress> method;

    public SubworldGenerationPass(Action<GenerationProgress> method) : base("", 1) {
        this.method = method;
    }

    public SubworldGenerationPass(float weight, Action<GenerationProgress> method) : base("", weight) {
        this.method = method;
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) => method(progress);
}