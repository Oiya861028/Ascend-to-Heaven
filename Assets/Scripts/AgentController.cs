using UnityEngine;
using System.Collections.Generic;

public class AgentController : MonoBehaviour
{
    public float speed = 3f;
    private Vector3 targetPosition;
    private MazeGenerator mazeGenerator;
    private float updatePathTimer = 0f;
    private float updatePathInterval = 2f;
    private int score = 0;
    private int maxAttempts = 50;

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentWaypointIndex = 0;

    void Start()
    {
        // Wait a bit longer for maze generation
        Invoke("Initialize", 1f);
    }

    void Initialize()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();
        if (mazeGenerator == null)
        {
            Debug.LogError("No MazeGenerator found!");
            return;
        }
        targetPosition = transform.position;
        Debug.Log($"NPC initialized at position: {transform.position}");
    }

    void Update()
    {
        if (mazeGenerator == null || mazeGenerator.nodes == null) return;

        updatePathTimer += Time.deltaTime;
        if (updatePathTimer >= updatePathInterval)
        {
            UpdateTargetPosition();
            updatePathTimer = 0f;
        }

        // Follow the current path
        if (currentPath != null && currentPath.Count > 0 && currentWaypointIndex < currentPath.Count)
        {
            Vector3 currentWaypoint = currentPath[currentWaypointIndex];
            
            if (Vector3.Distance(transform.position, currentWaypoint) > 0.1f)
            {
                Vector3 direction = (currentWaypoint - transform.position).normalized;
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
                
                if (direction != Vector3.zero)
                {
                    Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10f * Time.deltaTime);
                }
            }
            else
            {
                currentWaypointIndex++;
            }
        }
    }

    void UpdateTargetPosition()
    {
        try
        {
            if (mazeGenerator == null || mazeGenerator.nodes == null) return;

            // Get current position and make sure we're in bounds
            int currentX = Mathf.RoundToInt(transform.position.x);
            int currentY = Mathf.RoundToInt(transform.position.z);  // Using z for the y-coordinate since we're in 3D
            
            // Clamp values and add debug log
            currentX = Mathf.Clamp(currentX, 0, mazeGenerator.nodes.GetLength(0) - 1);
            currentY = Mathf.Clamp(currentY, 0, mazeGenerator.nodes.GetLength(1) - 1);
            
            Vector2Int currentPos = new Vector2Int(currentX, currentY);
            
            Debug.Log($"NPC world pos: {transform.position}, Grid pos: {currentPos}, Maze bounds: {mazeGenerator.nodes.GetLength(0)}x{mazeGenerator.nodes.GetLength(1)}");

            // Try to find a valid target position
            int attempts = 0;
            Vector2Int targetPos = currentPos;
            bool foundValid = false;

            while (!foundValid && attempts < maxAttempts)
            {
                attempts++;
                int x = Random.Range(0, mazeGenerator.width);
                int y = Random.Range(0, mazeGenerator.height);
                
                if (IsValidPosition(x, y))
                {
                    targetPos = new Vector2Int(x, y);
                    foundValid = true;
                    Debug.Log($"Found valid target position: {targetPos}");
                }
            }

            if (foundValid)
            {
                FindPath(currentPos, targetPos);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateTargetPosition: {e.Message}");
        }
    }

    bool IsValidPosition(int x, int y)
    {
        try
        {
            if (mazeGenerator == null || mazeGenerator.nodes == null) return false;
            if (x < 0 || x >= mazeGenerator.width || y < 0 || y >= mazeGenerator.height) return false;
            if (mazeGenerator.nodes[x, y] == null) return false;
            return mazeGenerator.nodes[x, y].IsWalkable;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error checking valid position ({x}, {y}): {e.Message}");
            return false;
        }
    }

    void FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        try
        {
            if (!IsValidPosition(startPos.x, startPos.y) || !IsValidPosition(endPos.x, endPos.y))
            {
                Debug.LogWarning("Invalid start or end position for pathfinding");
                return;
            }

            Debug.Log($"Attempting to find path from ({startPos.x}, {startPos.y}) to ({endPos.x}, {endPos.y})");
            Debug.Log($"Maze dimensions: {mazeGenerator.width} x {mazeGenerator.height}");
            Debug.Log($"Nodes array dimensions: {mazeGenerator.nodes.GetLength(0)} x {mazeGenerator.nodes.GetLength(1)}");
            
            if (startPos.x >= mazeGenerator.nodes.GetLength(0) || startPos.y >= mazeGenerator.nodes.GetLength(1))
            {
                Debug.LogError($"Start position {startPos} is out of bounds!");
                return;
            }
            
            if (endPos.x >= mazeGenerator.nodes.GetLength(0) || endPos.y >= mazeGenerator.nodes.GetLength(1))
            {
                Debug.LogError($"End position {endPos} is out of bounds!");
                return;
            }
            
            Node startNode = mazeGenerator.nodes[startPos.x, startPos.y];
            Node endNode = mazeGenerator.nodes[endPos.x, endPos.y];

            if (startNode == null || endNode == null)
            {
                Debug.LogWarning("Start or end node is null");
                return;
            }

            List<Node> openList = new List<Node>();
            HashSet<Node> closedList = new HashSet<Node>();

            // Reset nodes
            for (int x = 0; x < mazeGenerator.width; x++)
            {
                for (int y = 0; y < mazeGenerator.height; y++)
                {
                    if (mazeGenerator.nodes[x, y] != null)
                    {
                        mazeGenerator.nodes[x, y].GCost = int.MaxValue;
                        mazeGenerator.nodes[x, y].HCost = 0;
                        mazeGenerator.nodes[x, y].ParentNode = null;
                    }
                }
            }

            openList.Add(startNode);
            startNode.GCost = 0;
            startNode.HCost = CalculateHCost(startPos, endPos);

            while (openList.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(openList);
                
                if (currentNode.GridPosition == endNode.GridPosition)
                {
                    RetracePath(startNode, endNode);
                    return;
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (Node neighbor in GetNeighbors(currentNode))
                {
                    if (neighbor == null || !neighbor.IsWalkable || closedList.Contains(neighbor))
                        continue;

                    int tentativeGCost = currentNode.GCost + 1;

                    if (tentativeGCost < neighbor.GCost || !openList.Contains(neighbor))
                    {
                        neighbor.GCost = tentativeGCost;
                        neighbor.HCost = CalculateHCost(neighbor.GridPosition, endPos);
                        neighbor.ParentNode = currentNode;

                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in FindPath: {e.Message}");
        }
    }

    private int CalculateHCost(Vector2Int pos, Vector2Int target)
    {
        return Mathf.Abs(pos.x - target.x) + Mathf.Abs(pos.y - target.y);
    }

    private Node GetLowestFCostNode(List<Node> nodeList)
    {
        if (nodeList == null || nodeList.Count == 0) return null;
        
        Node lowestNode = nodeList[0];
        
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i] != null && lowestNode != null)
            {
                if (nodeList[i].FCost < lowestNode.FCost || 
                    (nodeList[i].FCost == lowestNode.FCost && nodeList[i].HCost < lowestNode.HCost))
                {
                    lowestNode = nodeList[i];
                }
            }
        }
        
        return lowestNode;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        if (node == null) return neighbors;

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.GridPosition.x + dx[i];
            int checkY = node.GridPosition.y + dy[i];

            if (IsValidPosition(checkX, checkY))
            {
                neighbors.Add(mazeGenerator.nodes[checkX, checkY]);
            }
        }

        return neighbors;
    }

    void RetracePath(Node startNode, Node endNode)
    {
        try
        {
            if (startNode == null || endNode == null) return;

            currentPath.Clear();
            Node currentNode = endNode;

            while (currentNode != startNode && currentNode != null)
            {
                currentPath.Add(new Vector3(currentNode.GridPosition.x, 1f, currentNode.GridPosition.y));
                currentNode = currentNode.ParentNode;
            }

            if (currentNode != null)  // Add start node position
            {
                currentPath.Add(new Vector3(startNode.GridPosition.x, 1f, startNode.GridPosition.y));
            }

            currentPath.Reverse();
            currentWaypointIndex = 0;

            Debug.Log($"Path found with {currentPath.Count} waypoints");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in RetracePath: {e.Message}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoodItem"))
        {
            score += 10;
            Debug.Log($"NPC collected a good item! Score: {score}");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("BadItem"))
        {
            score -= 10;
            Debug.Log($"NPC hit a bad item! Score: {score}");
            Destroy(other.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = currentWaypointIndex; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }
}