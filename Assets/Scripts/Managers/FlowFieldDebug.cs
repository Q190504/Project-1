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
    [SerializeField] private Canvas debugCanvas;
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
            Debug.LogWarning("Flow Field Grid entity not found in FlowFieldDebug.");
            return;
        }

        gridEntity = gridQuery.GetSingletonEntity();
        gridData = entityManager.GetComponentData<FlowFieldGridDataComponent>(gridEntity);

        width = gridData.width;
        height = gridData.height;
        cellSize = gridData.nodeSize;
        origin = gridData.originPosition;

        if (flowFieldDebugStatus == FlowFieldDebugStatus.Cost || flowFieldDebugStatus == FlowFieldDebugStatus.BestCost)
        {
            InitializeDebugTexts();
        }
    }

    private void Update()
    {
        if (gridEntity == Entity.Null || !entityManager.Exists(gridEntity)) return;

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
        }
    }

    private void InitializeDebugTexts()
    {
        debugTexts = new TMP_Text[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float3 nodeCenter = new float3(origin.x + (x + 0.5f) * cellSize, origin.y + (y + 0.5f) * cellSize, 0);
                TMP_Text text = Instantiate(debugTextPrefab, nodeCenter, Quaternion.identity, debugCanvas.transform);
                text.text = "";
                debugTexts[x, y] = text;
            }
        }
    }

    public void ShowCost()
    {
        if (!entityManager.HasBuffer<GridNode>(gridEntity)) return;

        var buffer = entityManager.GetBuffer<GridNode>(gridEntity);
        foreach (var node in buffer)
        {
            debugTexts[node.x, node.y].text = node.cost.ToString();
        }
    }

    public void ShowBestCost()
    {
        if (!entityManager.HasBuffer<GridNode>(gridEntity)) return;

        var buffer = entityManager.GetBuffer<GridNode>(gridEntity);
        foreach (var node in buffer)
        {
            debugTexts[node.x, node.y].text = node.bestCost.ToString();
        }
    }

    public void ShowVector()
    {
        if (!entityManager.HasBuffer<GridNode>(gridEntity)) return;

        var buffer = entityManager.GetBuffer<GridNode>(gridEntity);
        foreach (var node in buffer)
        {
            int x = node.x;
            int y = node.y;

            float3 bottomLeft = new float3(origin.x + x * cellSize, origin.y + y * cellSize, 0);
            float3 topRight = bottomLeft + new float3(cellSize, cellSize, 0);
            float3 topLeft = new float3(bottomLeft.x, topRight.y, 0);
            float3 bottomRight = new float3(topRight.x, bottomLeft.y, 0);

            // Draw the 4 borders of the cell
            Debug.DrawLine(bottomLeft, bottomRight, Color.white);
            Debug.DrawLine(bottomRight, topRight, Color.white);
            Debug.DrawLine(topRight, topLeft, Color.white);
            Debug.DrawLine(topLeft, bottomLeft, Color.white);

            // Calculate the center position of the cell
            float3 center = bottomLeft + new float3(cellSize / 2f, cellSize / 2f, 0);
            float2 vec = node.vector;

            if (math.lengthsq(vec) > 0.0001f)
            {
                float3 dir = math.normalize(new float3(vec.x, vec.y, 0)) * (cellSize * 0.4f);
                float3 arrowEnd = center + dir;

                Debug.DrawLine(center, arrowEnd, Color.green);

                // Draw arrowhead only if there is a valid direction
                float3 arrowHead = math.normalize(dir) * (cellSize * 0.2f);

                // Rotate arrowhead lines ±135 degrees
                float3 leftHead = arrowEnd + math.mul(quaternion.RotateZ(math.radians(135)), arrowHead);
                float3 rightHead = arrowEnd + math.mul(quaternion.RotateZ(math.radians(-135)), arrowHead);

                Debug.DrawLine(arrowEnd, leftHead, Color.green);
                Debug.DrawLine(arrowEnd, rightHead, Color.green);
            }
        }
    }
}