using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AgentController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Movement Settings")]
    public float speed = 3f;
    public float rotationSpeed = 10f;
    public float checkInterval = 0.1f;
    
    [Header("Detection Settings")]
    public float viewAngleThreshold = 60f;
    public float eyeHeightOffset = 2f;
    public bool showDebugInfo = true;
    
    [Header("Sound Detection")]
    public float maxSoundRadius = 7f;
    
    [Header("State Info")]
    public bool isBeingViewed;
    public int score = 0;
    public Vector2Int? soundLocation = null;

    private float lastCheckTime;
    private MazeGenerator mazeGenerator;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int currentPathIndex = 0;
    private Vector2Int currentNode;
    private Vector2Int? lastSoundLocation = null;
    private Vector2Int? currentTarget = null;
    private AgentState previousState;

    private enum AgentState
    {
        Frozen,
        Chasing,
        Investigating,
        Patrolling
    }
    private AgentState currentState = AgentState.Patrolling;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        mazeGenerator = FindFirstObjectByType<MazeGenerator>();
        if (!mazeGenerator || !playerCamera)
        {
            Debug.LogError("Missing required references: MazeGenerator or PlayerCamera!");
            enabled = false;
            return;
        }

        // Find a random spawn point far from start (assuming start is at 0,0)
        Vector2Int spawnPoint = FindFarSpawnPoint();
        
        // Set position to spawn point
        transform.position = new Vector3(
            spawnPoint.x * mazeGenerator.scaleFactor,
            transform.position.y,
            spawnPoint.y * mazeGenerator.scaleFactor
        );

        currentNode = spawnPoint;
        previousState = AgentState.Patrolling;
        
        if (showDebugInfo) Debug.Log($"Agent spawned at: {spawnPoint}");
    }

    private Vector2Int FindFarSpawnPoint()
    {
        List<Vector2Int> validSpawnPoints = new List<Vector2Int>();
        float minDistanceFromStart = Mathf.Min(mazeGenerator.width, mazeGenerator.height) * 0.75f; // At least 75% of maze size away

        for (int x = 0; x < mazeGenerator.width; x++)
        {
            for (int y = 0; y < mazeGenerator.height; y++)
            {
                Vector2Int point = new Vector2Int(x, y);
                float distanceFromStart = Vector2Int.Distance(point, Vector2Int.zero);

                if (distanceFromStart >= minDistanceFromStart && 
                    mazeGenerator.nodes[x, y].IsWalkable)
                {
                    validSpawnPoints.Add(point);
                }
            }
        }

        // If no points found meeting the distance criteria, fall back to any walkable point in the furthest third
        if (validSpawnPoints.Count == 0)
        {
            float fallbackDistance = Mathf.Min(mazeGenerator.width, mazeGenerator.height) * 0.6f;
            for (int x = 0; x < mazeGenerator.width; x++)
            {
                for (int y = 0; y < mazeGenerator.height; y++)
                {
                    Vector2Int point = new Vector2Int(x, y);
                    float distanceFromStart = Vector2Int.Distance(point, Vector2Int.zero);

                    if (distanceFromStart >= fallbackDistance && 
                        mazeGenerator.nodes[x, y].IsWalkable)
                    {
                        validSpawnPoints.Add(point);
                    }
                }
            }
        }

        // If still no valid points (very small maze?), just find any walkable point
        if (validSpawnPoints.Count == 0)
        {
            for (int x = 0; x < mazeGenerator.width; x++)
            {
                for (int y = 0; y < mazeGenerator.height; y++)
                {
                    if (mazeGenerator.nodes[x, y].IsWalkable)
                    {
                        validSpawnPoints.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        if (validSpawnPoints.Count == 0)
        {
            Debug.LogError("No valid spawn points found!");
            return Vector2Int.zero;
        }

        return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
    }

    void Update()
    {
        if (!mazeGenerator || !playerCamera) return;

        UpdateStateCheck();
        UpdateMovement();
        checkIfCaughtPlayer();
    }
    //Check if Agent has caught the player
    private void checkIfCaughtPlayer()
    {
        if (Vector3.Distance(transform.position, playerCamera.transform.position) < 1.5f)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("LosingScene");
        }
    }
    

    void UpdateStateCheck()
    {
        if (Time.time - lastCheckTime < checkInterval && currentState != AgentState.Chasing) 
            return;
        
        lastCheckTime = Time.time;
        bool wasBeingViewed = isBeingViewed;
        isBeingViewed = IsBeingViewedByCamera();

        // Handle state transitions
        if (isBeingViewed)
        {
            if (currentState != AgentState.Frozen)
            {
                previousState = currentState; // Store the state we're transitioning from
            }
            currentState = AgentState.Frozen;
            if (showDebugInfo) Debug.Log("State: FROZEN - Being viewed by player");
            return;
        }
        else if (wasBeingViewed && !isBeingViewed)
        {
            // We just stopped being viewed - resume previous state
            currentState = previousState;
            if (showDebugInfo) Debug.Log($"State: RESUMING {previousState} - No longer being viewed");
        }

        // Only update state if we're not frozen
        if (currentState != AgentState.Frozen)
        {
            if (CanSeePlayer())
            {
                currentState = AgentState.Chasing;
                currentPath.Clear();
                if (showDebugInfo) Debug.Log("State: CHASING - Moving directly to player");
            }
            else if (soundLocation != lastSoundLocation)
            {
                currentState = AgentState.Investigating;
                UpdateSoundInvestigation();
                if (showDebugInfo) Debug.Log("State: INVESTIGATING - Following sound");
            }
            else if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
            {
                currentState = AgentState.Patrolling;
                // TODO: Add patrol point selection logic
                if (showDebugInfo) Debug.Log("State: PATROLLING - Following patrol path");
            }
        }
    }

    bool CanSeePlayer()
    {
        if (!playerCamera) return false;

        Vector3 rayStart = transform.position + Vector3.up * eyeHeightOffset;
        Vector3 playerPosition = playerCamera.transform.position + Vector3.up;
        Vector3 directionToPlayer = playerPosition - rayStart;
        
        RaycastHit hit;
        if (!Physics.Raycast(rayStart, directionToPlayer.normalized, out hit, directionToPlayer.magnitude))
        {
            if (showDebugInfo) Debug.DrawLine(rayStart, playerPosition, Color.yellow);
            return true;
        }

        if (showDebugInfo) Debug.DrawLine(rayStart, hit.point, Color.red);
        return false;
    }

    void ChasePlayer()
    {
        // Direct movement towards player
        Vector3 directionToPlayer = (playerCamera.transform.position - transform.position);
        directionToPlayer.y = 0; // Keep movement on ground plane
        directionToPlayer.Normalize();

        // Move towards player
        transform.position += directionToPlayer * speed * Time.deltaTime;

        // Update current node for when we lose sight and need to pathfind again
        currentNode = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / mazeGenerator.scaleFactor),
            Mathf.RoundToInt(transform.position.z / mazeGenerator.scaleFactor)
        );
    }

    void UpdateSoundInvestigation()
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

    void UpdateMovement()
    {
        // Don't move or rotate if frozen
        if (currentState == AgentState.Frozen)
            return;

        // Always face the player unless frozen
        Vector3 directionToPlayer = (playerCamera.transform.position - transform.position);
        directionToPlayer.y = 0; // Keep rotation only on horizontal plane
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Handle movement based on state
        switch (currentState)
        {            
            case AgentState.Chasing:
                ChasePlayer();
                break;
            
            case AgentState.Investigating:
            case AgentState.Patrolling:
                if (currentPath.Count > 0 && currentPathIndex < currentPath.Count)
                {
                    MoveAlongPath();
                }
                break;
        }
    }

    bool IsBeingViewedByCamera()
    {
        if (!playerCamera) return false;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        Vector3 colliderCenter = transform.position + (capsule ? capsule.center : Vector3.zero);
        
        Vector3 directionToCamera = playerCamera.transform.position - colliderCenter;
        Vector3 cameraToAgent = -directionToCamera;
        float angle = Vector3.Angle(playerCamera.transform.forward, cameraToAgent);
        bool inFOV = angle < viewAngleThreshold;

        if (showDebugInfo)
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 50f, Color.blue);
            Debug.DrawLine(playerCamera.transform.position, colliderCenter, inFOV ? Color.green : Color.red);
        }

        if (inFOV)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, cameraToAgent.normalized, out hit))
            {
                bool canSeeAgent = hit.collider.gameObject == gameObject;
                if (showDebugInfo && canSeeAgent)
                {
                    Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red, 0.1f);
                }
                return canSeeAgent;
            }
        }
        return false;
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

        // Update position in grid when reaching target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentNode = currentPath[currentPathIndex];
            currentPathIndex++;
        }

        // Always face the player
        Vector3 directionToPlayer = (playerCamera.transform.position - transform.position);
        directionToPlayer.y = 0; // Keep rotation only on horizontal plane
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();
        
        // Reset node values
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

    public int GetScore() { return score; }
}