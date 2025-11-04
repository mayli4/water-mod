using Daybreak.Common.Features.Hooks;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using WaterMod.Utilities;

namespace WaterMod.Common.Ambience;

/*
 * TODO:
 * flocks
 * prefabs
 * pattern randomization
 */

internal struct Boid {
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;

    public Color PrimaryColor;
    public Color SecondaryColor; 
    
    public Rectangle Frame;
}

internal struct Flock {
    public int Id;
    public List<Boid> Boids;
}

internal sealed class BoidSystem {
    private static List<Flock> _flocks = new();
    private static int _nextFlockId;

    public ReadOnlySpan<Flock> Flocks => _flocks.ToArray().AsSpan();

    private const float vision = 100f;
    private const float max_force = 0.02f;
    private const float max_velocity = 2f;

    [UsedImplicitly]
    [OnLoad]
    static void InitStuff() {
        _flocks = new List<Flock>();
    }
    
    [UsedImplicitly]
    [OnUnload]
    static void Unload() {
        _flocks.Clear();
        _flocks = null!;
    }
    
    [UsedImplicitly]
    [SubscribesTo<ModSystemHooks.PostUpdateEverything>]
    static void PostUpdateEverything(ModSystemHooks.PostUpdateEverything.Original orig, ModSystem system) {
        orig();
        
        foreach (var flock in _flocks) {
            for (int i = 0; i < flock.Boids.Count; i++) {
                var currentBoid = flock.Boids[i];
                UpdateBoid(ref currentBoid, flock.Boids.ToArray().AsSpan());
                flock.Boids[i] = currentBoid;
            }
        }

        if(Main.keyState.IsKeyDown(Keys.G) && !Main.oldKeyState.IsKeyDown(Keys.G)) {
            Vector2 mouseWorldPosition = Main.MouseWorld;
            SpawnFlockAtPos(mouseWorldPosition);
        }
    }
    
    [UsedImplicitly]
    [SubscribesTo<ModSystemHooks.PostDrawTiles>]
    static void PostDrawTiles(ModSystemHooks.PostDrawTiles.Original orig, ModSystem system) {
        orig();
        if (Main.spriteBatch == null || Main.gameMenu) return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
        
        foreach (var flock in _flocks) {
            foreach (var boid in flock.Boids) {
                DrawBoid(Main.spriteBatch, boid);
            }
        }

        Main.spriteBatch.End();
    }

    public static Vector2 AvoidTiles(Vector2 position, Vector2 velocity, int range) {
        var sum = Vector2.Zero;
        var tilePos = position.ToTileCoordinates();

        const int tileRange = 2;

        for (int i = -tileRange; i < tileRange + 1; i++) {
            for (int j = -tileRange; j < tileRange + 1; j++) {
                if (WorldGen.InWorld(tilePos.X + i, tilePos.Y + j, 10)) {
                    var tile = Framing.GetTileSafely(tilePos.X + i, tilePos.Y + j);
                    var tileCenter = new Vector2((tilePos.X + i) * 16 + 8, (tilePos.Y + j) * 16 + 8);
                    float pdist = Vector2.DistanceSquared(position, tileCenter);
                    
                    bool isSolidOrObstructingLiquid = (tile.HasTile && Main.tileSolid[tile.TileType]) || tile.LiquidAmount is > 0 and < 255;

                    if (pdist < range * range && pdist > 0.001f && isSolidOrObstructingLiquid) {
                        var d = position - tileCenter;
                        var norm = Vector2.Normalize(d);
                        var weight = norm / (pdist / (16f * 16f)); 
                        sum += weight;
                    }
                }
            }
        }

        if (sum != Vector2.Zero) {
            sum = Vector2.Normalize(sum) * max_velocity;
            var acc = sum - velocity;
            return VectorExtensions.Limit(acc, max_force);
        }
        return Vector2.Zero;
    }

    public static Vector2 AvoidHooman(Vector2 position, Vector2 velocity, int range) {
        if (!Main.LocalPlayer.active || Main.LocalPlayer.dead) {
            return Vector2.Zero;
        }

        float pdistSq = Vector2.DistanceSquared(position, Main.LocalPlayer.Center);
        var sum = Vector2.Zero;

        if (pdistSq < range * range && pdistSq > 0.001f) {
            var d = position - Main.LocalPlayer.Center;
            var norm = Vector2.Normalize(d);
            sum += norm;
        }

        if (sum != Vector2.Zero) {
            sum = Vector2.Normalize(sum) * max_velocity;
            var acc = sum - velocity;
            return VectorExtensions.Limit(acc, max_force);
        }
        return Vector2.Zero;
    }

