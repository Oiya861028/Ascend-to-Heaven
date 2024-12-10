using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using Unity.Mathematics;

public class Pathfinding : MonoBehaviour
{
    public Vector2Int startPosition;
    public Vector2Int endPosition;

    private Node[,] nodes;
    private List<Node> Frontier;
    private HashSet<Node> ExploredNode;

    private MazeGenerator mazeGenerator;

    void Start()
    {
        mazeGenerator = GetComponent<MazeGenerator>();
        InitializeNodes();
        FindPath(startPosition, endPosition);
    }
    void InitializeNodes() {
        int width = mazeGenerator.width;
        int height = mazeGenerator.height;
        nodes = new Node[width, height];
        for(int i = 0; i < width; i++){
            for(int j = 0; j < height; j++){
                nodes[i,j] = new Node(new Vector2Int(i,j), mazeGenerator.getIsWalkable(i,j));
            }
        }
    }
    void FindPath(Vector2Int startPos, Vector2Int endPos){
        Node startNode = nodes[startPos.x, startPos.y];
        Node endNode = nodes[endPos.x, endPos.y];

        Frontier = new List<Node> {startNode};
        ExploredNode = new HashSet<Node>();
        startNode.StepCost = 0;
        startNode.DistanceToEndCost = GetHeuristic(startNode, endNode);
    }

    private int GetHeuristic(Node startNode, Node endNode)
    {
        int xDis = math.abs(startNode.GridPosition.x - endNode.GridPosition.x);
        int yDis = math.abs(startNode.GridPosition.y - endNode.GridPosition.y);
        return xDis+yDis;
    }
}
