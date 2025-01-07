using UnityEngine;

public class DynamiteItem : Item
{
    public float explosionRadius = 5f;
    
    public override void Use(PlayerController player)
    {
        Collider[] colliders = Physics.OverlapSphere(player.transform.position, explosionRadius);
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Wall"))
            {
                Destroy(col.gameObject);
            }
        }
    }
}