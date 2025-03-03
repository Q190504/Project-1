using UnityEngine;

public class GridPathNode
{
    
    private Grid<GridPathNode> grid;
    public int X { get; set; }
    public int Y { get; set; }

    public int GCost { get; set; }
    public int FCost { get; set; }
    public int HCost { get; set; }

    public GridPathNode CameFromNode { get; set; }

    public bool IsWalkable { get; set; }

    public GridPathNode(Grid<GridPathNode> grid, int x, int y)
    {
        this.grid = grid;
        X = x;
        Y = y;
        IsWalkable = true;
    }

    public string GetStatus()
    {
        if (IsWalkable)
            return $"g:{GCost}\nf:{FCost}\nh:{HCost}";
        else
            return "BLOCKED";
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
