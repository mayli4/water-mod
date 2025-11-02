using Newtonsoft.Json;
using System.Collections.Generic;

namespace WaterMod.Common.Ambience;

internal struct Boid {
    public Vector2 Position;
    public Vector2 Velocity;
    
    [JsonIgnore]
    public Flock ParentFlock;
    
    public Color BaseColor;
    public Color SecondaryColor;
}

internal struct Flock {
    public int Id;
    public List<Boid> Boids;
}

internal sealed class BoidSystem {
    private List<Flock> Flocks;
}