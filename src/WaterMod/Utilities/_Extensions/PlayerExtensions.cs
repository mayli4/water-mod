using System.Runtime.CompilerServices;

namespace WaterMod.Utilities;

internal static class PlayerExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUnderwater(this Player player, bool includeSlopes = false) {
        return Collision.DrownCollision(player.position, player.width, player.height, player.gravDir, includeSlopes);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGrounded(this Player player) {
        return player.velocity.Y == 0f;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMounted(this Player player) {
        return player.mount.Active;
    }
}