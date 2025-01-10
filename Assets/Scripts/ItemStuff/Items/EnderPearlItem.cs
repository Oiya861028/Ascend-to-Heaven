using UnityEngine;
public class EnderPearl : Item
{
    // Speed at which pearl moves
    public float moveSpeed = 5f;
    // Height pearl rises to
    public float riseHeight = 5f;
    
    private bool isMoving = false;
    private Vector3 targetPosition;
    
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
            // Set target position at same height as key
            targetPosition = new Vector3(
                nearestKey.transform.position.x,
                nearestKey.transform.position.y,
                nearestKey.transform.position.z);
            
            // Start moving
            isMoving = true;
            // First rise up
            transform.position += Vector3.up * riseHeight;
        }
    }
    
    void Update()
    {
        if (isMoving)
        {
            // Move toward target
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);
                
            // Check if reached target
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                OnUse();
            }
        }
    }
}