    public static Vector2 Separation(Vector2 currentPosition, Vector2 currentVelocity, Span<Boid> flockBoids, int range) {
        int count = 0;
        var sum = Vector2.Zero;
        float rangeSq = range * range;

        foreach (var otherBoid in flockBoids) {
            if (otherBoid.Position == currentPosition && otherBoid.Velocity == currentVelocity) continue;

            float distSq = Vector2.DistanceSquared(currentPosition, otherBoid.Position);
            if (distSq < rangeSq && distSq > 0.001f) {
                var d = currentPosition - otherBoid.Position;
                var norm = Vector2.Normalize(d);
                var weight = norm / distSq; 
                sum += weight;
                count++;
            }
        }

        if (count > 0) {
            sum /= count;
            sum = Vector2.Normalize(sum) * max_velocity;
            var acc = sum - currentVelocity;
            return VectorExtensions.Limit(acc, max_force);
        }
        return Vector2.Zero;
    }

    public static Vector2 Alignment(Vector2 currentPosition, Vector2 currentVelocity, Span<Boid> flockBoids, int range) {
        int count = 0;
        var sum = Vector2.Zero;
        float rangeSq = range * range;

        foreach (var otherBoid in flockBoids) {
            if (otherBoid.Position == currentPosition && otherBoid.Velocity == currentVelocity) continue;

            float distSq = Vector2.DistanceSquared(currentPosition, otherBoid.Position);
            if (distSq < rangeSq && distSq > 0.001f) {
                sum += otherBoid.Velocity;
                count++;
            }
        }

        if (count > 0) {
            sum /= count;
            if (sum != Vector2.Zero) {
                sum = Vector2.Normalize(sum) * max_velocity;
                var acc = sum - currentVelocity;
                return VectorExtensions.Limit(acc, max_force);
            }
        }
        return Vector2.Zero;
    }

    public static Vector2 Cohesion(Vector2 currentPosition, Vector2 currentVelocity, Span<Boid> flockBoids, int range) {
        int count = 0;
        var sum = Vector2.Zero;
        float rangeSq = range * range;

        foreach (var otherBoid in flockBoids) {
            if (otherBoid.Position == currentPosition && otherBoid.Velocity == currentVelocity) continue;

            float distSq = Vector2.DistanceSquared(currentPosition, otherBoid.Position);
            if (distSq < rangeSq && distSq > 0.001f) {
                sum += otherBoid.Position;
                count++;
            }
        }

        if (count > 0) {
            sum /= count;
            sum -= currentPosition;
            if (sum != Vector2.Zero) {
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

    public static void UpdateBoid(ref Boid boid, Span<Boid> flockBoids) {
        boid.Acceleration += Separation(boid.Position, boid.Velocity, flockBoids, 25) * 1.5f;
        boid.Acceleration += Alignment(boid.Position, boid.Velocity, flockBoids, 50) * 1f;
        boid.Acceleration += Cohesion(boid.Position, boid.Velocity, flockBoids, 50) * 1f;
        boid.Acceleration += AvoidHooman(boid.Position, boid.Velocity, 50) * 4f;
        boid.Acceleration += AvoidTiles(boid.Position, boid.Velocity, 100) * 5f;
        
        ApplyForces(ref boid);
        
        int worldWidth = Main.maxTilesX * 16;
        int worldHeight = Main.maxTilesY * 16;

        if (boid.Position.X < 0) boid.Position.X += worldWidth;
        if (boid.Position.X > worldWidth) boid.Position.X -= worldWidth;
        if (boid.Position.Y < 0) boid.Position.Y += worldHeight;
        if (boid.Position.Y > worldHeight) boid.Position.Y -= worldHeight;
    }

    public static void DrawBoid(SpriteBatch spriteBatch, Boid boid) {
        var texture = TextureAssets.Item[ItemID.DirtBlock].Value;
        
        var effects = boid.Velocity.X > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

        spriteBatch.Draw(texture, boid.Position - Main.screenPosition, null, Color.White, 
            boid.Velocity.ToRotation() + (float)Math.PI, 
            texture.Size() / 2, 
                         1f, 
                         effects, 
                         0f);
    }

    public static void SpawnFlockAtPos(Vector2 spawnPosition, int numBoids = 10) {
        Flock newFlock = new()
        {
            Id = _nextFlockId++,
            Boids = new List<Boid>(numBoids)
        };

        for (int i = 0; i < numBoids; i++) {
            var boid = new Boid {
                Position = spawnPosition + Main.rand.NextVector2Circular(20f, 20f),
                Velocity = Main.rand.NextVector2Circular(max_velocity * 0.5f, max_velocity * 0.5f),
                Acceleration = Vector2.Zero,
            };
            newFlock.Boids.Add(boid);
        }

        _flocks.Add(newFlock);
    }
}