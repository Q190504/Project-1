using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct FlowFieldInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Track Initialization Progress
        if (SystemAPI.TryGetSingleton<InitializationTrackerComponent>(out var tracker))
        {
            if (!tracker.flowFieldSystemInitialized)
            {
                var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

                EntityQuery query = SystemAPI.QueryBuilder()
                                      .WithAll<FlowFieldGridDataComponent>()
                                      .Build();
                if (query.IsEmpty)
                {
                    Debug.Log("Cant find FlowFieldGridDataComponent in FlowFieldSystem");
                    return;
                }

                Entity gridEntity = query.GetSingletonEntity();
                FlowFieldGridDataComponent grid = SystemAPI.GetComponent<FlowFieldGridDataComponent>(gridEntity);
                float nodeSize = grid.nodeSize;
                float nodeRadius = grid.nodeSize / 2;
                DynamicBuffer<GridNode> pathBuffer = state.EntityManager.AddBuffer<GridNode>(gridEntity);

                for (int i = 0; i < grid.width * grid.height; i++)
                {
                    int x = i % grid.width;
                    int y = i / grid.width;

                    float3 nodeWorldPos = new float3(nodeSize * x + nodeRadius, nodeSize * y + nodeRadius, 0);
                    byte nodeCost = CostFieldHelper.instance.EvaluateCost(nodeWorldPos, nodeSize);

                    pathBuffer.Add(new GridNode
                    {
                        index = i,
                        x = x,
                        y = y,
                        cost = nodeCost,                // Default movement cost
                        bestCost = byte.MaxValue,       // Uninitialized integration field
                        vector = float2.zero            // No flow direction yet
                    });
                }

                // Update tracker
                tracker.flowFieldSystemInitialized = true;
                ecb.SetComponent(SystemAPI.GetSingletonEntity<InitializationTrackerComponent>(), tracker);
            }
        }
    }
}
