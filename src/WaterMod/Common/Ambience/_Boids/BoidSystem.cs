using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using WaterMod.Utilities;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Linq;
using AssGen;

namespace WaterMod.Common.Ambience;

/* todo - non-randomized boids, hjson flock prefabs */

[UsedImplicitly]
internal sealed class BoidSystem {
    internal struct Boid {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;

        public Color Color;

        public Rectangle TextureFrame;

        public uint LifeTime;
    }

    private const int bits_per_mask = sizeof(ulong) * 8;
    private static uint _maxBoids;
    private static ulong[] _presenceMask = Array.Empty<ulong>();
    private static Boid[] _boids = Array.Empty<Boid>();

    private const float max_force = 0.02f;
    private const float max_velocity = 2f;

    private static readonly List<FlockData> flock_data = new();

    internal struct FlockData {
        public Texture2D Atlas;
        public int StartBoidIndex;
        public int EndBoidIndex;
    }

    public static uint MaxBoids {
        get => _maxBoids;
        set {
            _maxBoids = Math.Max(64, BitOperations.RoundUpToPowerOf2(value));

            if(_boids.Length < _maxBoids) {
                Array.Resize(ref _boids, (int)_maxBoids);
            }
            
            int requiredMaskLength = (int)((_maxBoids + bits_per_mask - 1) / bits_per_mask);
            if(_presenceMask.Length < requiredMaskLength) {
                Array.Resize(ref _presenceMask, requiredMaskLength);
            }
        }
    }

    [OnLoad]
    [UsedImplicitly]
    static void Initialize() {
        MaxBoids = 1024;
    }

    [OnUnload]
    [UsedImplicitly]
    static void Unload() {
        Array.Clear(_boids);
        Array.Clear(_presenceMask);
        flock_data.Clear();
    }

    [UsedImplicitly]
    [ModSystemHooks.PostUpdateEverything]
    static void PostUpdateEverything() {

        var screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2f;

        const int maxBoidLifeTime = 300 * 60;
        const float maxOffScreenDespawnDistanceSqr = 3000f * 3000f;

        for(int maskIndex = 0; maskIndex < _presenceMask.Length; maskIndex++) {
            ref ulong maskRef = ref _presenceMask[maskIndex];
            ulong maskCopy = maskRef;

            if (maskCopy == 0) continue;

            for (int bitIndex = 0; bitIndex < bits_per_mask; bitIndex++) {
                if ((maskCopy & (1ul << bitIndex)) == 0) continue;

                int globalBoidIndex = (maskIndex * bits_per_mask) + bitIndex;
                if(globalBoidIndex >= _boids.Length) continue;

                ref var currentBoid = ref _boids[globalBoidIndex];

                bool despawn = currentBoid.LifeTime++ >= maxBoidLifeTime || Vector2.DistanceSquared(currentBoid.Position, screenCenter) >= maxOffScreenDespawnDistanceSqr;

                if(despawn) {
                    DeallocateBoid(globalBoidIndex);
                }
            }
        }

        for(int maskIndex = 0; maskIndex < _presenceMask.Length; maskIndex++) {
            ulong maskCopy = _presenceMask[maskIndex];

            if (maskCopy == 0) continue;

            for (int bitIndex = 0; bitIndex < bits_per_mask; bitIndex++)
            {
                if ((maskCopy & (1ul << bitIndex)) == 0) continue;

                int globalBoidIndex = (maskIndex * bits_per_mask) + bitIndex;
                if(globalBoidIndex >= _boids.Length) continue;

                ref Boid boidToUpdate = ref _boids[globalBoidIndex];

                FlockData? currentFlockData = null;
                for(int fIdx = 0; fIdx < flock_data.Count; fIdx++) {
                    if (globalBoidIndex >= flock_data[fIdx].StartBoidIndex && globalBoidIndex < flock_data[fIdx].EndBoidIndex) {
                        currentFlockData = flock_data[fIdx];
                        break;
                    }
                }

                if (currentFlockData.HasValue) {
                    UpdateBoid(ref boidToUpdate, currentFlockData.Value, globalBoidIndex);
                }
            }
        }


        for(int i = flock_data.Count - 1; i >= 0; i--) {
            var flockData = flock_data[i];
            bool hasActiveBoids = false;
            for(int boidIdx = flockData.StartBoidIndex; boidIdx < flockData.EndBoidIndex; boidIdx++) {
                if(IsBoidActive(boidIdx)) {
                    hasActiveBoids = true;
                    break;
                }
            }
            if(!hasActiveBoids) {
                flock_data.RemoveAt(i);
            }
        }
        if(Main.keyState.IsKeyDown(Keys.G) && !Main.oldKeyState.IsKeyDown(Keys.G)) {
            Vector2 mouseWorldPosition = Main.MouseWorld;
            SpawnFlockAtPos(mouseWorldPosition, WaterMod.Core.AssetReferences.Assets.Textures.Boids.GenericBoids.Asset.Value);
        }
    }

