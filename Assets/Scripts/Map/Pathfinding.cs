using UnityEngine;

public class Pathfinding
{
    public Grid<PathNode> grid;

    public Pathfinding(int width, int height, float cellSize, Vector3 originPosition)
    {
        grid = new Grid<PathNode>(width, height, cellSize, originPosition, (Grid<PathNode> g , int x, int y, bool isWalkable) => new PathNode(g, x, y, isWalkable), true);
    }
}
