using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;

public partial struct EnemyMoveOrderSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (enemy, localTransform, entity) in
                     SystemAPI.Query<RefRW<EnemyComponentTag>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                MapManager.Instance.pathfindingGrid.GetXY(localTransform.ValueRO.Position, out int startX, out int startY);

                ValidatePosition(ref startX, ref startY);

                ecb.AddComponent(entity, new PathFindingComponent
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(4, 0),
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
