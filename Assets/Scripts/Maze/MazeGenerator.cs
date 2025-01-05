using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.UIElements;

public class MazeGenerator : MonoBehaviour
{
    //Set up singleton
    public static MazeGenerator Instance { get; private set; }

    
    public float hiddenBadProb = 0.2f;
    public float hiddenGoodProb = 0.2f;
    public float goodReward = 10f;
    public float badPenalty = -5f;
    
    public int width = 21;
    public int height = 21;
    public float scaleFactor = 2f;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pathMarkerPrefab;
    public Vector2Int startPosition = new Vector2Int(1, 1);
    public Vector2Int endPosition = new Vector2Int(19, 19);

    public Cell[,] grid;
    public Node[,] nodes;
    public Stack<Vector2Int> stack = new Stack<Vector2Int>();

    void Awake()
    {
        InitializeGrid();
        GenerateMaze();
        InitializeNodes();
        AssignHiddenRewards();
        PlaceCharacters();
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGrid()
    {
        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell();
                Vector3 position = new Vector3(x*scaleFactor, 1, y*scaleFactor);
                grid[x, y].Instance = Instantiate(wallPrefab, position, Quaternion.identity, transform);
                grid[x, y].CellState = grid[x, y].Instance.AddComponent<CellState>();
                grid[x, y].CellState.x = x;
                grid[x, y].CellState.y = y;
            }
        }
    }

    void GenerateMaze()
    {
        Vector2Int current = new Vector2Int(1, 1);
        stack.Push(current);
        SetCell(current.x, current.y, false);

        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(current);

            if (unvisitedNeighbors.Count > 0)
            {
                stack.Push(current);
                Vector2Int next = unvisitedNeighbors[UnityEngine.Random.Range(0, unvisitedNeighbors.Count)];
                SetCell((current.x + next.x) / 2, (current.y + next.y) / 2, false);
                SetCell(next.x, next.y, false);
                stack.Push(next);
            }
        }

        EnsurePath(startPosition, endPosition);
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(0, 2), // Up
            new Vector2Int(2, 0), // Right
            new Vector2Int(0, -2), // Down
            new Vector2Int(-2, 0) // Left
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = new Vector2Int(pos.x + dir.x, pos.y + dir.y);
            if (IsInBounds(newPos) && !grid[newPos.x, newPos.y].IsVisited)
            {
                neighbors.Add(newPos);
            }
        }
        
        return neighbors;
    }

    bool IsInBounds(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < width - 1 && pos.y > 0 && pos.y < height - 1;
    }

    void EnsurePath(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;
        while (current != end)
        {
            if (UnityEngine.Random.value < 0.5f && current.x < end.x)
            {
                SetCell(current.x + 1, current.y, false);
                SetCell(current.x + 2, current.y, false);
                current.x += 2;
            }
            else if (current.y < end.y)
            {
                SetCell(current.x, current.y + 1, false);
                SetCell(current.x, current.y + 2, false);
                current.y += 2;
            }
            else if (current.x < end.x)
            {
                SetCell(current.x + 1, current.y, false);
                SetCell(current.x + 2, current.y, false);
                current.x += 2;
            }
        }
    }

    void SetCell(int x, int y, bool isWall)
    {
        if (grid[x, y] == null) return;

        Destroy(grid[x, y].Instance);
        grid[x, y].IsWall = isWall;
        grid[x, y].IsVisited = true;
        Vector3 position = new Vector3(x * scaleFactor, 0, y * scaleFactor);
        grid[x, y].Instance = Instantiate(isWall ? wallPrefab : floorPrefab, position, Quaternion.identity, transform);

        CellState cellState = grid[x, y].Instance.AddComponent<CellState>();
        cellState.x = x;
        cellState.y = y;
        cellState.isWalkable = !isWall;

        if (cellState.isWalkable)
        {
            int wallCount = 0;

            // Assuming you have a method to check if a neighboring cell is a wall
            if (IsWall(cellState.x - 1, cellState.y)) wallCount++; // Check left
            if (IsWall(cellState.x + 1, cellState.y)) wallCount++; // Check right
            if (IsWall(cellState.x, cellState.y - 1)) wallCount++; // Check below
            if (IsWall(cellState.x, cellState.y + 1)) wallCount++; // Check above

            if (wallCount == 3)
            {
                // This is a corner cell
            }
        }
        grid[x, y].CellState = cellState;
    }

    private bool IsWall(int x, int y){
        return grid[x,y].CellState != null && grid[x,y].CellState.isWalkable;
    }

    void AssignHiddenRewards()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y].IsWall)
                {
                    float randVal = UnityEngine.Random.value;
                    CellState cellState = grid[x, y].CellState;

                    if (cellState != null)
                    {
                        if (randVal < hiddenGoodProb)
                        {
                            cellState.hiddenReward = goodReward;
                        }
                        else if (randVal < hiddenGoodProb + hiddenBadProb)
                        {
                            cellState.hiddenReward = badPenalty;
                        }
                        else
                        {
                            cellState.hiddenReward = 0f;
                        }
                    }
                }
            }
        }
    }

    void PlaceCharacters()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(startPosition.x*scaleFactor, 1, startPosition.y*scaleFactor);
        }

        GameObject npc = GameObject.Find("NPC");
        if (npc != null)
        {
            npc.transform.position = new Vector3(endPosition.x*scaleFactor, 1, endPosition.y*scaleFactor);
        }
    }

    void InitializeNodes()
    {
        nodes = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nodes[x, y] = new Node(new Vector2Int(x, y), !grid[x, y].IsWall);
            }
        }
    }
}