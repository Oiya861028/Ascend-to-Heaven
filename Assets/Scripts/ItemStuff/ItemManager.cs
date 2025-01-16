using UnityEngine;
using UnityEngine.UI;
public class ItemManager : MonoBehaviour
{
    // Current held item
    private Item currentItem;

    
    // UI image to show current item
    public Image itemUIImage;
    
    // Distance to pick up items
    public float pickupDistance = 2f;
    
    void Update()
    {
        // Pick up item
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Only if not holding an item
            if (currentItem == null)
            {
                // Find nearest item
                Item[] items = FindObjectsOfType<Item>();
                Item nearest = null;
                float nearestDist = float.MaxValue;
                
                foreach (Item item in items)
                {
                    float dist = Vector3.Distance(transform.position, item.transform.position);
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
                    nearest.gameObject.SetActive(false);
                    itemUIImage.sprite = nearest.GetComponent<SpriteRenderer>().sprite;
                    itemUIImage.enabled = true;
                }
            }
        }
        
        // Drop item
        if (Input.GetKeyDown(KeyCode.Q) && currentItem != null)
        {
            currentItem.gameObject.SetActive(true);
            currentItem.transform.position = transform.position;
            currentItem = null;
            itemUIImage.enabled = false;
        }
        
        // Use item
        if (Input.GetKeyDown(KeyCode.F) && currentItem != null)
        {
            currentItem.Use();
            currentItem = null;
            itemUIImage.enabled = false;
        }
    }
}