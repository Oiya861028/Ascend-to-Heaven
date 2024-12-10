using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    private class Cell
    {
        public bool IsVisited = false;
        public bool IsWall = true;
        public GameObject Instance;
    }
    
    public class Node
    {
        public Vector2Int GridPosition;
        public bool IsWalkable;
        public int GCost; // Cost from start node
        public int HCost; // Heuristic cost to end node
        public int FCost => GCost + HCost; // Total cost
        public Node ParentNode;

        public Node(Vector2Int gridPosition, bool isWalkable)
        {
            GridPosition = gridPosition;
            IsWalkable = isWalkable;
        }
    }


    public int width = 21;
    public int height = 21;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pathMarkerPrefab;
    public float generationDelay = 0.025f;
    public Vector2Int startPosition = new Vector2Int(1, 1);
    public Vector2Int endPosition = new Vector2Int(19, 19);

    private Cell[,] grid;
    private List<Vector2Int> wallList;
    private Node[,] nodes;
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
        FindPath(startPosition, endPosition);
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

    IEnumerator GenerateMaze()
    {
        wallList = new List<Vector2Int>();

        // choose a random starting cell
        int startX = Random.Range(1, width - 1);
        int startY = Random.Range(1, height - 1);

        // make sure starting on an odd coordinate
        startX = startX % 2 == 0 ? startX - 1 : startX;
        startY = startY % 2 == 0 ? startY - 1 : startY;

        // mark the starting cell as a passage
        SetCell(startX, startY, false);
        yield return new WaitForSeconds(generationDelay);

        // add the neighboring walls to the wall list
        AddWallsToList(startX, startY);
        
        // main loop of the algorithm
        while (wallList.Count > 0)
        {
            // select a random wall from the list
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

        // Determine the cells on either side of the wall
        List<Vector2Int> neighbors = GetNeighbors(x, y);

        if (neighbors.Count == 2)
        {
            Cell cell1 = grid[neighbors[0].x, neighbors[0].y];
            Cell cell2 = grid[neighbors[1].x, neighbors[1].y];

            // If one of the cells is unvisited
            if (cell1.IsVisited != cell2.IsVisited)
            {
                // Make the wall a passage
                SetCell(x, y, false);
                yield return new WaitForSeconds(generationDelay);

                // Mark the unvisited cell as visited
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
            // Vertical wall
            if (y - 1 >= 0) neighbors.Add(new Vector2Int(x, y - 1));
            if (y + 1 < height) neighbors.Add(new Vector2Int(x, y + 1));
        }
        else if (y % 2 == 1)
        {
            // Horizontal wall
            if (x - 1 >= 0) neighbors.Add(new Vector2Int(x - 1, y));
            if (x + 1 < width) neighbors.Add(new Vector2Int(x + 1, y));
        }

        return neighbors;
    }
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
                // Path found
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
        // Using Manhattan Distance as the heuristic
        int dx = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
        int dy = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
        return dx + dy;
    }
    int GetDistance(Node a, Node b)
    {
        // Since movement is only in four directions, the cost is always 1
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

        if (x - 1 >= 0) neighbors.Add(nodes[x - 1, y]); // Left
        if (x + 1 < width) neighbors.Add(nodes[x + 1, y]); // Right
        if (y - 1 >= 0) neighbors.Add(nodes[x, y - 1]); // Down
        if (y + 1 < height) neighbors.Add(nodes[x, y + 1]); // Up

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

        // Visualize the path
        StartCoroutine(DrawPath(path));
    }
    System.Collections.IEnumerator DrawPath(List<Node> path)
    {
        foreach (Node node in path)
        {
            Vector3 position = new Vector3(node.GridPosition.x, 0.5f, node.GridPosition.y);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.5f;
            sphere.GetComponent<Renderer>().material.color = Color.red;
            yield return new WaitForSeconds(0.05f); // For visual effect
        }
    }

    
}