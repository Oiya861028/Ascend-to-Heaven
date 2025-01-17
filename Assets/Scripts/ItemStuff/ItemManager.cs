using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class ItemManager : MonoBehaviour
{
    // Current held item
    private Item currentItem;
    
    // UI image to show current item
    public Image itemUIImage;
    public Image keyUIImage;
    
    // Distance to pick up items
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
                    nearest.gameObject.SetActive(false);
                    //Check if item is key, if yes then use immediately and update UI
                    if(currentItem is KeyItem)
                    {
                        //If it's the first key, we want to draw the icon
                        if(keyUIImage.sprite == null)
                        {
                            keyUIImage.sprite = nearest.GetComponent<SpriteRenderer>().sprite;
                            keyUIImage.enabled = true;
                        }
                        currentItem.Use();
                        currentItem = null;
                    }
                    else
                    {
                        itemUIImage.sprite = nearest.GetComponent<SpriteRenderer>().sprite;
                        itemUIImage.enabled = true;
                    }
                    
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