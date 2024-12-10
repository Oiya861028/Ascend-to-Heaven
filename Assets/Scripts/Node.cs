using UnityEngine;

public class Node
{
    public Vector2Int GridPosition;
    public bool IsWalkable;
    public int StepCost; //cost from Start node
    public int DistanceToEndCost; //heiristic cost to end node
    public int FCost => StepCost + DistanceToEndCost; //Total Cost
    public Node parentCell;
    public Node(Vector2Int GridPosition, bool isWalkable) {
        this.GridPosition = GridPosition;
        IsWalkable = isWalkable;
    }
}
