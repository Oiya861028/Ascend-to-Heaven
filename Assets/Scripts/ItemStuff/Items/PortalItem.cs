using UnityEngine;
using System.Collections.Generic;
public class Portal : Item
{
    // Range within which to find a key
    public float searchRadius = 20f;
    
    public override void Use()
    {
        // Find all keys in the scene
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        
        // Get valid positions near keys (same Z level as player)
        List<Vector3> validPositions = new List<Vector3>();
        float playerZ = playerTransform.position.z;
        
        foreach (GameObject key in keys)
        {
            Vector3 keyPos = key.transform.position;
            // Check if position is within radius and same Z level
            if (Vector3.Distance(keyPos, playerTransform.position) <= searchRadius && 
                Mathf.Approximately(keyPos.z, playerZ))
            {
                validPositions.Add(keyPos);
            }
        }
        
        // If valid positions found, teleport player to random one
        if (validPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validPositions.Count);
            playerTransform.position = validPositions[randomIndex];
        }
        
        OnUse();
    }
}