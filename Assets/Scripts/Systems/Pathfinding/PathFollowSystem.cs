using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial struct PathFollowSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (localTransform, pathFollowComponent, pathPositionBuffer) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PathFollowComponent>, DynamicBuffer<PathPositionComponent>>())
        {
            if (pathFollowComponent.ValueRO.pathIndex >= 0)
            {
                int2 pathPosition = pathPositionBuffer[pathFollowComponent.ValueRO.pathIndex].position;

                float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
                float3 moveDirection = math.normalizesafe(targetPosition - localTransform.ValueRO.Position);

                float moveSpeed = 3f;

                localTransform.ValueRW.Position += moveDirection * moveSpeed * Time.deltaTime;

                if (math.distance(localTransform.ValueRO.Position, targetPosition) < 0.1f)
                {
                    //next waypoint
                    pathFollowComponent.ValueRW.pathIndex--;
                }
            }
        }
    }
}
