using UnityEngine;

public class CellState : MonoBehaviour
{
    public int x;
    public int y;
    public bool isWalkable = true;
    public float hiddenReward = 0f;
    public bool isRevealed = false;
    
    private Material originalMaterial;
    private Renderer cellRenderer;

    void Awake()
    {
        cellRenderer = GetComponent<Renderer>();
        if (cellRenderer != null)
        {
            originalMaterial = cellRenderer.material;
        }
    }

    public void Reveal()
    {
        isRevealed = true;
        UpdateCellVisual();
    }

    public void UpdateCellVisual()
    {
        if (cellRenderer == null) return;

        if (isRevealed)
        {
            if (hiddenReward > 0)
                cellRenderer.material.color = new Color(0f, 0.8f, 0f, 0.5f); // Semi-transparent green
            else if (hiddenReward < 0)
                cellRenderer.material.color = new Color(0.8f, 0f, 0f, 0.5f); // Semi-transparent red
            else
                cellRenderer.material.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent gray
        }
        else
        {
            cellRenderer.material.color = Color.white;
        }
    }

    public void ResetVisual()
    {
        if (cellRenderer != null && originalMaterial != null)
        {
            cellRenderer.material = originalMaterial;
        }
    }
}