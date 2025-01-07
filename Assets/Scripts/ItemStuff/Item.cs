using UnityEngine;
public enum ItemType
{
    None,
    Portal,
    Compass,
    Goggles,
    Spoon,
    Dynamite
}

public abstract class Item : MonoBehaviour
{
    public ItemType type;
    public Sprite icon;
    
    public abstract void Use(PlayerController player);
}