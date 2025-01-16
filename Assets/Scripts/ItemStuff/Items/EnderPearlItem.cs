using UnityEngine;
public class EnderPearl : Item
{
    // Speed at which pearl moves
    public float moveSpeed = 5f;
    // Height pearl rises to
    public float riseHeight = 5f;
    
    private bool isMoving = false;
    private Vector3 targetPosition;
    private float originalZ;
    private bool hasRisen = false;
    
    public override void Use()
    {
        // Find nearest key
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        float nearestDist = float.MaxValue;
        GameObject nearestKey = null;
        
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
            originalZ = transform.position.z;
            targetPosition = new Vector3(
                nearestKey.transform.position.x,
                nearestKey.transform.position.y,
                originalZ);
            
            isMoving = true;
            hasRisen = false;
            gameObject.SetActive(true);  // Make sure it's visible for the movement
            
            // Call base OnUse but without destroying the object
            AudioPlayer audioPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioPlayer>();
            if (audioPlayer != null)
            {
                audioPlayer.NotifyAgentOfSound();
            }
            base.hasBeenUsed = true;  // Mark as used but don't destroy
        }
        else
        {
            OnUse(); // If no key found, destroy normally
        }
    }
    
    void Update()
    {
        if (isMoving)
        {
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
                // Then move toward target
                Vector3 currentPos = transform.position;
                Vector3 moveTarget = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
                
                transform.position = Vector3.MoveTowards(currentPos, moveTarget, moveSpeed * Time.deltaTime);
                
                // Check if reached target X and Y
                if (Mathf.Approximately(transform.position.x, targetPosition.x) && 
                    Mathf.Approximately(transform.position.y, targetPosition.y))
                {
                    Destroy(gameObject); // Only destroy when reaching the target
                }
            }
        }
    }
}