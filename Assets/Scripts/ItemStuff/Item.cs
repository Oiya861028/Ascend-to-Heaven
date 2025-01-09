using UnityEngine;
using UnityEngine.UI;
public abstract class Item : MonoBehaviour
{
    // Reference to the UI image that shows the item
    protected Image itemUIImage;
    
    // Reference to the player transform
    protected Transform playerTransform;
    
    // Virtual method that will be overridden by specific items
    public abstract void Use();
    
    // Virtual method for cleanup after use
    public virtual void OnUse()
    {
        Destroy(gameObject);
    }
}