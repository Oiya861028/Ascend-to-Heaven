using UnityEngine;
// Base Item class
public abstract class Item : MonoBehaviour
{
    protected Transform playerTransform;
    protected bool hasBeenUsed = false;  // New flag to track usage
    
    protected virtual void Awake()
    {
        InitializePlayerTransform();
    }

    protected virtual void Start()
    {
        if (playerTransform == null)
        {
            InitializePlayerTransform();
        }
    }

    private void InitializePlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has 'Player' tag");
        }
    }
    
    public abstract void Use();
    
    // New method to check if item has been used
    public bool HasBeenUsed()
    {
        return hasBeenUsed;
    }
    
    public virtual void OnUse()
    {
        // Find the AudioPlayer component on the Player object
        AudioPlayer audioPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioPlayer>();
        if (audioPlayer != null)
        {
            audioPlayer.NotifyAgentOfSound();
        }
        else
        {
            Debug.LogError("AudioPlayer component not found on Player!");
        }
        
        hasBeenUsed = true;
        Destroy(gameObject);
    }
}