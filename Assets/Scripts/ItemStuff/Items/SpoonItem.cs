using UnityEngine;

public class SpoonItem : Item
{
    public float range = 2f;
    
    public override void Use(PlayerController player)
    {
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, range))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }
}