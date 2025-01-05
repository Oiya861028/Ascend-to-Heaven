using System.Collections.Generic;
using UnityEngine;

public class PlaceKeys : MonoBehaviour
{
    public GameObject keyPrefab; // Assign your key GameObject in the inspector

    void Start()
    {
        MazeGenerator mazeGenerator = MazeGenerator.Instance;
        if (mazeGenerator == null)
        {
            Debug.LogError("MazeGenerator instance not found!");
            return;
        }

        int mazeWidth = mazeGenerator.width;
        int mazeHeight = mazeGenerator.height;
        Cell[,] grid = mazeGenerator.grid;

        List<Cell> threeWallCells = new List<Cell>();

        // Find all cells with three walls
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (grid[x, y].IsWall) continue; // Skip walls
                int wallCount = CountWalls(grid[x, y]);
                if (wallCount == 3)
                {
                    threeWallCells.Add(grid[x, y]);
                }
            }
        }

        // Randomly choose 3 cells and place a GameObject in them
        for (int i = 0; i < 3; i++)
        {
            if (threeWallCells.Count == 0) break;

            int randomIndex = Random.Range(0, threeWallCells.Count);
            Cell chosenCell = threeWallCells[randomIndex];
            threeWallCells.RemoveAt(randomIndex);

            Vector3 position = new Vector3(chosenCell.CellState.x * mazeGenerator.scaleFactor, 0, chosenCell.CellState.y * mazeGenerator.scaleFactor); // Adjust position as needed
            Instantiate(keyPrefab, position, Quaternion.identity);
        }
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