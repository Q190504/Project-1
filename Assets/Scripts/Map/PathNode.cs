using UnityEngine;

public class PathNode
{
    private Grid<PathNode> grid;
    public int X { get; set; }
    public int Y { get; set; }

    public int GCost { get; set; }
    public int FCost { get; set; }
    public int HCost { get; set; }

    public PathNode CameFromNode { get; set; }

    public bool IsBlocked { get; set; }

    public PathNode(Grid<PathNode> grid, int x, int y, bool isBlocked)
    {
        this.grid = grid;
        this.X = x;
        this.Y = y;
        this.IsBlocked = isBlocked;
    }

    public string GetStatus()
    {
        if (IsBlocked)
            return "BLOCKED";
        else
            return $"g:{GCost}\nf:{FCost}\nh:{HCost}";
    }

    public override string ToString()
    {
        return X + ", " + Y;
    }

    public int CaculateFCost()
    {
        FCost = GCost + HCost;
        return FCost;
    }
}
