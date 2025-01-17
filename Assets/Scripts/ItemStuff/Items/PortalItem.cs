using UnityEngine;
using System.Collections.Generic;
public class Portal : Item
{
    public float searchRadius = 50f;  // Increased radius
    public float minDistance = 20f;   // Minimum distance for teleport
    
    public override void Use()
    {
        Debug.Log("Portal Use() called");
        
        // Find the player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found with tag!");
            OnUse();
            return;
        }

        // Find all keys
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        if (keys.Length == 0)
        {
            Debug.LogError("No keys found in scene!");
            OnUse();
            return;
        }

        // Get valid positions near keys
        List<Vector3> validPositions = new List<Vector3>();
        float playerZ = player.transform.position.z;

        foreach (GameObject key in keys)
        {
            if (key != null)
            {
                Vector3 keyPos = key.transform.position;
                float dist = Vector2.Distance(
                    new Vector2(keyPos.x, keyPos.y),
                    new Vector2(player.transform.position.x, player.transform.position.y));
                
                // Only add positions that are far enough away but within search radius
                if (dist >= minDistance && dist <= searchRadius)
                {
                    Vector3 validPos = new Vector3(keyPos.x, keyPos.y, playerZ);
                    validPositions.Add(validPos);
                    Debug.Log($"Added valid position at distance: {dist}");
                }
            }
        }

        if (validPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validPositions.Count);
            Vector3 targetPos = validPositions[randomIndex];

            // Move the player
            CharacterController charController = player.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
                player.transform.position = targetPos;
                charController.enabled = true;
            }
            else
            {
                player.transform.position = targetPos;
            }

            Debug.Log($"Teleported player to: {targetPos}");
        }
        else
        {
            Debug.LogWarning("No valid positions found! Try increasing search radius or decreasing minimum distance.");
        }

        OnUse();
    }
}