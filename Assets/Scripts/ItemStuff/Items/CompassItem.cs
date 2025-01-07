using UnityEngine;

public class CompassItem : Item
{
    public GameObject arrowPrefab;
    
    public override void Use(PlayerController player)
    {
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        Transform nearestKey = itemManager.GetNearestKey(player.transform.position);
        
        if (nearestKey != null)
        {
            Vector3 direction = (nearestKey.position - player.transform.position).normalized;
            GameObject arrow = Instantiate(arrowPrefab, player.transform.position, Quaternion.identity);
            arrow.transform.forward = new Vector3(direction.x, 0, direction.z);
            Destroy(arrow, 5f); // Arrow disappears after 5 seconds
        }
    }
}