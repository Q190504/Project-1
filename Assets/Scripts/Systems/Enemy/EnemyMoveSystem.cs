using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;

[BurstCompile]
[UpdateAfter(typeof(FlowFieldComputationSystem))]

public partial struct EnemyMoveSystem : ISystem
{
    Entity grid;
    EntityManager entityManager;
    EntityQuery gridQuery;

    public void OnCreate(ref SystemState state)
    {
        gridQuery = state.EntityManager.CreateEntityQuery(typeof(FlowFieldGridDataComponent), typeof(GridNode));
    }

    public void OnUpdate(ref SystemState state)
    {
        if (gridQuery.IsEmpty)
        {
            Debug.Log("Can't find grid in EnemyMoveSystem!");
            return;
        }

        grid = gridQuery.GetSingletonEntity();
        FlowFieldGridDataComponent flowFieldGridDataComponent = state.EntityManager.GetComponentData<FlowFieldGridDataComponent>(grid);
        DynamicBuffer<GridNode> pathBuffer = state.EntityManager.GetBuffer<GridNode>(grid);
        int width = flowFieldGridDataComponent.width;
        float cellSize = flowFieldGridDataComponent.nodeSize;

        foreach (var (localTransform, enemyTag, physicsVelocity, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyTagComponent>, RefRW<PhysicsVelocity>>().WithEntityAccess())
        {
            int x = (int)(localTransform.ValueRO.Position.x / cellSize);
            int y = (int)(localTransform.ValueRO.Position.y / cellSize);
            int index = x + y * width;

            if (index >= 0 && index < pathBuffer.Length)
            {
                float2 flowDirection = pathBuffer[index].vector;

                float3 movement = new float3(flowDirection.x, flowDirection.y, 0) * 2f;

                physicsVelocity.ValueRW.Linear = movement;
            }
        }
    }
}
