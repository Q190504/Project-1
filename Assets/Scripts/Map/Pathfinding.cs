using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14; //14 = sprt(10^2 + 10^2) (Pytago)

    private void Start()
    {
        int findPathJobCount = 50;
        NativeArray<JobHandle>  jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);

        for (int i = 0; i < findPathJobCount; i++)
        {
            FindPathJob findPathJob = new FindPathJob
            {
                startPosition = new int2(0, 0),
                endPosition = new int2(99, 99)
            };
            jobHandleArray[i] = findPathJob.Schedule();
        }

        JobHandle.CompleteAll(jobHandleArray);

        jobHandleArray.Dispose();
    }

    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;

        public void Execute()
        {
            int2 gridSize = new int2(100, 100);

            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    PathNode node = new PathNode();
                    //x = column, y = row
                    node.x = x;
                    node.y = y;
                    node.index = CalculateNodeIndex(x, y, gridSize.x);

                    node.gCost = int.MaxValue;
                    node.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    node.CalculateFCost();

                    node.isWalkable = true;
                    node.cameFromNodeIndex = -1;

                    pathNodeArray[node.index] = node;
                }
            }

            //Create neighbors' position
            NativeArray<int2> neighborOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighborOffsetArray[0] = new int2(-1, 0);   //left
            neighborOffsetArray[1] = new int2(-1, 1);   //left up
            neighborOffsetArray[2] = new int2(-1, -1);  //left down
            neighborOffsetArray[3] = new int2(1, 0);    //right
            neighborOffsetArray[4] = new int2(1, 1);    //right up
            neighborOffsetArray[5] = new int2(1, -1);   //right down
            neighborOffsetArray[6] = new int2(0, 1);    //up
            neighborOffsetArray[7] = new int2(0, -1);   //down

            int endNodeIndex = CalculateNodeIndex(endPosition.x, endPosition.y, gridSize.x);

            PathNode startNode = pathNodeArray[CalculateNodeIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedHashMap = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);
            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];
                if (currentNodeIndex == endNodeIndex)
                {
                    //reach the end node
                    break;
                }

                //Remove current node from openList
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedHashMap.Add(currentNodeIndex);

                //Go to valid neighbors
                for (int i = 0; i < neighborOffsetArray.Length; i++)
                {
                    int2 neighborOffset = neighborOffsetArray[i];
                    int2 neighborPosition = new int2(currentNode.x + neighborOffset.x, currentNode.y + neighborOffset.y);

                    //Neighbor not valid position
                    if (!IsPositionInsideGrid(neighborPosition, gridSize))
                        continue;

                    int neighborNodeIndex = CalculateNodeIndex(neighborPosition.x, neighborPosition.y, gridSize.x);

                    //Already sreached this node
                    if (closedHashMap.Contains(neighborNodeIndex))
                        continue;

                    PathNode neighbourNode = pathNodeArray[neighborNodeIndex];
                    //Neighbor node is not walkable
                    if (!neighbourNode.isWalkable)
                        continue;

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);
                    int2 neighbourNodePosition = new int2(neighbourNode.x, neighbourNode.y);
                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourNodePosition);
                    if (tentativeGCost < neighbourNode.gCost) //if the path from the current node to this neighbour node is faster than the previous path
                    {
                        //update new path to this neighbour node
                        neighbourNode.cameFromNodeIndex = currentNode.index;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.gCost = CalculateDistanceCost(neighbourNodePosition, endPosition);
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighborNodeIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                            openList.Add(neighbourNode.index);
                    }
                }
            }

            PathNode endNode = pathNodeArray[endNodeIndex];

            if (endNode.cameFromNodeIndex == -1)
            {
                //Didn't find a path
            }
            else
            {
                //Found a path
                NativeList<int2> path = CalculatePath(pathNodeArray, endNode);

                path.Dispose();
            }

            pathNodeArray.Dispose();
            neighborOffsetArray.Dispose();
            openList.Dispose();
            closedHashMap.Dispose();
        }

        private int CalculateNodeIndex(int column, int row, int gridWidth)
        {
            return column + row * gridWidth;
        }

        private int CalculateDistanceCost(int2 startPosition, int2 endPosition)
        {
            int xDistance = math.abs(startPosition.x - endPosition.x);
            int yDistance = math.abs(startPosition.y - endPosition.y);
            int remaining = math.abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + remaining * MOVE_STRAIGHT_COST;
        }

        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostFNode = pathNodeArray[openList[0]];
            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostFNode.fCost)
                    lowestCostFNode = testPathNode;
            }
            return lowestCostFNode.index;
        }

        private bool IsPositionInsideGrid(int2 position, int2 gridSize)
        {
            return position.x >= 0 &&
                position.y >= 0 &&
                position.x < gridSize.x &&
                position.y < gridSize.y;
        }

        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            //Didn't find a path
            if (endNode.cameFromNodeIndex == -1)
                return new NativeList<int2>(Allocator.Temp);
            else //Found a path
            {
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }

                return path;
            }
        }

        private struct PathNode
        {
            public int x;
            public int y;

            public int index;

            public int gCost;
            public int fCost;
            public int hCost;

            public bool isWalkable;

            public int cameFromNodeIndex;

            public void CalculateFCost()
            {
                fCost = gCost + hCost;
            }
        }
    }
}
