using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FlowFieldComputationSystem))]
public partial struct FlowFieldDebugSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityQuery gridQuery = state.EntityManager.CreateEntityQuery(typeof(FlowFieldGridDataComponent), typeof(GridNode));

        if (gridQuery.IsEmpty)
            return;

        Entity gridEntity = gridQuery.GetSingletonEntity();
        FlowFieldGridDataComponent gridData = state.EntityManager.GetComponentData<FlowFieldGridDataComponent>(gridEntity);

        if (!gridData.showDebug)
            return;

        DynamicBuffer<GridNode> pathBuffer = state.EntityManager.GetBuffer<GridNode>(gridEntity);

        float cellSize = gridData.cellSize;
        float3 gridOrigin = gridData.originPosition;

        for (int i = 0; i < pathBuffer.Length; i++)
        {
            GridNode node = pathBuffer[i];

            int x = i % gridData.width;
            int y = i / gridData.width;

            // Calculate the center position of the cell
            float3 cellCenter = new float3(gridOrigin.x + (x + 0.5f) * cellSize, gridOrigin.y + (y + 0.5f) * cellSize, 0);

            // Main arrow direction
            float3 direction = math.normalize(new float3(node.vector.x, node.vector.y, 0)) * (cellSize * 0.4f);
            float3 arrowEnd = cellCenter + direction;

            Debug.DrawLine(cellCenter, arrowEnd, Color.green);

            // Draw arrowhead only if there is a valid direction
            if (!math.all(node.vector == float2.zero))
            {
                float3 arrowHeadSize = math.normalize(direction) * (cellSize * 0.2f); // Adjust arrowhead size

                // Rotate arrowhead lines ±135 degrees
                float3 leftHead = arrowEnd + math.mul(quaternion.RotateZ(math.radians(135)), arrowHeadSize);
                float3 rightHead = arrowEnd + math.mul(quaternion.RotateZ(math.radians(-135)), arrowHeadSize);

                Debug.DrawLine(arrowEnd, leftHead, Color.green);
                Debug.DrawLine(arrowEnd, rightHead, Color.green);
            }
        }
    }
}
