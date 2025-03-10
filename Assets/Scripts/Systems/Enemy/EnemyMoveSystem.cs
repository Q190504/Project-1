using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;
using Unity.Burst;

[BurstCompile]
public partial struct EnemyMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        foreach (var (enemyTag, localTransform, target, entity) in
                 SystemAPI.Query<RefRW<EnemyTagComponent>, RefRO<LocalTransform>, RefRO<EnemyTargetComponent>>().WithEntityAccess())
        {
            MapManager.Instance.pathfindingGrid.GetXY(localTransform.ValueRO.Position, out int startX, out int startY);
            ValidatePosition(ref startX, ref startY);

            float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity).Position;

            int playerXPosition = (int)playerPosition.x;
            int playerYPosition = (int)playerPosition.y;
            ValidatePosition(ref playerXPosition, ref playerYPosition);

            ecb.AddComponent(entity, new PathFindingComponent
            {
                startPosition = new int2(startX, startY),
                endPosition = new int2(playerXPosition, playerYPosition),
            });
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void ValidatePosition(ref int x, ref int y)
    {
        x = math.clamp(x, 0, MapManager.Instance.pathfindingGrid.GetWidth() - 1);
        y = math.clamp(y, 0, MapManager.Instance.pathfindingGrid.GetHeight() - 1);
    }
}
