using System;
using System.Reflection;
using Terraria.Graphics.Renderers;

namespace WaterMod.Common.Rendering;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PoolCapacityAttribute(int size) : Attribute
{
    public int Capacity { get; } = size;
}

public abstract class BaseParticle<T> : IPooledParticle where T : BaseParticle<T>, new()
{
    public const int DEFAULT_POOL_CAPACITY = 200;

    public static ParticlePool<T> Pool { get; } = new(typeof(T).GetCustomAttribute<PoolCapacityAttribute>(inherit: false)?.Capacity ?? DEFAULT_POOL_CAPACITY, GetNewParticle);

    public bool IsRestingInPool { get; private set; }

    public bool ShouldBeRemovedFromRenderer { get; protected set; }

    public virtual void FetchFromPool()
    {
        IsRestingInPool = false;
        ShouldBeRemovedFromRenderer = false;
    }

    public virtual void RestInPool()
    {
        IsRestingInPool = true;
    }

    public virtual void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) { }

    public virtual void Update(ref ParticleRendererSettings settings) { }

    protected static T GetNewParticle() => new();
}