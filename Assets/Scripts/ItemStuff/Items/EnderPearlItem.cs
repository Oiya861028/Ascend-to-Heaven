using UnityEngine;
public class EnderPearl : Item
{
    public float moveSpeed = 5f;
    public float riseHeight = 5f;
    
    private bool isMoving = false;
    private Vector3 targetPosition;
    private float originalZ;
    private bool hasRisen = false;
    
    public override void Use()
    {
        // Find nearest key
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        GameObject nearestKey = null;
        float nearestDist = float.MaxValue;
        
        foreach (GameObject key in keys)
        {
            float dist = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(key.transform.position.x, key.transform.position.y));
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestKey = key;
            }
        }
        
        if (nearestKey != null)
        {
            // Store position data
            originalZ = transform.position.z;
            targetPosition = new Vector3(
                nearestKey.transform.position.x,
                nearestKey.transform.position.y,
                transform.position.z);
            
            // Setup for movement
            isMoving = true;
            hasRisen = false;
            gameObject.SetActive(true);
        }
        
        // Notify that sound was made
        AudioPlayer audioPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioPlayer>();
        if (audioPlayer != null)
        {
            audioPlayer.NotifyAgentOfSound();
        }
        
        // Tell item manager we're used (but don't destroy yet)
        base.hasBeenUsed = true;
    }
    
    void Update()
    {
        if (!isMoving) return;
        
        if (!hasRisen)
        {
            // First rise up
            float newZ = Mathf.MoveTowards(transform.position.z, originalZ + riseHeight, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
            
            if (Mathf.Approximately(transform.position.z, originalZ + riseHeight))
            {
                hasRisen = true;
            }
        }
        else
        {
            // Move toward target
            Vector3 currentPos = transform.position;
            Vector3 moveTarget = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            transform.position = Vector3.MoveTowards(currentPos, moveTarget, moveSpeed * Time.deltaTime);
            
            // Check if reached target
            if (Mathf.Approximately(transform.position.x, targetPosition.x) && 
                Mathf.Approximately(transform.position.y, targetPosition.y))
            {
                Destroy(gameObject);
            }
        }
    }
}