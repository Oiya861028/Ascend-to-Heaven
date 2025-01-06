using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour
{
    public GameObject keyPrefab; // Assign your key GameObject in the inspector
    public float respawnTime = 5f; // Time in seconds before keys respawn
    private List<Vector2> cornerCellsPosition;
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
            }
        }
        // Randomly choose 3 cells and place a GameObject in them
        for (int i = 0; i < 3; i++)
        {
            if (cornerCellsPosition.Count == 0) break;

            Vector2 RandomCellPosition = GetUniqueRandomCellPosition();
            usedPositions.Add(RandomCellPosition);

            Vector3 position = new Vector3(RandomCellPosition.x, 0, RandomCellPosition.y); // Adjust position as needed
            keys[i] = Instantiate(keyPrefab, position, Quaternion.identity);
            keys[i].tag = "Key";
        }
        StartCoroutine(RespawnKeys());
    }
    System.Collections.IEnumerator RespawnKeys()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime);
            
            //Clear last used positions
            usedPositions.Clear();

            foreach (GameObject key in keys)
            {
                Vector2 RandomCellPosition = GetUniqueRandomCellPosition();
                usedPositions.Add(RandomCellPosition);
                key.transform.position = new Vector3(RandomCellPosition.x, 0, RandomCellPosition.y);
            }
        }
    }
    Vector2 GetUniqueRandomCellPosition()
    {
        Vector2 randomCellPosition;
        do
        {
            randomCellPosition = cornerCellsPosition[Random.Range(0, cornerCellsPosition.Count)];
        } while (usedPositions.Contains(randomCellPosition));
        return randomCellPosition;
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