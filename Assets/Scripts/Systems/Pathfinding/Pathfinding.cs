using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.PlayerLoop;

public partial struct Pathfinding : ISystem
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14; //14 = sprt(10^2 + 10^2) (Pytago)

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        int gridWidth = MapManager.Instance.pathfindingGrid.GetWidth();
        int gridHeight = MapManager.Instance.pathfindingGrid.GetHeight();
        int2 gridSize = new int2(gridWidth, gridHeight);

        foreach (var (pathFindingComponent, entity) in
                 SystemAPI.Query<RefRO<PathFindingComponent>>().WithEntityAccess())
        {
            var pathBuffer = SystemAPI.GetBuffer<PathPositionComponent>(entity);
            var pathFollowComponentDataFromEntity = SystemAPI.GetComponentLookup<PathFollowComponent>();

            FindPathJob findPathJob = new FindPathJob
            {
                gridSize = gridSize,
                pathNodeArray = GetPathNodeArray(),
                startPosition = pathFindingComponent.ValueRO.startPosition,
                endPosition = pathFindingComponent.ValueRO.endPosition,
                pathPositionBuffer = pathBuffer,
                entity = entity,
                pathFollowComponentDataFromEntity = pathFollowComponentDataFromEntity
            };

            findPathJob.Run();

            ecb.RemoveComponent<PathFindingComponent>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private NativeArray<PathNode> GetPathNodeArray()
    {
        Grid<GridPathNode> grid = MapManager.Instance.pathfindingGrid;

        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());
        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.TempJob);

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode node = new PathNode();
                //x = column, y = row
                node.x = x;
                node.y = y;
                node.index = CalculateNodeIndex(x, y, gridSize.x);

                node.gCost = int.MaxValue;

                node.CalculateFCost();

                node.isWalkable = grid.GetNode(x, y).IsWalkable;

                node.cameFromNodeIndex = -1;

                pathNodeArray[node.index] = node;
            }
        }

        return pathNodeArray;
    }

    [BurstCompile]
    private struct FindPathJob : IJob
    {
        [DeallocateOnJobCompletion]
        public NativeArray<PathNode> pathNodeArray;
        public int2 gridSize;

        public int2 startPosition;
        public int2 endPosition;

        public Entity entity;
        public ComponentLookup<PathFollowComponent> pathFollowComponentDataFromEntity;
        public DynamicBuffer<PathPositionComponent> pathPositionBuffer;

        public void Execute()
        {
            for (int i = 0; i < pathNodeArray.Length; i++)
            {
                PathNode pathNode = pathNodeArray[i];
                pathNode.hCost = CalculateDistanceCost(new int2(pathNode.x, pathNode.y), endPosition);
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[i] = pathNode;
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

            //Calculate startNode's & endNode's index
            int endNodeIndex = CalculateNodeIndex(endPosition.x, endPosition.y, gridSize.x);
            PathNode startNode = pathNodeArray[CalculateNodeIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedHashMap = new NativeList<int>(Allocator.Temp);


            #region Finding Path
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

            #endregion

            #region Return result

            pathPositionBuffer.Clear();
            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                //Didn't find a path
                pathFollowComponentDataFromEntity[entity] = new PathFollowComponent { pathIndex = -1 };
            }
            else
            {
                //Found a path
                CalculatePath(pathNodeArray, endNode, pathPositionBuffer);
                //Set the first node index to path follow component data (pathPositionBuffer.Length - 1 because the path is currently in reverse: endNode -> startNode)
                pathFollowComponentDataFromEntity[entity] = new PathFollowComponent { pathIndex = pathPositionBuffer.Length - 1 };
            }

            #endregion

            #region Disposing

            neighborOffsetArray.Dispose();
            openList.Dispose();
            closedHashMap.Dispose();

            #endregion
        }
    }

    private static int CalculateNodeIndex(int column, int row, int gridWidth)
    {
        return column + row * gridWidth;
    }

    private static int CalculateDistanceCost(int2 startPosition, int2 endPosition)
    {
        int xDistance = math.abs(startPosition.x - endPosition.x);
        int yDistance = math.abs(startPosition.y - endPosition.y);
        int remaining = math.abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + remaining * MOVE_STRAIGHT_COST;
    }

    private static int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
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

    private static bool IsPositionInsideGrid(int2 position, int2 gridSize)
    {
        return position.x >= 0 &&
            position.y >= 0 &&
            position.x < gridSize.x &&
            position.y < gridSize.y;
    }

    private static void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathPositionComponent> pathPositionBuffer)
    {
        pathPositionBuffer.Add(new PathPositionComponent { position = new int2(endNode.x, endNode.y) });

        PathNode currentNode = endNode;
        while (currentNode.cameFromNodeIndex != -1)
        {
            PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
            pathPositionBuffer.Add(new PathPositionComponent { position = new int2(cameFromNode.x, cameFromNode.y) });
            currentNode = cameFromNode;
        }
    }

    private static NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
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
