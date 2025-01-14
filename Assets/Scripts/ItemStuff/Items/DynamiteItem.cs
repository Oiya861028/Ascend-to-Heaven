using UnityEngine;
public class Dynamite : Item
{
    // Explosion radius
    public float radius = 5f;
    
    public override void Use()
    {
        // Find all walls in radius
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, radius);
        
        foreach (Collider col in colliders)
        {
            // Destroy walls in radius
            if (col.CompareTag("Wall"))
            {
                Destroy(col.gameObject);
            }
        }
        
        OnUse();
    }
}