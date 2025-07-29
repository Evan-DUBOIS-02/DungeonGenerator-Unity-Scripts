using System;
using System.Collections.Generic;
using ProjectRuneUtils;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TileType
{
    Empty,
    Room,
    Hallway
}

public class Tile
{
    public Vector3 _position;
    public TileType _type;
    public int _voronoiGrp;
    public int hCost;       // Used for A*
    public int gCost;       // Used for A*
    public Tile parent;     // Used for A*
    public bool isRoomCenter;
}

public enum PreviewType
{
    Type,
    Voronoi
}

namespace DungeonGenerator
{
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Rooms")] 
        [SerializeField] private int NUMBER_OF_ROOMS = 5;
        private int _gridSize;
        private Tile[] _roomsPositions;
        Tile[,] _grid;

        [Header("Preview")] 
        [SerializeField] private Material _roomMaterial;
        [SerializeField] private Material _emptyMaterial;
        [SerializeField] private Material _hallwaysMaterial;
        private Dictionary<int, Material> _voronoiMaterials;
        [SerializeField] private PreviewType _previewType;
        private List<GameObject> _gridPreview = new List<GameObject>();
        
        [Header("Prefabs")]
        [SerializeField] private GameObject _roomPrefab;
        [SerializeField] private GameObject _hallwayPrefab;
        
        public void Generate()
        {
            // STEP 0: Initialization
            _roomsPositions = new Tile[NUMBER_OF_ROOMS];
            _gridSize = (NUMBER_OF_ROOMS + 1) * 5;
            _grid = GenerateGrid();
            _voronoiMaterials = new Dictionary<int, Material>();
            
            // STEP 1: Place rooms
            int currentRoomGrp = 0;
            for (int i = 0; i < NUMBER_OF_ROOMS; i++)
            {
                Material voronoiMat = new Material(_emptyMaterial);
                voronoiMat.color = Color.Lerp(Color.white, Color.black, currentRoomGrp/(float)NUMBER_OF_ROOMS);
                _voronoiMaterials.Add(currentRoomGrp, voronoiMat);
                _roomsPositions[i] = PlaceRoom();
                _grid[(int)_roomsPositions[i]._position.x, (int)_roomsPositions[i]._position.y]._voronoiGrp = currentRoomGrp;
                _grid[(int)_roomsPositions[i]._position.x, (int)_roomsPositions[i]._position.y].isRoomCenter = true;
                currentRoomGrp++;
            }
            
            // STEP 2: Create Delaunay Triangulation
            SetupVoronoiGroup();
            Graph delaunayTriangulation = GraphManager.ComputeDelaunayTriangulationFromVoronoiGrid(_grid, _gridSize, _roomsPositions);
            
            // STEP 3: Find Minimum Spanning Tree
            Graph mst = GraphManager.FindMST(delaunayTriangulation);
            
            // STEP 4: Apply A*
            PathFinding.ApplyAStarFromMSTGraph(mst, _grid, _gridSize);
                
            DisplayGrid();
            ShowGraph(delaunayTriangulation);
            
            // STEP 5: Apply prefabs
            ApplyPrefab();
        }
        
        private Tile[,] GenerateGrid()
        {
            Tile[,] grid = new Tile[_gridSize, _gridSize];
            for(int i = 0; i < _gridSize; i++)
            for (int j = 0; j < _gridSize; j++)
            {
                grid[i, j] = new Tile();
                grid[i, j]._type = TileType.Empty;
                grid[i, j]._position = new Vector2(i, j);
            }
            
            return grid;
        }

        private Tile PlaceRoom()
        {
            bool isPositionValid = false;
            int randomX, randomY;
            do
            {
                randomX = Random.Range(2, _gridSize-3);
                randomY = Random.Range(2, _gridSize-3);
                isPositionValid = CheckRoomPosition(randomX, randomY);
                
            } while (!isPositionValid);

            for (int i = randomX - 1; i <= randomX+1; i++)
            {
                for (int j = randomY - 1; j <= randomY + 1; j++)
                {
                    _grid[i, j]._type = TileType.Room;
                }
            }
            
            return _grid[randomX,randomY];
        }

