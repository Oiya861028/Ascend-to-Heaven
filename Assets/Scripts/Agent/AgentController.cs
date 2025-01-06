using UnityEngine;
using System.Collections.Generic;

public class AgentController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Movement Settings")]
    public float speed = 3f;
    public float viewAngleThreshold = 60f;
    public float checkInterval = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool isBeingViewed;
    
    [Header("Sensing Settings")]
    public float senseInterval = 0.5f;
    public float senseRadius = 2f;
    
    [Header("Score")]
    public int score = 0;

    [Header("Sound Detection")]
    public Vector2Int? soundLocation = null;
    public float maxSoundRadius = 7f;

    private float senseTimer = 0f;
    private float lastCheckTime;
    private MazeGenerator mazeGenerator;
    private HashSet<Vector2Int> visitedCells = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, float> knownRewards = new Dictionary<Vector2Int, float>();
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int currentPathIndex = 0;
    private Vector2Int currentNode;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();
        if (!mazeGenerator || !playerCamera)
        {
            Debug.LogError("Missing references!");
            return;
        }

        currentNode = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / mazeGenerator.scaleFactor),
            Mathf.RoundToInt(transform.position.z / mazeGenerator.scaleFactor)
        );
        visitedCells.Add(currentNode);
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        
        for (int x = 0; x < mazeGenerator.width; x++)
        {
            for (int y = 0; y < mazeGenerator.height; y++)
            {
                mazeGenerator.nodes[x,y].GCost = int.MaxValue;
                mazeGenerator.nodes[x,y].HCost = 0;
                mazeGenerator.nodes[x,y].ParentNode = null;
            }
        }

        Node startNode = mazeGenerator.nodes[start.x, start.y];
        startNode.GCost = 0;
        startNode.HCost = GetHeuristic(start, target);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = GetLowestFCostNode(openSet);
            if (current.GridPosition == target)
            {
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            closedSet.Add(current.GridPosition);

            foreach (Vector2Int neighborPos in GetValidNeighbors(current.GridPosition))
            {
                if (closedSet.Contains(neighborPos)) continue;

                Node neighbor = mazeGenerator.nodes[neighborPos.x, neighborPos.y];
                int tentativeGCost = current.GCost + 1;

                if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.ParentNode = current;
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetHeuristic(neighborPos, target);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<Vector2Int>();
    }

    List<Vector2Int> GetValidNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };

        foreach (var dir in directions)
        {
            Vector2Int newPos = pos + dir;
            if (IsValidPosition(newPos) && mazeGenerator.nodes[newPos.x, newPos.y].IsWalkable)
            {
                neighbors.Add(newPos);
            }
        }
        return neighbors;
    }

    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mazeGenerator.width &&
               pos.y >= 0 && pos.y < mazeGenerator.height;
    }

    int GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    Node GetLowestFCostNode(List<Node> nodes)
    {
        Node lowest = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].FCost < lowest.FCost) lowest = nodes[i];
        }
        return lowest;
    }

    List<Vector2Int> ReconstructPath(Node endNode)
    {
        var path = new List<Vector2Int>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.GridPosition);
            current = current.ParentNode;
        }
        path.Reverse();
        return path;
    }

    bool IsBeingViewedByCamera()
    {
        if (playerCamera == null) return false;

        Vector3 directionToCamera = playerCamera.transform.position - transform.position;
        float distanceToCamera = directionToCamera.magnitude;
        Vector3 cameraToAgent = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, cameraToAgent);
        bool inFOV = angle < viewAngleThreshold;

        if (showDebugInfo)
        {
            Debug.Log($"Angle to agent: {angle}, FOV Threshold: {viewAngleThreshold}, In FOV: {inFOV}");
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 10f, Color.blue);
            Debug.DrawLine(playerCamera.transform.position, transform.position, inFOV ? Color.green : Color.red);
        }

        if (inFOV)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, cameraToAgent, out hit, distanceToCamera))
            {
                bool canSeeAgent = hit.collider.gameObject == gameObject;
                if (showDebugInfo)
                {
                    Debug.Log($"Hit object: {hit.collider.gameObject.name}, Can see agent: {canSeeAgent}");
                }
                return canSeeAgent;
            }
        }

        return false;
    }

    private Vector2Int GetRandomPointAroundSound(float radius)
    {
        List<Vector2Int> validPoints = new List<Vector2Int>();
        int roundedRadius = Mathf.RoundToInt(radius);

        for (int x = -roundedRadius; x <= roundedRadius; x++)
        {
            for (int y = -roundedRadius; y <= roundedRadius; y++)
            {
                Vector2Int checkPoint = new Vector2Int(
                    soundLocation.Value.x + x,
                    soundLocation.Value.y + y
                );

                if (IsValidPosition(checkPoint) && 
                    mazeGenerator.nodes[checkPoint.x, checkPoint.y].IsWalkable &&
                    Vector2Int.Distance(checkPoint, soundLocation.Value) <= radius)
                {
                    validPoints.Add(checkPoint);
                }
            }
        }

        return validPoints.Count > 0 
            ? validPoints[Random.Range(0, validPoints.Count)] 
            : soundLocation.Value;
    }

    private Vector2Int? lastSoundLocation = null;
    private Vector2Int? currentTarget = null;
    
    void Update()
    {
        if (!mazeGenerator) return;

        isBeingViewed = IsBeingViewedByCamera();
        if (isBeingViewed) return;

        // Only update path if sound location has changed
        if (soundLocation != lastSoundLocation)
        {
            lastSoundLocation = soundLocation;
            
            if (soundLocation.HasValue)
            {
                int distanceToSound = Mathf.Abs(currentNode.x - soundLocation.Value.x) + 
                                    Mathf.Abs(currentNode.y - soundLocation.Value.y);
                
                if (distanceToSound <= 1)
                {
                    currentTarget = soundLocation.Value;
                }
                else
                {
                    float radius = (distanceToSound / maxSoundRadius) * maxSoundRadius;
                    currentTarget = GetRandomPointAroundSound(radius);
                }

                currentPath = FindPath(currentNode, currentTarget.Value);
                currentPathIndex = 0;
            }
        }

        // Continue moving if we have a path
        if (currentPath.Count > 0 && currentPathIndex < currentPath.Count)
        {
            MoveAlongPath();
        }

        senseTimer += Time.deltaTime;
        if (senseTimer >= senseInterval)
        {
            SenseNearbyTiles();
            senseTimer = 0f;
        }
    }

    void MoveAlongPath()
    {
        Vector3 targetPosition = new Vector3(
            currentPath[currentPathIndex].x * mazeGenerator.scaleFactor,
            transform.position.y,
            currentPath[currentPathIndex].y * mazeGenerator.scaleFactor
        );

        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.position += moveDirection * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentNode = currentPath[currentPathIndex];
            currentPathIndex++;
        }

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                10f * Time.deltaTime
            );
        }
    }

    void SenseNearbyTiles()
    {
        int currentX = Mathf.RoundToInt(transform.position.x / mazeGenerator.scaleFactor);
        int currentY = Mathf.RoundToInt(transform.position.z / mazeGenerator.scaleFactor);

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
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        int currentX = Mathf.RoundToInt(transform.position.x / mazeGenerator.scaleFactor);
        int currentY = Mathf.RoundToInt(transform.position.z / mazeGenerator.scaleFactor);
        
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
        if (playerCamera != null && showDebugInfo)
        {
            Vector3 forward = playerCamera.transform.forward;
            Vector3 origin = playerCamera.transform.position;
            float radius = 5f;

            Vector3 rightEdge = Quaternion.Euler(0, viewAngleThreshold, 0) * forward;
            Vector3 leftEdge = Quaternion.Euler(0, -viewAngleThreshold, 0) * forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + rightEdge * radius);
            Gizmos.DrawLine(origin, origin + leftEdge * radius);

            if (currentPath != null && currentPath.Count > 0)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < currentPath.Count - 1; i++)
                {
                    Vector3 start = new Vector3(currentPath[i].x * mazeGenerator.scaleFactor, 1, currentPath[i].y * mazeGenerator.scaleFactor);
                    Vector3 end = new Vector3(currentPath[i + 1].x * mazeGenerator.scaleFactor, 1, currentPath[i + 1].y * mazeGenerator.scaleFactor);
                    Gizmos.DrawLine(start, end);
                }
            }

            if (soundLocation.HasValue)
            {
                Gizmos.color = Color.red;
                Vector3 soundPos = new Vector3(
                    soundLocation.Value.x * mazeGenerator.scaleFactor,
                    1,
                    soundLocation.Value.y * mazeGenerator.scaleFactor
                );
                Gizmos.DrawWireSphere(soundPos, maxSoundRadius * mazeGenerator.scaleFactor);
            }
        }
    }

    public int GetScore() { return score; }
}