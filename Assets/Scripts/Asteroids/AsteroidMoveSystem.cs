using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;


public class AsteroidMoveSystem : JobComponentSystem
{
    private struct AsteroidMoveJob : IJobForEachWithEntity<Translation, Rotation, AsteroidMoveComponent>
    {
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float UnscaledTime;

        public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation, ref AsteroidMoveComponent moveData)
        {
            var entityPos = new Vector2(moveData.current.x, moveData.current.y);
            var newPosXY = Utils.GetRotatedPosition(entityPos, translation.Value.z / 50 * DeltaTime);
            moveData.current = new Unity.Mathematics.float2(newPosXY.x, newPosXY.y);
            translation.Value = new Unity.Mathematics.float3(newPosXY.x + moveData.origin.x, newPosXY.y + moveData.origin.y, translation.Value.z);

            rotation.Value = Unity.Mathematics.quaternion.Euler(
                moveData.rotationSpeeds.x * UnscaledTime,
                moveData.rotationSpeeds.y * UnscaledTime,
                0
            );
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var asteroidMoveJob = new AsteroidMoveJob()
        {
            DeltaTime = Time.deltaTime,
            UnscaledTime = Time.unscaledTime
        };

        return asteroidMoveJob.Schedule(this, inputDeps);
    }
}