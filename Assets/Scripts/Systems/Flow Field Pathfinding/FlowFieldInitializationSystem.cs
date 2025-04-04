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

                DynamicBuffer<GridNode> pathBuffer = state.EntityManager.AddBuffer<GridNode>(gridEntity);

                for (int i = 0; i < grid.width * grid.height; i++)
                {
                    pathBuffer.Add(new GridNode
                    {
                        index = i,
                        x = i % grid.width,
                        y = i / grid.width,
                        cost = 1,                       // Default movement cost
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
