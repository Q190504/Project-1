using UnityEngine;

public class PathNode
{
    private Grid<PathNode> grid;
    private int x;
    private int y;

    public int gCost;
    public int fCost;
    public int hCost;

    public PathNode cameFromNode;

    public bool isBlocked;

    public PathNode(Grid<PathNode> grid, int x, int y, bool isBlocked)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isBlocked = isBlocked;
    }

    public override string ToString()
    {
        if(isBlocked) 
            return "BLOCKED";
        else
            return $"g:{gCost}\nf:{fCost}\nh:{hCost}";
    }
}
