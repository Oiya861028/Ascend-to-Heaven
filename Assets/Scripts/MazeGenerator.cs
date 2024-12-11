using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public class Cell
    {
        public bool IsVisited = false;
        public bool IsWall = true;
        public GameObject Instance;
        public bool HasGoodItem = false;
        public bool HasBadItem = false;
    }
    
    // Lab-specific additions
    public GameObject goodItemPrefab;
    public GameObject badItemPrefab;
    public float generationDelay = 0.025f;
    
    // Distribution from lab spec
    public float badProb = 0.2f;
    public float goodProb = 0.2f;

    public int width = 21;
    public int height = 21;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pathMarkerPrefab;
    public Vector2Int startPosition = new Vector2Int(1, 1);
    public Vector2Int endPosition = new Vector2Int(19, 19);

    private Cell[,] grid;
    private List<Vector2Int> wallList;
    public Node[,] nodes;  // Made public so NPCController can access it
    private List<Node> openList;
    private HashSet<Node> closedList;


    void Start()
    {
        InitializeGrid();
        StartCoroutine(GenerateMazeAndPath());
    }

    IEnumerator GenerateMazeAndPath()
    {
        yield return StartCoroutine(GenerateMaze());
        InitializeNodes();
        PlaceItemsInMaze();
        PlaceCharacters();
        // FindPath(startPosition, endPosition);
    }

    void InitializeGrid()
    {
        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell();
                Vector3 position = new Vector3(x, 0, y);
                grid[x, y].Instance = Instantiate(wallPrefab, position, Quaternion.identity, transform);
            }
        }
    }

    void PlaceItemsInMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!grid[x, y].IsWall)
                {
                    float randVal = Random.value;
                    Vector3 itemPosition = new Vector3(x, 0.5f, y);
                    
                    if (randVal < badProb)
                    {
                        GameObject badItem = Instantiate(badItemPrefab, itemPosition, Quaternion.identity);
                        badItem.tag = "BadItem";
                        grid[x, y].HasBadItem = true;
                    }
                    else if (randVal < badProb + goodProb)
                    {
                        GameObject goodItem = Instantiate(goodItemPrefab, itemPosition, Quaternion.identity);
                        goodItem.tag = "GoodItem";
                        grid[x, y].HasGoodItem = true;
                    }
                }
            }
        }
    }

    void PlaceCharacters()
    {
        // Place player at start
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(startPosition.x, 1, startPosition.y);
        }

        // Place NPC at end
        GameObject npc = GameObject.Find("NPC");
        if (npc != null)
        {
            npc.transform.position = new Vector3(endPosition.x, 1, endPosition.y);
        }
    }

    IEnumerator GenerateMaze()
    {
        wallList = new List<Vector2Int>();

        int startX = Random.Range(1, width - 1);
        int startY = Random.Range(1, height - 1);

        startX = startX % 2 == 0 ? startX - 1 : startX;
        startY = startY % 2 == 0 ? startY - 1 : startY;

        SetCell(startX, startY, false);
        yield return new WaitForSeconds(generationDelay);

        AddWallsToList(startX, startY);
        
        while (wallList.Count > 0)
        {
            int randomIndex = Random.Range(0, wallList.Count);
            Vector2Int wall = wallList[randomIndex];
            wallList.RemoveAt(randomIndex);

            yield return StartCoroutine(ProcessWall(wall));
        }
    }

    void AddWallsToList(int x, int y)
    {
        if (x - 2 > 0) 
            wallList.Add(new Vector2Int(x - 1, y));

        if (x + 2 < width - 1) 
            wallList.Add(new Vector2Int(x + 1, y));

        if (y - 2 > 0)
            wallList.Add(new Vector2Int(x, y - 1));

        if (y + 2 < height - 1)
            wallList.Add(new Vector2Int(x, y + 1));
    }

    IEnumerator ProcessWall(Vector2Int wall)
    {
        int x = wall.x;
        int y = wall.y;

        List<Vector2Int> neighbors = GetNeighbors(x, y);

        if (neighbors.Count == 2)
        {
            Cell cell1 = grid[neighbors[0].x, neighbors[0].y];
            Cell cell2 = grid[neighbors[1].x, neighbors[1].y];

            if (cell1.IsVisited != cell2.IsVisited)
            {
                SetCell(x, y, false);
                yield return new WaitForSeconds(generationDelay);

                if (!cell1.IsVisited)
                {
                    SetCell(neighbors[0].x, neighbors[0].y, false);
                    yield return new WaitForSeconds(generationDelay);
                    AddWallsToList(neighbors[0].x, neighbors[0].y);
                }
                else
                {
                    SetCell(neighbors[1].x, neighbors[1].y, false);
                    yield return new WaitForSeconds(generationDelay);
                    AddWallsToList(neighbors[1].x, neighbors[1].y);
                }
            }
        }
    }

    void SetCell(int x, int y, bool isWall)
    {
        Destroy(grid[x, y].Instance);
        grid[x, y].IsWall = isWall;
        grid[x, y].IsVisited = true;
        Vector3 position = new Vector3(x, 0, y);
        grid[x, y].Instance = Instantiate(isWall ? wallPrefab : floorPrefab, position, Quaternion.identity, transform);
    }

    List<Vector2Int> GetNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (x % 2 == 1)
        {
            if (y - 1 >= 0) neighbors.Add(new Vector2Int(x, y - 1));
            if (y + 1 < height) neighbors.Add(new Vector2Int(x, y + 1));
        }
        else if (y % 2 == 1)
        {
            if (x - 1 >= 0) neighbors.Add(new Vector2Int(x - 1, y));
            if (x + 1 < width) neighbors.Add(new Vector2Int(x + 1, y));
        }

        return neighbors;
    }

    // A* Pathfinding Implementation
    void InitializeNodes()
    {
        nodes = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isWalkable = !grid[x, y].IsWall;
                nodes[x, y] = new Node(new Vector2Int(x, y), isWalkable);
            }
        }
    }

    void FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        Node startNode = nodes[startPos.x, startPos.y];
        Node endNode = nodes[endPos.x, endPos.y];

        openList = new List<Node> { startNode };
        closedList = new HashSet<Node>();

        startNode.GCost = 0;
        startNode.HCost = GetHeuristic(startNode, endNode);

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedList.Contains(neighbor))
                    continue;

                int tentativeGCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (tentativeGCost < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetHeuristic(neighbor, endNode);
                    neighbor.ParentNode = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
    }

    int GetHeuristic(Node a, Node b)
    {
        int dx = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
        int dy = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
        return dx + dy;
    }

    int GetDistance(Node a, Node b)
    {
        return 1;
    }

    Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];

        foreach (Node node in nodeList)
        {
            if (node.FCost < lowestFCostNode.FCost ||
                (node.FCost == lowestFCostNode.FCost && node.HCost < lowestFCostNode.HCost))
            {
                lowestFCostNode = node;
            }
        }

        return lowestFCostNode;
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        int x = node.GridPosition.x;
        int y = node.GridPosition.y;

        if (x - 1 >= 0) neighbors.Add(nodes[x - 1, y]);
        if (x + 1 < width) neighbors.Add(nodes[x + 1, y]);
        if (y - 1 >= 0) neighbors.Add(nodes[x, y - 1]);
        if (y + 1 < height) neighbors.Add(nodes[x, y + 1]);

        return neighbors;
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.ParentNode;
        }

        path.Reverse();
        StartCoroutine(DrawPath(path));
    }

    IEnumerator DrawPath(List<Node> path)
    {
        foreach (Node node in path)
        {
            Vector3 position = new Vector3(node.GridPosition.x, 0.5f, node.GridPosition.y);
            GameObject marker = Instantiate(pathMarkerPrefab, position, Quaternion.identity);
            yield return new WaitForSeconds(0.05f);
        }
    }
}