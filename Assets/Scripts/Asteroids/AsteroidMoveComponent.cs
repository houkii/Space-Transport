using Unity.Entities;

public struct AsteroidMoveComponent : IComponentData
{
    public Unity.Mathematics.float3 rotationSpeeds;
    public Unity.Mathematics.float2 origin;
    public Unity.Mathematics.float2 current;
}
