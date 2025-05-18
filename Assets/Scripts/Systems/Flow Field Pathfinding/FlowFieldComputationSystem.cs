using System.Numerics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(IntegrationFieldSystem))]

public partial struct FlowFieldComputationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!GameManager.Instance.GetGameState()) return;

        foreach (var (grid, pathBuffer) in SystemAPI.Query<RefRO<FlowFieldGridDataComponent>, DynamicBuffer<GridNode>>())
        {
            for (int i = 0; i < pathBuffer.Length; i++)
            {
                GridNode node = pathBuffer.ElementAt(i); // Copy the struct
                node.cost = pathBuffer[i].cost;
                node.bestCost = pathBuffer[i].bestCost;
                node.vector = float2.zero;              // Unassigned direction
                pathBuffer.ElementAt(i) = node;         // Reassign the modified struct
            }

            #region Calculate Direction

            for (int i = 0; i < grid.ValueRO.width * grid.ValueRO.height; i++)
            {
                int x = i % grid.ValueRO.width;
                int y = i / grid.ValueRO.width;
                int lowestCost = pathBuffer[i].bestCost;
                float2 bestDirection = float2.zero;

                // 8 directions: Left, Right, Up, Down, and Diagonals
                int2[] neighborOffsets = new int2[]
                {
                    new int2(-1,  0), // Left
                    new int2( 1,  0), // Right
                    new int2( 0, -1), // Down
                    new int2( 0,  1), // Up
                    new int2(-1, -1), // Bottom-left
                    new int2( 1, -1), // Bottom-right
                    new int2(-1,  1), // Top-left
                    new int2( 1,  1)  // Top-right
                };

                for (int j = 0; j < 8; j++)
                {
                    int2 neighbor = new int2(x, y) + neighborOffsets[j];

                    // Skip out-of-bounds neighbors
                    if (neighbor.x < 0 || neighbor.x >= grid.ValueRO.width ||
                        neighbor.y < 0 || neighbor.y >= grid.ValueRO.height)
                        continue;

                    int neighborIndex = neighbor.x + neighbor.y * grid.ValueRO.width;
                    int neighborCost = pathBuffer[neighborIndex].bestCost;

                    if (neighborCost < lowestCost)
                    {
                        lowestCost = neighborCost;
                        bestDirection = math.normalize(new float2(neighborOffsets[j].x, neighborOffsets[j].y));
                    }
                }

                GridNode node = pathBuffer.ElementAt(i); // Copy the struct
                node.cost = pathBuffer[i].cost;
                node.bestCost = pathBuffer[i].bestCost;
                node.vector = bestDirection;            //Set best direction
                pathBuffer.ElementAt(i) = node;         // Reassign the modified struct

                #endregion
            }
        }
    }
}
