using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectRuneUtils
{
    public class Edge
    {
        public Tile _node1;
        public Tile _node2;
        public float _weight;
        public bool _isSelected; // used for MST
    }
    
    public class Graph
    {
        public List<Tile> _nodes;
        public List<Edge> _edges;
    }

    public static class GraphManager
    {
        public static Graph ComputeDelaunayTriangulationFromVoronoiGrid(Tile[,] grid, int gridSize, Tile[] germsPosition)
        {
            Graph graph = new Graph();
            graph._edges = new List<Edge>();
            graph._nodes = germsPosition.ToList();
            
            List<Tuple<int, int>> _existingEdges = new List<Tuple<int, int>>();
            
            for(int i = 1; i < gridSize; i++)
            for (int j = 1; j < gridSize; j++)
            {
                int currentVornoiGrp = grid[i, j]._voronoiGrp;
                
                int otherVornoiGrp = grid[i - 1, j]._voronoiGrp;
                if (otherVornoiGrp != currentVornoiGrp && !_existingEdges.Contains(new Tuple<int, int>(otherVornoiGrp, currentVornoiGrp)) &&  !_existingEdges.Contains(new Tuple<int, int>(currentVornoiGrp, otherVornoiGrp)))
                {
                    Edge edge = new Edge();
                    edge._node1 =  germsPosition[currentVornoiGrp];
                    edge._node2 = germsPosition[otherVornoiGrp];
                    edge._weight = Vector2.Distance(germsPosition[currentVornoiGrp]._position,  germsPosition[otherVornoiGrp]._position);
                        
                    graph._edges.Add(edge);
                    _existingEdges.Add(new Tuple<int, int>(otherVornoiGrp, currentVornoiGrp));
                }
                
                otherVornoiGrp = grid[i, j-1]._voronoiGrp;
                if (otherVornoiGrp != currentVornoiGrp && !_existingEdges.Contains(new Tuple<int, int>(otherVornoiGrp, currentVornoiGrp)) &&  !_existingEdges.Contains(new Tuple<int, int>(currentVornoiGrp, otherVornoiGrp)))
                {
                    Edge edge = new Edge();
                    edge._node1 =  germsPosition[currentVornoiGrp];
                    edge._node2 = germsPosition[otherVornoiGrp];
                    edge._weight = Vector2.Distance(germsPosition[currentVornoiGrp]._position,  germsPosition[otherVornoiGrp]._position);
                        
                    graph._edges.Add(edge);
                    _existingEdges.Add(new Tuple<int, int>(otherVornoiGrp, currentVornoiGrp));
                }
            }
            
            return graph;
        }

        public static Graph FindMST(Graph graph)
        {
            Edge random_edge = graph._edges[Random.Range(0, graph._edges.Count-1)];
            Tile startingNode;
            if(Random.Range(0, 100) < 50)
                startingNode = random_edge._node1;
            else
                startingNode = random_edge._node2;
            
            List<Tile> _selectedNodes = new List<Tile>();
            _selectedNodes.Add(startingNode);
            
            while (_selectedNodes.Count < graph._nodes.Count)
            {
                float minWeight = float.MaxValue;
                int selectedEdgeIdx = -1;
                
                for(int edgeIdx = 0; edgeIdx < graph._edges.Count; edgeIdx++)
                {
                    if (!graph._edges[edgeIdx]._isSelected)
                    {
                        if ((_selectedNodes.Contains(graph._edges[edgeIdx]._node1) && !_selectedNodes.Contains(graph._edges[edgeIdx]._node2)) ||
                            (!_selectedNodes.Contains(graph._edges[edgeIdx]._node1) && _selectedNodes.Contains(graph._edges[edgeIdx]._node2)))
                        {
                            if (graph._edges[edgeIdx]._weight < minWeight)
                            {
                                minWeight = graph._edges[edgeIdx]._weight;
                                selectedEdgeIdx = edgeIdx;
                            }
                        }
                    }
                }
                graph._edges.ToArray()[selectedEdgeIdx]._isSelected = true;
                if(!_selectedNodes.Contains(graph._edges[selectedEdgeIdx]._node1))
                    _selectedNodes.Add(graph._edges[selectedEdgeIdx]._node1);
                else
                    _selectedNodes.Add(graph._edges[selectedEdgeIdx]._node2);
            }
            
            Graph mst =  new Graph();
            mst._edges = new List<Edge>();
            foreach (Edge edge in graph._edges)
            {
                if(edge._isSelected)
                    mst._edges.Add(edge);
            }

            return mst;
        }
    }
}