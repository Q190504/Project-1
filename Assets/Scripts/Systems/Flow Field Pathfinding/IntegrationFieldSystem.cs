using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(FlowFieldInitializationSystem))]
public partial struct IntegrationFieldSystem : ISystem
{
    private Entity player;

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (grid, pathBuffer) in SystemAPI.Query<RefRO<FlowFieldGridDataComponent>, DynamicBuffer<GridNode>>())
        {
            for (int i = 0; i < pathBuffer.Length; i++)
            {
                GridNode node = pathBuffer.ElementAt(i); // Copy the struct
                node.bestCost = byte.MaxValue;
                node.vector = float2.zero;
                pathBuffer.ElementAt(i) = node;         // Reassign the modified struct
            }

            #region Set Destination

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery playerQuery = entityManager.CreateEntityQuery(typeof(PlayerTagComponent));
            if (playerQuery.CalculateEntityCount() > 0)
                player = playerQuery.GetSingletonEntity();
            float2 playerPosition = SystemAPI.GetComponent<LocalTransform>(player).Position.xy;

            int destinationIndex = GetPlayerPositionNodeIndex(playerPosition, grid);
            GridNode destinationNode = pathBuffer.ElementAt(destinationIndex);  // Copy the struct
            destinationNode.bestCost = 0;                                       //Destination best cost = 0
            destinationNode.vector = float2.zero;
            pathBuffer.ElementAt(destinationIndex) = destinationNode;           // Reassign the modified struct

            #endregion

            #region Set Best Cost

            NativeQueue<int> openList = new NativeQueue<int>(Allocator.Temp);
            openList.Enqueue(destinationIndex);

            while (openList.TryDequeue(out int current))
            {
                int x = current % grid.ValueRO.width;
                int y = current / grid.ValueRO.width;
                byte currentCost = pathBuffer[current].bestCost;

                // 4 directions: Left, Right, Up, Down
                int2[] neighborOffsets = new int2[]
                {
                    new int2(-1,  0), // Left
                    new int2( 1,  0), // Right
                    new int2( 0, -1), // Down
                    new int2( 0,  1), // Up
                };

                for (int i = 0; i < 4; i++) // Loop through 4 directions
                {
                    int2 neighbor = new int2(x, y) + neighborOffsets[i];
                
                    // Skip out-of-bounds neighbors
                    if (neighbor.x < 0 || neighbor.x >= grid.ValueRO.width ||
                        neighbor.y < 0 || neighbor.y >= grid.ValueRO.height)
                        continue;

                    int neighborIndex = neighbor.x + neighbor.y * grid.ValueRO.width;
                    byte cost = pathBuffer[neighborIndex].cost;

                    if (cost == byte.MaxValue) // Skip obstacles
                        continue;

                    byte newCost = (byte)(currentCost + cost);

                    if (newCost < pathBuffer[neighborIndex].bestCost)
                    {
                        GridNode neighborNode = pathBuffer.ElementAt(neighborIndex); // Copy the struct
                        neighborNode.bestCost = newCost;
                        neighborNode.vector = float2.zero;
                        pathBuffer.ElementAt(neighborIndex) = neighborNode;         // Reassign the modified struct
                        openList.Enqueue(neighborIndex);
                    }
                }
            }

            #endregion

            openList.Dispose();
        }
    }

    private int GetPlayerPositionNodeIndex(float2 playerPosition, RefRO<FlowFieldGridDataComponent> grid)
    {
        int x = (int)((playerPosition.x - grid.ValueRO.originPosition.x) / grid.ValueRO.cellSize);
        int y = (int)((playerPosition.y - grid.ValueRO.originPosition.y) / grid.ValueRO.cellSize);

        // Clamp values to prevent out-of-bounds errors
        x = math.clamp(x, 0, grid.ValueRO.width - 1);
        y = math.clamp(y, 0, grid.ValueRO.height - 1);

        return y * grid.ValueRO.width + x;
    }
}
