using UnityEngine;

public class PortalItem : Item
{
    public float teleportRadius = 10f;

    public override void Use(PlayerController player)
    {
        ItemManager itemManager = FindObjectOfType<ItemManager>();
        Vector3 newPosition = itemManager.GetRandomPositionNearKey(teleportRadius);
        player.transform.position = newPosition;
    }
}