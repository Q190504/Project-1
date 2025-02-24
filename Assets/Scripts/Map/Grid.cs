using System;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Grid<TGridObject>
{
    private const bool NODE_STATUS_NOT_BLOCKED = false;
    private const bool NODE_STATUS_BLOCKED = true;

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private PathNode[,] gridArray;

    private TextMeshPro[,] debugArray;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, bool, PathNode> createPathNode, bool showDebug)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new PathNode[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createPathNode(this, x, y, NODE_STATUS_NOT_BLOCKED);
            }
        }

        if (showDebug)
        {
            debugArray = new TextMeshPro[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    debugArray[x, y] = CreateWorldText(gridArray[x, y].GetStatus() + "\n" + gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 2, Color.white);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        }
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y, 0) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
       x = Mathf.FloorToInt(worldPosition.x - originPosition.x / cellSize);
       y = Mathf.FloorToInt(worldPosition.y - originPosition.y / cellSize);
    }

    public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default, int fontSize = 40, Color color = default, TextAlignmentOptions textAlignment = TextAlignmentOptions.Center, int sortingOrder = 1)
    {
        if(color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, color, textAlignment, sortingOrder);
    }

    public static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAlignmentOptions textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMeshPro textMeshPro = gameObject.GetComponent<TextMeshPro>();
        textMeshPro.alignment = textAlignment;
        textMeshPro.text = text;
        textMeshPro.fontSize = fontSize;
        textMeshPro.color = color;
        textMeshPro.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

        return textMeshPro;
    }

    public void SetNodeCosts(int x, int y, int gCost, int hCost, int fCost)
    {
        if (x >= 0 && y >= 0 && x < height && y < width)
        {
            gridArray[x, y].GCost = gCost;
            gridArray[x, y].HCost = hCost;
            gridArray[x, y].FCost = fCost;
            debugArray[x, y].text = gridArray[x, y].GetStatus() + "\n" + gridArray[x, y].ToString();
            if(hCost > 0) //have checked this node
                debugArray[x, y].color = Color.green;
        }
    }

    public void SetNodeStatus(int x, int y)
    {
        if(x >= 0 && y >= 0 && x < height && y < width)
        {
            gridArray[x, y].IsBlocked = !gridArray[x, y].IsBlocked;
            debugArray[x, y].text = gridArray[x, y].GetStatus() + "\n" + gridArray[x, y].ToString();
            if (gridArray[x, y].IsBlocked == NODE_STATUS_BLOCKED)
                debugArray[x, y].color = Color.black;
            else
                debugArray[x, y].color = Color.white;
        }
    }

    public void SetNodeStatus(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        SetNodeStatus(x, y);
    }

    public PathNode GetNode(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return gridArray[x, y];
        else return default(PathNode);
    }

    public PathNode GetNode(Vector3 worldPosition)
    {
        GetXY(worldPosition, out int x, out int y);
        return gridArray[x, y];
    }

    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
}
