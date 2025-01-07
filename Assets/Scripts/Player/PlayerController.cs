using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float pickupRadius = 3f; // Radius within which items can be picked up
    public LayerMask itemLayer; // Layer containing pickup items
    public Image itemDisplayImage;
    private Item currentItem;
    public AudioClip pickupSound;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupItem();
        }
        
        if (Input.GetKeyDown(KeyCode.Q) && currentItem != null)
        {
            DropItem();
        }
        
        if (Input.GetKeyDown(KeyCode.F) && currentItem != null)
        {
            UseItem();
        }
    }
    void TryPickupItem()
    {
        if (currentItem != null) return;

        // Get all colliders within pickup radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius, itemLayer);
        
        if (hitColliders.Length == 0) return;

        // Find the closest item
        Item closestItem = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in hitColliders)
        {
            Item item = col.GetComponent<Item>();
            if (item != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }

        if (closestItem != null)
        {
            PickupItem(closestItem);
        }
    }

    void PickupItem(Item item)
    {
        currentItem = item;
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        item.gameObject.SetActive(false);
        UpdateItemDisplay();
        
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    void DropItem()
    {
        currentItem.transform.SetParent(null);
        currentItem.transform.position = transform.position + transform.forward;
        currentItem.gameObject.SetActive(true);
        currentItem = null;
        UpdateItemDisplay();
    }

    void UseItem()
    {
        currentItem.Use(this);
        Destroy(currentItem.gameObject);
        currentItem = null;
        UpdateItemDisplay();
    }

    void UpdateItemDisplay()
    {
        if (currentItem != null)
        {
            itemDisplayImage.sprite = currentItem.icon;
            itemDisplayImage.color = Color.white;
        }
        else
        {
            itemDisplayImage.sprite = null;
            itemDisplayImage.color = Color.clear;
        }
    }

    // Optional: Visualize pickup radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}