using System.Collections.Generic;
using UnityEngine;

namespace ProjectRuneUtils
{
    public static class PathFinding
    {
        private static Dictionary<TileType, int> AStarCost = new Dictionary<TileType, int>
        {
            { TileType.Empty, 5},
            { TileType.Room, 10},
            { TileType.Hallway, 1}
        };
        
        public static void ApplyAStarFromMSTGraph(Graph graph, Tile[,] grid, int gridSize)
        {
            foreach (Edge e in graph._edges)
            {
                List<Tile> path = PathFinding.FindPath(e._node1, e._node2, grid, gridSize);
                foreach(Tile t in path)
                {
                    if(t._type == TileType.Empty)
                        t._type = TileType.Hallway;
                }
            }
        }
        
        private static List<Tile> FindPath(Tile start, Tile end, Tile[,] grid, int gridSize)
        {
            List<Tile> openList = new List<Tile>() {start};
            HashSet<Tile> closedList = new HashSet<Tile>();
            while (openList.Count > 0)
            {
                Tile currentTile =  openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (AStarCost[openList[i]._type] < AStarCost[currentTile._type] ||
                        AStarCost[openList[i]._type] == AStarCost[currentTile._type] && openList[i].hCost < currentTile.hCost)
                    {
                        currentTile = openList[i];
                    }
                }
                
                openList.Remove(currentTile);
                closedList.Add(currentTile);

                if (currentTile == end)
                {
                    return RetracePath(start, end);
                }

                foreach (Tile neighbor in GetNeighbors(currentTile, grid, gridSize))
                {
                    if (closedList.Contains(neighbor)) continue;
                    int newMovementCostToNeighbor = currentTile.gCost + GetDistance(currentTile, neighbor);
                    if (newMovementCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, end);
                        neighbor.parent = currentTile;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private static int GetDistance(Tile nodeA, Tile nodeB)
        {
            int distanceX = Mathf.Abs((int)nodeA._position.x - (int)nodeB._position.x);
            int distanceY = Mathf.Abs((int)nodeA._position.y - (int)nodeB._position.y);

            if (distanceX > distanceY)
            {
                return 14 * distanceY + 10 * (distanceX - distanceY);
            }
            else
            {
                return 14 * distanceX + 10 * (distanceY - distanceX);
            }
        }

        private static List<Tile> GetNeighbors(Tile node, Tile[,] grid,  int gridSize)
        {
            List<Tile> neighbors = new List<Tile>();
            if(node._position.x - 1 >= 0)
                neighbors.Add(grid[(int)node._position.x - 1, (int)node._position.y]);
            if(node._position.x + 1 < gridSize)
                neighbors.Add(grid[(int)node._position.x + 1, (int)node._position.y]);
            if(node._position.y - 1 >= 0)
                neighbors.Add(grid[(int)node._position.x, (int)node._position.y - 1]);
            if(node._position.y + 1 < gridSize)
                neighbors.Add(grid[(int)node._position.x, (int)node._position.y + 1]);

            return neighbors;
        }

        private static List<Tile> RetracePath(Tile startNode, Tile endNode)
        {
            List<Tile> path = new List<Tile>();
            Tile currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();
            return path;
        }
    }
}