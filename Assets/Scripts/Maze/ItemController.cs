using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [Header("Item prefabs")]
    public GameObject keyPrefab; // Assign your key GameObject in the inspector
    public GameObject portal;
    public GameObject spoon;
    public GameObject dynamite;
    public GameObject chest;
    public GameObject map;
    public GameObject pearl;
    

    [Header("Respawn Settings")]
    public float KeyRespawnTime = 5f; // Time in seconds before keys respawn
    public float spawnDistance = 5f; // Distance between items
    public float spawnHeight = 2f; // Height of the items

    private List<Vector2> cornerCellsPosition;
    private List<Vector2> walkableCellsPosition;
    private GameObject[] keys = new GameObject[3];
    private List<Vector2> usedPositions = new List<Vector2>();

    void Start()
    {
        MazeGenerator mazeGenerator = MazeGenerator.Instance;
        float scaleFactor = mazeGenerator.scaleFactor;
        if (mazeGenerator == null)
        {
            Debug.LogError("MazeGenerator instance not found!");
            return;
        }

        int mazeWidth = mazeGenerator.width;
        int mazeHeight = mazeGenerator.height;
        Cell[,] grid = mazeGenerator.grid;

        cornerCellsPosition = new List<Vector2>();
        walkableCellsPosition = new List<Vector2>();

        // Find all cells with three walls
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (grid[x, y].IsWall) continue; // Skip walls
                int wallCount = CountWalls(grid[x, y]);
                if (wallCount == 3)
                {
                    //Store the position of the corner cells for later use
                    cornerCellsPosition.Add(new Vector2(x*scaleFactor, y*scaleFactor));
                }
                else walkableCellsPosition.Add(new Vector2(x*scaleFactor, y*scaleFactor));
            }
        }
        PlaceKeys();
        PlaceItems();
        StartCoroutine(RespawnKeys());
    }

    private void PlaceItems()
    {
        PlaceItem(portal);
        PlaceItem(spoon);
        PlaceItem(dynamite);
        PlaceItem(chest);
        PlaceItem(map);
        PlaceItem(pearl);
    }

    private void PlaceItem(GameObject item)
    {
        Vector2 randomPosition = GetUniqueRandomCellPosition(false);
        usedPositions.Add(randomPosition);

        Vector3 position = new Vector3(randomPosition.x, spawnHeight, randomPosition.y); // Adjust position as needed
        Instantiate(item, position, Quaternion.identity);
    }


    private void PlaceKeys()
    {
        // Randomly choose 3 cells and place a GameObject in them
        for (int i = 0; i < 3; i++)
        {
            if (cornerCellsPosition.Count == 0) break;

            Vector2 RandomCellPosition = GetUniqueRandomCellPosition(true);
            usedPositions.Add(RandomCellPosition);

            Vector3 position = new Vector3(RandomCellPosition.x, 1, RandomCellPosition.y); // Adjust position as needed
            keys[i] = Instantiate(keyPrefab, position, Quaternion.identity);
            keys[i].tag = "Key";
        }
    }
    
    System.Collections.IEnumerator RespawnKeys()
    {
        while (true)
        {
            yield return new WaitForSeconds(KeyRespawnTime);
            
            //Clear last used positions
            usedPositions.Clear();

            foreach (GameObject key in keys)
            {
                usedPositions.Remove(new Vector2(key.transform.position.x, key.transform.position.z));
                Vector2 RandomCellPosition = GetUniqueRandomCellPosition(true);
                usedPositions.Add(RandomCellPosition);
                key.transform.position = new Vector3(RandomCellPosition.x, 1, RandomCellPosition.y);
            }
        }
    }

    Vector2 GetUniqueRandomCellPosition(bool isKey)
    {
        List<Vector2> cellPositions = isKey ? cornerCellsPosition : walkableCellsPosition;
        if (cellPositions.Count == 0)
        {
            Debug.LogError("No available cell positions to place the item.");
            return Vector2.zero; // Return a default value or handle it as needed
        }

        Vector2 randomPosition;
        int attempts = 0;
        do
        {
            randomPosition = cellPositions[UnityEngine.Random.Range(0, cellPositions.Count)];
            attempts++;
            if (attempts > 100) // Prevent infinite loop
            {
                Debug.LogWarning("Could not find a suitable position far enough from other items.");
                break;
            }
        } while (usedPositions.Contains(randomPosition) || !IsFarEnough(randomPosition));

        return randomPosition;
    }
    private bool IsFarEnough(Vector2 position)
    {
        foreach (Vector2 usedPosition in usedPositions)
        {
            if (Vector2.Distance(position, usedPosition) < spawnDistance)
            {
                return false;
            }
        }
        return true;
    }
    int CountWalls(Cell cell)
    {
        int wallCount = 0;
        if (IsWall(cell.CellState.x - 1, cell.CellState.y)) wallCount++; // Check left
        if (IsWall(cell.CellState.x + 1, cell.CellState.y)) wallCount++; // Check right
        if (IsWall(cell.CellState.x, cell.CellState.y - 1)) wallCount++; // Check down
        if (IsWall(cell.CellState.x, cell.CellState.y + 1)) wallCount++; // Check up
        return wallCount;
    }

    bool IsWall(int x, int y)
    {
        MazeGenerator mazeGenerator = MazeGenerator.Instance;
        if (x < 0 || x >= mazeGenerator.width || y < 0 || y >= mazeGenerator.height) return true; // Out of bounds is considered a wall
        return mazeGenerator.grid[x, y].IsWall;
    }
}