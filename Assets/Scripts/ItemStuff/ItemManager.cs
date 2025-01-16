using UnityEngine;
using UnityEngine.UI;
public class ItemManager : MonoBehaviour
{
    private Item currentItem;
    public Image itemUIImage;
    public float pickupDistance = 2f;
    private Transform playerTransform;
    
    void Start()
    {
        playerTransform = transform;
        
        if (itemUIImage == null)
        {
            Debug.LogError("Item UI Image not assigned in Inspector!");
        }
    }
    
    void Update()
    {
        if (itemUIImage == null) return;  // Safety check
        
        // Pick up item
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem == null || currentItem.HasBeenUsed())  // Changed condition
            {
                // Find nearest item
                Item[] items = FindObjectsOfType<Item>();
                Item nearest = null;
                float nearestDist = float.MaxValue;
                
                foreach (Item item in items)
                {
                    if (item == null) continue;  // Safety check
                    
                    float dist = Vector3.Distance(playerTransform.position, item.transform.position);
                    if (dist < pickupDistance && dist < nearestDist)
                    {
                        nearest = item;
                        nearestDist = dist;
                    }
                }
                
                // Pick up nearest item
                if (nearest != null)
                {
                    currentItem = nearest;
                    SpriteRenderer spriteRenderer = nearest.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        itemUIImage.sprite = spriteRenderer.sprite;
                        itemUIImage.enabled = true;
                    }
                    nearest.gameObject.SetActive(false);
                }
            }
        }
        
        // Drop item - only if not used
        if (Input.GetKeyDown(KeyCode.Q) && currentItem != null && !currentItem.HasBeenUsed())
        {
            currentItem.gameObject.SetActive(true);
            currentItem.transform.position = playerTransform.position;
            currentItem = null;
            itemUIImage.enabled = false;
        }
        
        // Use item - only if not used
        if (Input.GetKeyDown(KeyCode.F) && currentItem != null && !currentItem.HasBeenUsed())
        {
            currentItem.Use();
            itemUIImage.enabled = false;
            
            // Only clear current item if it's not an ender pearl
            if (currentItem != null && currentItem.GetType() != typeof(EnderPearl))
            {
                currentItem = null;
            }
        }
    }
}