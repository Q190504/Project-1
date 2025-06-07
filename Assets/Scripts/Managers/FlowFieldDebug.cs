using UnityEngine;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;

public enum FlowFieldDebugStatus
{
    None,
    Cost,
    BestCost,
    Vector,
}

public class FlowFieldDebug : MonoBehaviour
{
    [SerializeField] private TMP_Text debugTextPrefab;
    [SerializeField] private Canvas canvas;

    [SerializeField] private FlowFieldDebugStatus flowFieldDebugStatus;

    private TMP_Text[,] debugTexts;

    private Entity gridEntity;
    private EntityManager entityManager;
    private FlowFieldGridDataComponent gridData;

    private int width;
    private int height;
    private float cellSize;
    private Vector3 origin;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQuery gridQuery = entityManager.CreateEntityQuery(typeof(FlowFieldGridDataComponent));

        if (gridQuery.IsEmpty)
        {
            Debug.Log("Cant find Flow Field Grid entity in FlowFieldDebug");
            return;
        }

        gridEntity = gridQuery.GetSingletonEntity();
        gridData = entityManager.GetComponentData<FlowFieldGridDataComponent>(gridEntity);

        width = gridData.width;
        height = gridData.height;
        cellSize = gridData.nodeSize;
        origin = gridData.originPosition;

        debugTexts = new TMP_Text[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate the center position of the cell
                float3 nodeCenter = new float3(origin.x + (x + 0.5f) * cellSize, origin.y + (y + 0.5f) * cellSize, 0);
                TMP_Text text = Instantiate(debugTextPrefab, nodeCenter, Quaternion.identity, canvas.transform);
                debugTexts[x, y] = text;
            }
        }
    }

    private void Update()
    {
        switch (flowFieldDebugStatus)
        {
            case FlowFieldDebugStatus.Cost:
                ShowCost();
                break;
            case FlowFieldDebugStatus.BestCost:
                ShowBestCost();
                break;
            case FlowFieldDebugStatus.Vector:
                ShowVector();
                break;
            default: break;
        }
    }

    public void ShowCost()
    {
        if (entityManager.HasBuffer<GridNode>(gridEntity))
        {
            DynamicBuffer<GridNode> pathBuffer = entityManager.GetBuffer<GridNode>(gridEntity);

            for (int i = 0; i < pathBuffer.Length; i++)
            {
                GridNode node = pathBuffer[i];
                debugTexts[node.x, node.y].text = node.cost.ToString();
            }
        }
    }

    public void ShowBestCost()
    {
        if (entityManager.HasBuffer<GridNode>(gridEntity))
        {
            DynamicBuffer<GridNode> pathBuffer = entityManager.GetBuffer<GridNode>(gridEntity);

            for (int i = 0; i < pathBuffer.Length; i++)
            {
                GridNode node = pathBuffer[i];
                debugTexts[node.x, node.y].text = node.bestCost.ToString();
            }
        }
    }

    public void ShowVector()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                debugTexts[x, y].enabled = false;
            }
        }

        if (entityManager.HasBuffer<GridNode>(gridEntity))
        {
            DynamicBuffer<GridNode> pathBuffer = entityManager.GetBuffer<GridNode>(gridEntity);

            for (int i = 0; i < pathBuffer.Length; i++)
            {
                GridNode node = pathBuffer[i];

                int x = node.x;
                int y = node.y;

                float3 bottomLeft = new float3(origin.x + x * cellSize, origin.y + y * cellSize, 0);
                float3 bottomRight = bottomLeft + new float3(cellSize, 0, 0);
                float3 topRight = bottomRight + new float3(0, cellSize, 0);
                float3 topLeft = bottomLeft + new float3(0, cellSize, 0);

                // Draw the 4 borders of the cell
                Debug.DrawLine(bottomLeft, bottomRight, Color.white);
                Debug.DrawLine(bottomRight, topRight, Color.white);
                Debug.DrawLine(topRight, topLeft, Color.white);
                Debug.DrawLine(topLeft, bottomLeft, Color.white);

                // Calculate the center position of the cell
                float3 nodeCenter = new float3(origin.x + (x + 0.5f) * cellSize, origin.y + (y + 0.5f) * cellSize, 0);

                // Main arrow direction
                float3 direction = math.normalize(new float3(node.vector.x, node.vector.y, 0)) * (cellSize * 0.4f);

                float3 arrowEnd = nodeCenter + direction;

                Debug.DrawLine(nodeCenter, arrowEnd, Color.green);

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
}