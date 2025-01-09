using UnityEngine;
public class Spoon : Item
{
    // Distance to check for walls
    public float checkDistance = 2f;
    
    public override void Use()
    {
        // Cast ray forward from player
        RaycastHit hit;
        if (Physics.Raycast(playerTransform.position, playerTransform.forward, out hit, checkDistance))
        {
            // If hit a wall, destroy it
            if (hit.collider.CompareTag("Wall"))
            {
                Destroy(hit.collider.gameObject);
            }
        }
        
        OnUse();
    }
}