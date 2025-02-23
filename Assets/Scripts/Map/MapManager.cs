using Unity.Cinemachine;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private Vector3 originPosition = Vector3.zero;
    [SerializeField] private bool showDebug;

    private Grid<PathNode> grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Pathfinding pathfinding = new Pathfinding(width, height, cellSize, originPosition);
        grid = pathfinding.grid;
    }

    // Update is called once per frame
    void Update()
    {
        //toggle node status
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 position = GetMouseWorldPosition();
            PathNode pathNode = grid.GetNode(position);
            if(pathNode != null)
            {
                grid.SetNode(GetMouseWorldPosition());
            }
        }
        //get node
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 position = GetMouseWorldPosition();
            PathNode pathNode = grid.GetNode(position);
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
