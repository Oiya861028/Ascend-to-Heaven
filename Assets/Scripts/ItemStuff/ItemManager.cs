using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class ItemManager : MonoBehaviour
{
    // Current held item
    private Item currentItem;
    
    // UI image to show current item
    public Image ItemImage;
    
    // Distance to pick up items
    public float pickupDistance = 2f;
    private Transform playerTransform;
    
    void Start()
    {
        playerTransform = transform;
        
        if (ItemImage == null)
        {
            Debug.LogError("Item UI Image not assigned in Inspector!");
        }
    }
    
    void Update()
    {
        
        // Pick up item
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentItem == null)  // Changed condition
            {
                // Find nearest item
                Item[] items = FindObjectsOfType<Item>();
                Item nearest = null;
                float nearestDist = float.MaxValue;
                
                foreach (Item item in items)
                {
                    
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
                    nearest.gameObject.SetActive(false);
                    //Check if item is key, if yes then use immediately and update UI
                    ItemImage.sprite = nearest.GetComponent<SpriteRenderer>().sprite;
                    ItemImage.enabled = true;
                    
                }
            }
        }
        
        // Drop item
        if (Input.GetKeyDown(KeyCode.Q) && currentItem != null)
        {
            currentItem.gameObject.SetActive(true);
            currentItem.transform.position = transform.position;
            currentItem = null;
            ItemImage.enabled = false;
        }
        
        // Use item
        if (Input.GetKeyDown(KeyCode.F) && currentItem != null)
        {
            currentItem.Use();
            currentItem = null;
            ItemImage.enabled = false;
        }
    }
}