    [UsedImplicitly]
    [ModSystemHooks.PostDrawTiles]
    static void PostDrawTiles() {
        if(Main.spriteBatch == null || Main.gameMenu) return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

        for(int maskIndex = 0; maskIndex < _presenceMask.Length; maskIndex++) {
            ulong maskCopy = _presenceMask[maskIndex];

            if (maskCopy == 0) continue;

            for (int bitIndex = 0; bitIndex < bits_per_mask; bitIndex++)
            {
                if ((maskCopy & (1ul << bitIndex)) == 0) continue;

                int globalBoidIndex = (maskIndex * bits_per_mask) + bitIndex;
                if(globalBoidIndex >= _boids.Length) continue;

                ref var boid = ref _boids[globalBoidIndex];

                Texture2D? currentFlockTextureAtlas = null;
                foreach(var data in flock_data) {
                    if(globalBoidIndex >= data.StartBoidIndex && globalBoidIndex < data.EndBoidIndex) {
                        currentFlockTextureAtlas = data.Atlas;
                        break;
                    }
                }

                if(currentFlockTextureAtlas != null) {
                    DrawBoid(Main.spriteBatch, boid, currentFlockTextureAtlas);
                }
            }
        }

        Main.spriteBatch.End();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsBoidActive(int globalBoidIndex) {
        if(globalBoidIndex < 0 || globalBoidIndex >= MaxBoids) {
            return false;
        }
        int maskIndex = globalBoidIndex / bits_per_mask;
        int bitIndex = globalBoidIndex % bits_per_mask;
        return (_presenceMask[maskIndex] & (1ul << bitIndex)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DeallocateBoid(int globalBoidIndex) {
        if(globalBoidIndex < 0 || globalBoidIndex >= MaxBoids) {
            return;
        }
        int maskIndex = globalBoidIndex / bits_per_mask;
        int bitIndex = globalBoidIndex % bits_per_mask;
        _presenceMask[maskIndex] &= ~(1ul << bitIndex);
        _boids[globalBoidIndex] = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AllocateBoidIndex() {
        int maskIndex;

        for(maskIndex = 0; maskIndex < _presenceMask.Length; maskIndex++) {
            int bitIndex = BitOperations.TrailingZeroCount(~_presenceMask[maskIndex]);

            if(bitIndex < bits_per_mask) {
                int index = (maskIndex * bits_per_mask) + bitIndex;
                if(index < MaxBoids) {
                    _presenceMask[maskIndex] |= (1ul << bitIndex);
                    return index;
                }
            }
        }

        return -1;
    }

    public static Vector2 AvoidTiles(Vector2 position, Vector2 velocity, int range) {
        var sum = Vector2.Zero;
        var tilePos = position.ToTileCoordinates();
        float rangeSq = range * range;

        const int tileRange = 2;

        for(int i = -tileRange; i < tileRange + 1; i++) {
            for(int j = -tileRange; j < tileRange + 1; j++) {
                int checkX = tilePos.X + i;
                int checkY = tilePos.Y + j;

                if(WorldGen.InWorld(checkX, checkY, 10)) {
                    var tile = Framing.GetTileSafely(checkX, checkY);
                    var tileCenter = new Vector2(checkX * 16 + 8, checkY * 16 + 8);
                    float pdistSq = Vector2.DistanceSquared(position, tileCenter);

                    bool isObstacle = false;

                    if(tile.HasTile && Main.tileSolid[tile.TileType]) {
                        isObstacle = true;
                    } else if(tile.LiquidAmount > 0) {
                        if(tile.LiquidAmount < 255 || tile.LiquidType != LiquidID.Water) {
                            isObstacle = true;
                        }
                    } else if(tile is { HasTile: false, LiquidAmount: 0 }) {
                        isObstacle = true;
                    }


                    if(!(pdistSq < rangeSq) || !(pdistSq > 0.001f) || !isObstacle) {
                        continue;
                    }

                    var d = position - tileCenter;
                    var norm = Vector2.Normalize(d);
                    var weight = norm / (pdistSq / (16f * 16f));
                    sum += weight;
                }
            }
        }

        if(sum != Vector2.Zero) {
            sum = Vector2.Normalize(sum) * max_velocity;
            var acc = sum - velocity;
            return VectorExtensions.Limit(acc, max_force);
        }
        return Vector2.Zero;
    }

    public static Vector2 AvoidPlayers(Vector2 position, Vector2 velocity, int range) {
        if(!Main.LocalPlayer.active || Main.LocalPlayer.dead) {
            return Vector2.Zero;
        }

        float pdistSq = Vector2.DistanceSquared(position, Main.LocalPlayer.Center);
        float rangeSq = range * range;

        var sum = Vector2.Zero;

        if(pdistSq < rangeSq && pdistSq > 0.001f) {
            var d = position - Main.LocalPlayer.Center;
            var norm = Vector2.Normalize(d);
            sum += norm;
        }

        if(sum != Vector2.Zero) {
            sum = Vector2.Normalize(sum) * max_velocity;
            var acc = sum - velocity;
            return VectorExtensions.Limit(acc, max_force);
        }
        return Vector2.Zero;
    }

    public static Vector2 Separation(Vector2 currentPosition, Vector2 currentVelocity, in FlockData flockInfo, int globalCurrentBoidIndex, int range) {
        int count = 0;
        var sum = Vector2.Zero;
        float rangeSq = range * range;

        for (int i = flockInfo.StartBoidIndex; i < flockInfo.EndBoidIndex; i++) {
            if (i == globalCurrentBoidIndex) continue;
            if (!IsBoidActive(i)) continue;

            ref Boid otherBoid = ref _boids[i];
            float distSq = Vector2.DistanceSquared(currentPosition, otherBoid.Position);
            if(distSq < rangeSq && distSq > 0.001f) {
                var d = currentPosition - otherBoid.Position;
                var norm = Vector2.Normalize(d);
                var weight = norm / distSq;
                sum += weight;
                count++;
            }
        }

        if(count > 0) {
            sum /= count;
            sum = Vector2.Normalize(sum) * max_velocity;
            var acc = sum - currentVelocity;
            return VectorExtensions.Limit(acc, max_force);
        }
        return Vector2.Zero;
    }

    public static Vector2 Alignment(Vector2 currentPosition, Vector2 currentVelocity, in FlockData flockInfo, int globalCurrentBoidIndex, int range) {
        int count = 0;
        var sum = Vector2.Zero;
        float rangeSq = range * range;

        for (int i = flockInfo.StartBoidIndex; i < flockInfo.EndBoidIndex; i++) {
            if (i == globalCurrentBoidIndex) continue;
            if (!IsBoidActive(i)) continue;

            ref Boid otherBoid = ref _boids[i];

            float distSq = Vector2.DistanceSquared(currentPosition, otherBoid.Position);
            if(distSq < rangeSq && distSq > 0.001f) {
                sum += otherBoid.Velocity;
                count++;
            }
        }

        if(count > 0) {
            sum /= count;
            if(sum != Vector2.Zero) {
                sum = Vector2.Normalize(sum) * max_velocity;
                var acc = sum - currentVelocity;
                return VectorExtensions.Limit(acc, max_force);
            }
        }
        return Vector2.Zero;
    }

    public static Vector2 Cohesion(Vector2 currentPosition, Vector2 currentVelocity, in FlockData flockInfo, int globalCurrentBoidIndex, int range) {
        int count = 0;
        var sum = Vector2.Zero;
        float rangeSq = range * range;

        for (int i = flockInfo.StartBoidIndex; i < flockInfo.EndBoidIndex; i++) {
            if (i == globalCurrentBoidIndex) continue;
            if (!IsBoidActive(i)) continue;

            ref Boid otherBoid = ref _boids[i];

            float distSq = Vector2.DistanceSquared(currentPosition, otherBoid.Position);
            if(distSq < rangeSq && distSq > 0.001f) {
                sum += otherBoid.Position;
                count++;
            }
        }

        if(count > 0) {
            sum /= count;
            sum -= currentPosition;
            if(sum != Vector2.Zero) {
                sum = Vector2.Normalize(sum) * max_velocity;
                var acc = sum - currentVelocity;
                return VectorExtensions.Limit(acc, max_force);
            }
        }
        return Vector2.Zero;
    }

    public static void ApplyForces(ref Boid boid) {
        boid.Velocity += boid.Acceleration;
        boid.Velocity = VectorExtensions.Limit(boid.Velocity, max_velocity);
        boid.Position += boid.Velocity;
        boid.Acceleration = Vector2.Zero;
    }

    public static void UpdateBoid(ref Boid boid, in FlockData flockInfo, int globalBoidIndex) {
        boid.Acceleration += Separation(boid.Position, boid.Velocity, flockInfo, globalBoidIndex, 25) * 1.5f;
        boid.Acceleration += Alignment(boid.Position, boid.Velocity, flockInfo, globalBoidIndex, 50) * 1f;
        boid.Acceleration += Cohesion(boid.Position, boid.Velocity, flockInfo, globalBoidIndex, 50) * 1f;
        boid.Acceleration += AvoidPlayers(boid.Position, boid.Velocity, 50) * 4f;
        boid.Acceleration += AvoidTiles(boid.Position, boid.Velocity, 100) * 5f;

        ApplyForces(ref boid);

        if(Filters.Scene["WaterDistortion"].GetShader() is not WaterShaderData data)
            return;

        data.QueueRipple(boid.Position, 0.5f, RippleShape.Square, MathHelper.PiOver4);

        int worldWidth = Main.maxTilesX * 16;
        int worldHeight = Main.maxTilesY * 16;

        if(boid.Position.X < 0) boid.Position.X += worldWidth;
        if(boid.Position.X > worldWidth) boid.Position.X -= worldWidth;
        if(boid.Position.Y < 0) boid.Position.Y += worldHeight;
        if(boid.Position.Y > worldHeight) boid.Position.Y -= worldHeight;
    }

    public static void DrawBoid(SpriteBatch spriteBatch, Boid boid, Texture2D texAtlas) {
        var lightColor = Lighting.GetColor(boid.Position.ToTileCoordinates());

        var finalColor = new Color(
            (byte)(boid.Color.R * (lightColor.R / 255f)),
            (byte)(boid.Color.G * (lightColor.G / 255f)),
            (byte)(boid.Color.B * (lightColor.B / 255f)),
            boid.Color.A
        );

        var source = boid.TextureFrame;
        var effects = boid.Velocity.X > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

        spriteBatch.Draw(texAtlas, boid.Position - Main.screenPosition, source, finalColor,
            boid.Velocity.ToRotation() + (float)Math.PI, source.Size() / 2, 1, effects, 0f);
    }

    public static void SpawnFlockAtPos(Vector2 spawnPosition, Texture2D flockTextureAtlas, int numBoids = 10) {
        if(numBoids <= 0) {
            return;
        }

        int currentActiveBoids = _presenceMask.Sum(BitOperations.PopCount);

        if(currentActiveBoids + numBoids > MaxBoids && MaxBoids > 0) {
            return;
        }

        int minBoidIndex = int.MaxValue;
        int maxBoidIndex = int.MinValue;
        int allocatedCount = 0;

        for(int i = 0; i < numBoids; i++) {
            int boidGlobalIndex = AllocateBoidIndex();

            if(boidGlobalIndex == -1) {
                break;
            }

            if(boidGlobalIndex < minBoidIndex) minBoidIndex = boidGlobalIndex;
            if(boidGlobalIndex > maxBoidIndex) maxBoidIndex = boidGlobalIndex;
            allocatedCount++;

            var fishTemplates = new Rectangle[] {
                new (0, 0, 22, 24),
                new (24, 0, 22, 24),
                new (48, 0, 22, 24),
                new (0, 26, 22, 24),
                new (24, 26, 22, 24),
                new (48, 26, 22, 24),
            };

            var rect = fishTemplates[Main.rand.Next(fishTemplates.Length)];

            var randomColor = new Color(
                Main.rand.Next(150, 256),
                Main.rand.Next(150, 256),
                Main.rand.Next(150, 256)
            );

            _boids[boidGlobalIndex] = new()
            {
                Position = spawnPosition + Main.rand.NextVector2Circular(20f, 20f),
                Velocity = Main.rand.NextVector2Circular(max_velocity * 0.5f, max_velocity * 0.5f),
                Acceleration = Vector2.Zero,
                Color = randomColor,
                TextureFrame = rect,
                LifeTime = 0
            };
        }

        if(allocatedCount == 0) {
            return;
        }

        FlockData newFlock = new()
        {
            Atlas = flockTextureAtlas,
            StartBoidIndex = minBoidIndex,
            EndBoidIndex = maxBoidIndex + 1
        };
        flock_data.Add(newFlock);
    }
}