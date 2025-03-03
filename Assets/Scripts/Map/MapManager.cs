using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine.LightTransport;


public class MapManager : MonoBehaviour
{
    public static MapManager Instance {  get; private set; }

    public Grid<GridPathNode> pathfindingGrid; 

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private Vector3 originPosition = Vector3.zero;
    [SerializeField] private bool showDebug;

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathfindingGrid = new Grid<GridPathNode>(width, height, cellSize, originPosition, (Grid<GridPathNode> grid, int x, int y) => new GridPathNode(grid, x, y), showDebug);
    }

    // Update is called once per frame
    void Update()
    {
        if (showDebug)
        {
            //toggle node status
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 position = GetMouseWorldPosition();
                GridPathNode pathNode = pathfindingGrid.GetNode(position);
                if (pathNode != null)
                {
                    pathfindingGrid.SetNodeStatus(GetMouseWorldPosition());
                }
            }
        }
    }

    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vector = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vector.z = 0;
        return vector;
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}
