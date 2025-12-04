using JetBrains.Annotations;
using Terraria.ID;
using WaterMod.Common.Swimming;

namespace WaterMod.Content.VanillaChanges.Swimming;

[UsedImplicitly]
internal sealed class FlipperGlobalItem : GlobalItem {
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.Flipper;

    public override void UpdateEquip(Item item, Player player) {
        base.UpdateEquip(item, player);

        player.accFlipper = false;

        if(!player.TryGetModPlayer(out SwimmingPlayer swimmingPlayer)) {
            return;
        }

        swimmingPlayer.GetMovementSpeed() += 1.2f;
        swimmingPlayer.GetMovementAcceleration() += 1.2f;
    }
}