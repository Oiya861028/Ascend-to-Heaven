using UnityEngine;
using System.Collections.Generic;

using UnityEngine;
using System.Collections.Generic;

public class AgentController : MonoBehaviour
{
    public float speed = 3f;
    public float senseInterval = 0.5f;  // Sense more frequently
    public float senseRadius = 2f;      // Sense a bit further
    public float explorationWeight = 0.3f; // How much to value exploring vs exploiting
    
    private Vector3 targetPosition;
    private MazeGenerator mazeGenerator;
    private float updatePathTimer = 0f;
    private float updatePathInterval = 1f;
    private int score = 0;
    private float senseTimer = 0f;

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentWaypointIndex = 0;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, float> knownRewards = new Dictionary<Vector2Int, float>();

    public int GetScore() { return score; }

    void Start()
    {
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
        Vector2Int startPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        visitedCells.Add(startPos);
    }

    void Update()
    {
        if (mazeGenerator == null || mazeGenerator.nodes == null) return;

        // Regular sensing
        senseTimer += Time.deltaTime;
        if (senseTimer >= senseInterval)
        {
            SenseNearbyTiles();
            senseTimer = 0f;
        }

        // Path updating
        updatePathTimer += Time.deltaTime;
        if (updatePathTimer >= updatePathInterval)
        {
            DecideNextMove();
            updatePathTimer = 0f;
        }

        // Movement
        FollowPath();
    }

    void SenseNearbyTiles()
    {
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentY = Mathf.RoundToInt(transform.position.z);

        for (int xOffset = -2; xOffset <= 2; xOffset++)
        {
            for (int yOffset = -2; yOffset <= 2; yOffset++)
            {
                int checkX = currentX + xOffset;
                int checkY = currentY + yOffset;

                if (checkX < 0 || checkX >= mazeGenerator.width || 
                    checkY < 0 || checkY >= mazeGenerator.height)
                    continue;

                float distance = Vector2.Distance(
                    new Vector2(currentX, currentY),
                    new Vector2(checkX, checkY)
                );

                if (distance <= senseRadius)
                {
                    CellState cellState = mazeGenerator.grid[checkX, checkY].CellState;
                    if (cellState != null && !cellState.isRevealed)
                    {
                        cellState.Reveal();
                        Vector2Int pos = new Vector2Int(checkX, checkY);
                        knownRewards[pos] = cellState.hiddenReward;
                        
                        Debug.Log($"AI sensed reward {cellState.hiddenReward} at ({checkX}, {checkY})");
                    }
                }
            }
        }
    }

    void DecideNextMove()
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // Find best known reward within reasonable distance
        Vector2Int bestTarget = currentPos;
        float bestValue = float.MinValue;

        foreach (var rewardInfo in knownRewards)
        {
            Vector2Int pos = rewardInfo.Key;
            float reward = rewardInfo.Value;
            
            // Skip if we've already visited this cell
            if (visitedCells.Contains(pos)) continue;

            // Calculate distance-adjusted value
            float distance = Vector2.Distance(currentPos, pos);
            float distanceWeight = 1f / (1f + distance); // Closer tiles are worth more
            
            // Add exploration bonus for unvisited areas
            float explorationBonus = !visitedCells.Contains(pos) ? explorationWeight : 0f;
            
            // Calculate total value considering reward, distance, and exploration
            float value = (reward * distanceWeight) + explorationBonus;

            if (value > bestValue && IsValidPosition(pos.x, pos.y))
            {
                bestValue = value;
                bestTarget = pos;
                Debug.Log($"Found better target at ({pos.x}, {pos.y}) with value {value}");
            }
        }

        // If we didn't find a good known reward, explore!
        if (bestTarget == currentPos)
        {
            bestTarget = FindExplorationTarget(currentPos);
            Debug.Log("No good rewards found, exploring new area");
        }

        // Set new path
        if (bestTarget != currentPos)
        {
            FindPath(currentPos, bestTarget);
            Debug.Log($"Setting new path to ({bestTarget.x}, {bestTarget.y})");
        }
    }

    Vector2Int FindExplorationTarget(Vector2Int currentPos)
    {
        // Find the nearest unvisited cell
        int searchRadius = 1;
        int maxSearchRadius = Mathf.Max(mazeGenerator.width, mazeGenerator.height);

        while (searchRadius < maxSearchRadius)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2Int checkPos = new Vector2Int(
                        currentPos.x + x,
                        currentPos.y + y
                    );

                    if (IsValidPosition(checkPos.x, checkPos.y) && 
                        !visitedCells.Contains(checkPos))
                    {
                        candidates.Add(checkPos);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }

            searchRadius++;
        }

        // If all cells are visited, pick a random valid position
        return new Vector2Int(
            Random.Range(0, mazeGenerator.width),
            Random.Range(0, mazeGenerator.height)
        );
    }

    void FollowPath()
    {
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
                // Mark cell as visited when we reach it
                visitedCells.Add(new Vector2Int(
                    Mathf.RoundToInt(currentWaypoint.x),
                    Mathf.RoundToInt(currentWaypoint.z)
                ));
                currentWaypointIndex++;
            }
        }
    }

    bool IsValidPosition(int x, int y)
    {
        if (mazeGenerator == null || mazeGenerator.nodes == null) return false;
        if (x < 0 || x >= mazeGenerator.width || y < 0 || y >= mazeGenerator.height) return false;
        if (mazeGenerator.nodes[x, y] == null) return false;
        return mazeGenerator.nodes[x, y].IsWalkable;
    }
    void UpdateTargetPosition()
    {
        try
        {
            if (mazeGenerator == null || mazeGenerator.nodes == null) return;

            int currentX = Mathf.RoundToInt(transform.position.x);
            int currentY = Mathf.RoundToInt(transform.position.z);
            
            currentX = Mathf.Clamp(currentX, 0, mazeGenerator.nodes.GetLength(0) - 1);
            currentY = Mathf.Clamp(currentY, 0, mazeGenerator.nodes.GetLength(1) - 1);
            
            Vector2Int currentPos = new Vector2Int(currentX, currentY);
            
            // Try to find a valid target position with positive reward if revealed
            int attempts = 0;
            Vector2Int targetPos = currentPos;
            bool foundValid = false;
            float bestReward = float.MinValue;
            int maxAttempts = 10;
            while (!foundValid && attempts < maxAttempts)
            {
                attempts++;
                int x = Random.Range(0, mazeGenerator.width);
                int y = Random.Range(0, mazeGenerator.height);
                
                if (IsValidPosition(x, y))
                {
                    CellState cellState = mazeGenerator.grid[x, y].CellState;
                    if (cellState != null && cellState.isRevealed)
                    {
                        // Prefer moving to revealed positive reward cells
                        if (cellState.hiddenReward > bestReward)
                        {
                            targetPos = new Vector2Int(x, y);
                            bestReward = cellState.hiddenReward;
                            foundValid = true;
                        }
                    }
                    else if (!foundValid)
                    {
                        // If we haven't found any better options, use this as fallback
                        targetPos = new Vector2Int(x, y);
                        foundValid = true;
                    }
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
        // Get current cell state
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentY = Mathf.RoundToInt(transform.position.z);
        
        if (currentX >= 0 && currentX < mazeGenerator.width &&
            currentY >= 0 && currentY < mazeGenerator.height)
        {
            CellState cellState = mazeGenerator.grid[currentX, currentY].CellState;
            if (cellState != null && cellState.isRevealed)
            {
                score += (int)cellState.hiddenReward;
                Debug.Log($"NPC score changed by {cellState.hiddenReward}. New score: {score}");
            }
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