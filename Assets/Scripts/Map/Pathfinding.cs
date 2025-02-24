using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14; //14 = sprt(10^2 + 10^2) (Pytago)

    public Grid<PathNode> Grid { get; set; }
    private List<PathNode> openList;
    private HashSet<PathNode> closedList;

    public Pathfinding(int width, int height, float cellSize, Vector3 originPosition)
    {
        Grid = new Grid<PathNode>(width, height, cellSize, originPosition, (Grid<PathNode> g, int x, int y, bool isWalkable) => new PathNode(g, x, y, isWalkable), true);
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = Grid.GetNode(startX, startY);
        PathNode endNode = Grid.GetNode(endX, endY);

        openList = new List<PathNode>() { startNode };
        closedList = new HashSet<PathNode>();

        for (int i = 0; i < Grid.GetWidth(); i++)
        {
            for (int j = 0; j < Grid.GetHeight(); j++)
            {
                PathNode node = Grid.GetNode(i, j);

                node.GCost = int.MaxValue;
                node.FCost = node.CaculateFCost();
                node.CameFromNode = null;

                Grid.SetNodeCosts(i, j, node.GCost, node.HCost, node.FCost);
            }
        }

        startNode.GCost = 0;
        startNode.HCost = CalculateDistanceCost(startNode, endNode);
        startNode.FCost = startNode.CaculateFCost();

        Grid.SetNodeCosts(startNode.X, startNode.Y, startNode.GCost, startNode.HCost, startNode.FCost);

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) //Reach the end node
                return CalculatePath(endNode);
            else
            {
                openList.Remove(currentNode);
                closedList.Add(currentNode);


                foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
                {
                    if (closedList.Contains(neighbourNode) || neighbourNode.IsBlocked) continue;

                    int tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.GCost) //if the path from the current node to this neighbour node is faster than the previous path
                    {
                        //update new path to this neighbour node
                        neighbourNode.CameFromNode = currentNode;
                        neighbourNode.GCost = tentativeGCost;
                        neighbourNode.HCost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.FCost = neighbourNode.CaculateFCost();

                        Grid.SetNodeCosts(neighbourNode.X, neighbourNode.Y, neighbourNode.GCost, neighbourNode.HCost, neighbourNode.FCost);

                        if (!openList.Contains(neighbourNode))
                            openList.Add(neighbourNode);
                    }
                }
            }
        }

        //Unable to find a path
        return null;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        //Left
        if (currentNode.X - 1 >= 0)
        {
            neighbourList.Add(Grid.GetNode(currentNode.X - 1, currentNode.Y));

            //Left Up
            if (currentNode.Y + 1 < Grid.GetHeight())
                neighbourList.Add(Grid.GetNode(currentNode.X - 1, currentNode.Y + 1));
            //Left Down
            if (currentNode.Y - 1 >= 0)
                neighbourList.Add(Grid.GetNode(currentNode.X - 1, currentNode.Y - 1));
        }

        //Right
        if (currentNode.X + 1 < Grid.GetWidth())
        {
            neighbourList.Add(Grid.GetNode(currentNode.X + 1, currentNode.Y));

            //Right Up
            if (currentNode.Y + 1 < Grid.GetHeight())
                neighbourList.Add(Grid.GetNode(currentNode.X + 1, currentNode.Y + 1));
            //Right Down
            if (currentNode.Y - 1 >= 0)
                neighbourList.Add(Grid.GetNode(currentNode.X + 1, currentNode.Y - 1));
        }

        //Down
        if (currentNode.Y - 1 >= 0)
            neighbourList.Add(Grid.GetNode(currentNode.X, currentNode.Y - 1));

        //Up
        if (currentNode.Y + 1 < Grid.GetHeight())
            neighbourList.Add(Grid.GetNode(currentNode.X, currentNode.Y + 1));

        return neighbourList;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodes = new List<PathNode>() { endNode };
        PathNode currentNode = endNode;

        while (currentNode.CameFromNode != null)
        {
            pathNodes.Add(currentNode);
            currentNode = currentNode.CameFromNode;
        }

        pathNodes.Add(currentNode); //add the start node

        pathNodes.Reverse();
        return pathNodes;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.X - b.X);
        int yDistance = Mathf.Abs(a.Y - b.Y);
        int remaining = Mathf.Abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        foreach (PathNode node in pathNodeList)
        {
            if (node.FCost < lowestFCostNode.FCost)
                lowestFCostNode = node;
        }

        return lowestFCostNode;
    }
}