        private bool CheckRoomPosition(int x, int y)
        {
            for (int i = x - 2; i <= x + 2; i++)
            {
                for (int j = y - 2; j <= y + 2; j++)
                {
                    if (_grid[i, j]._type == TileType.Room)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        private void SetupVoronoiGroup()
        {
            for(int i = 0; i < _gridSize; i++)
            for (int j = 0; j < _gridSize; j++)
            {
                float minDistance = float.MaxValue;
                int nearestGrp = 0;

                for (int roomId = 0; roomId < NUMBER_OF_ROOMS; roomId++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(i, j),
                        new Vector2(_roomsPositions[roomId]._position.x,  _roomsPositions[roomId]._position.y));
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestGrp = _grid[(int)_roomsPositions[roomId]._position.x, (int)_roomsPositions[roomId]._position.y]._voronoiGrp;
                    }
                }
                
                _grid[i,j]._voronoiGrp =  nearestGrp;
            }
        }

        private void ApplyPrefab()
        {
            for(int i = 0; i < _gridSize; i++)
            for (int j = 0; j < _gridSize; j++)
            {
                GameObject structureGO = null;
                Structure structure = null;
                switch (_grid[i, j]._type)
                {
                    case TileType.Room:
                        if (_grid[i, j].isRoomCenter)
                        {
                            structureGO = Instantiate(_roomPrefab, new Vector3(i, 2, j), Quaternion.identity);
                            if (structureGO.TryGetComponent<Structure>(out structure))
                            {
                                if (i - 2 >= 0 && _grid[i - 2, j]._type == TileType.Hallway)
                                    structure.OpenXnegDoor();
                                if (i + 2 < _gridSize && _grid[i + 2, j]._type == TileType.Hallway)
                                    structure.OpenXposDoor();
                                if (j - 2 >= 0 && _grid[i, j - 2]._type == TileType.Hallway)
                                    structure.OpenZnegDoor();
                                if (j + 2 < _gridSize && _grid[i, j + 2]._type == TileType.Hallway)
                                    structure.OpenZposDoor();
                            }
                        }

                        break;
                    case TileType.Hallway:
                        structureGO = Instantiate(_hallwayPrefab, new Vector3(i, 2, j), Quaternion.identity);
                        if (structureGO.TryGetComponent<Structure>(out structure))
                        {
                            if(i-1 >= 0 && (_grid[i-1,j]._type == TileType.Hallway ||  _grid[i-1,j]._type == TileType.Room))
                                structure.OpenXnegDoor();
                            if(i+1 < _gridSize && (_grid[i+1,j]._type == TileType.Hallway  ||  _grid[i+1,j]._type == TileType.Room))
                                structure.OpenXposDoor();
                            if(j-1 >= 0 && (_grid[i,j-1]._type == TileType.Hallway ||  _grid[i,j-1]._type == TileType.Room))
                                structure.OpenZnegDoor();
                            if(j+1<_gridSize && (_grid[i,j+1]._type == TileType.Hallway || _grid[i,j+1]._type == TileType.Room))
                                structure.OpenZposDoor();
                        }
                        break;
                    default:
                        break;
                }
                
                
            }
        }
        
        public void DisplayGrid()
        {
            ClearPreview();

            for(int i = 0; i < _gridSize; i++)
            for (int j = 0; j < _gridSize; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(i, 0, j);
                cube.transform.parent = transform;
                cube.name = i + "_" + j;
                ApplyMaterial(cube, i, j);
                _gridPreview.Add(cube);
            }
        }

        private void ApplyMaterial(GameObject cube, int x, int y)
        {
            switch (_previewType)
            {
                case PreviewType.Type:
                    switch (_grid[x, y]._type)
                    {
                        case TileType.Room:
                            cube.GetComponent<Renderer>().sharedMaterial = _roomMaterial;
                            break;
                        case TileType.Empty:
                            cube.GetComponent<Renderer>().sharedMaterial = _emptyMaterial;
                            break;
                        case TileType.Hallway:
                            cube.GetComponent<Renderer>().sharedMaterial = _hallwaysMaterial;
                            break;
                    }
                    break;
                case PreviewType.Voronoi:
                    cube.GetComponent<Renderer>().sharedMaterial = _voronoiMaterials[_grid[x, y]._voronoiGrp]; 
                    break;
            }
        }

        private void ClearPreview()
        {
            foreach (GameObject cube in _gridPreview)
            {
                DestroyImmediate(cube.gameObject);
            }
            _gridPreview.Clear();
            
            foreach (var go in GameObject.FindGameObjectsWithTag("DungeonStructure"))
            {
                DestroyImmediate(go);
            }
        }

        private void ShowGraph(Graph graph)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag("Lines"))
            {
                DestroyImmediate(go);
            }
            
            GameObject lines =  GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lines.transform.tag = "Lines";
            foreach (Edge edge in graph._edges)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                line.transform.parent = lines.transform;
                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                lineRenderer.SetPosition(0, new Vector3(edge._node1._position.x, 1.5f,  edge._node1._position.y));
                lineRenderer.SetPosition(1, new Vector3(edge._node2._position.x, 1.5f,  edge._node2._position.y));
                if (edge._isSelected)
                {
                    lineRenderer.sharedMaterial = _roomMaterial;
                }
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
}

