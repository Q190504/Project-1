using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct UnitMoveOrderSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LocalTransform>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (Input.GetMouseButtonDown(0))
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (localTransform, entity) in
                     SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess())
            {
                ecb.AddComponent(entity, new PathFindingComponent
                {
                    startPosition = new int2(0, 0),
                    endPosition = new int2(4, 0),
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
