using UnityEngine;

public class GogglesItem : Item
{
    public float duration = 10f;
    
    public override void Use(PlayerController player)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            MakeTransparent(wall);
        }
        
        // Return walls to normal after duration
        player.StartCoroutine(ResetWalls(walls));
    }

    private void MakeTransparent(GameObject wall)
    {
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            Color color = material.color;
            color.a = 0.3f;
            material.color = color;
        }
    }

    private System.Collections.IEnumerator ResetWalls(GameObject[] walls)
    {
        yield return new WaitForSeconds(duration);
        foreach (GameObject wall in walls)
        {
            if (wall != null)
            {
                Renderer renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = renderer.material;
                    Color color = material.color;
                    color.a = 1f;
                    material.color = color;
                }
            }
        }
    }
}