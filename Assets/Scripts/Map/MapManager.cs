using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;


public class MapManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private Vector3 originPosition = Vector3.zero;
    [SerializeField] private bool showDebug;

    private Pathfinding pathfinding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //pathfinding = new Pathfinding(width, height, cellSize, originPosition);
    }

    // Update is called once per frame
    void Update()
    {
        //if(showDebug)
        //{
        //    //select end node
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        Vector3 mouseWorlPosition = GetMouseWorldPosition();

        //        pathfinding.Grid.GetXY(mouseWorlPosition, out int x, out int y);
        //        List<GridPathNode> path = pathfinding.FindPath(0, 0, x, y);

        //        if (path != null)
        //        {
        //            for (int i = 0; i < path.Count - 1; i++)
        //            {
        //                Debug.DrawLine(new Vector3(path[i].X, path[i].Y) + Vector3.one * 0.5f, new Vector3(path[i + 1].X, path[i + 1].Y) + Vector3.one * 0.5f, Color.red, 100f);
        //                //Debug.Log(path[i].ToString());
        //            }
        //        }
        //    }
        //    //toggle node status
        //    if (Input.GetMouseButtonDown(1))
        //    {
        //        Vector3 position = GetMouseWorldPosition();
        //        GridPathNode pathNode = pathfinding.Grid.GetNode(position);
        //        if (pathNode != null)
        //        {
        //            pathfinding.Grid.SetNodeStatus(GetMouseWorldPosition());
        //        }
        //    }
        //}
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
