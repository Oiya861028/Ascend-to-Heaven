using UnityEngine;
using System.Collections.Generic;
public class Portal : Item
{
    public float searchRadius = 20f;
    private bool initialized = false;
    private Transform playerRootTransform;
    
    protected override void Start()
    {
        Debug.Log("Portal Start called");
        InitializePortal();
    }

    private void InitializePortal()
    {
        Debug.Log("Initializing Portal");
        if (!initialized)
        {
            // Find the root player object
            GameObject playerRoot = GameObject.FindGameObjectWithTag("Player");
            Debug.Log(playerRoot != null ? "Found player object" : "Could not find player object");
            
            if (playerRoot != null)
            {
                playerRootTransform = playerRoot.transform;
                initialized = true;
                Debug.Log($"Found player at: {playerRootTransform.position}");
            }
            else
            {
                // Try finding player capsule and get its parent
                GameObject playerCapsule = GameObject.Find("Player Capsule");
                if (playerCapsule != null && playerCapsule.transform.parent != null)
                {
                    playerRootTransform = playerCapsule.transform.parent;
                    initialized = true;
                    Debug.Log($"Found player through capsule at: {playerRootTransform.position}");
                }
                else
                {
                    Debug.LogError("Could not find player through any method!");
                }
            }
        }
    }
    
    public override void Use()
    {
        Debug.Log("Portal Use() called");
        
        if (!initialized)
        {
            Debug.Log("Trying to initialize portal during Use()");
            InitializePortal();
        }

        // Try to find player again if reference is missing
        if (playerRootTransform == null)
        {
            GameObject playerRoot = GameObject.FindGameObjectWithTag("Player");
            if (playerRoot != null)
            {
                playerRootTransform = playerRoot.transform;
                Debug.Log("Re-found player during Use()");
            }
            else
            {
                GameObject playerCapsule = GameObject.Find("Player Capsule");
                if (playerCapsule != null && playerCapsule.transform.parent != null)
                {
                    playerRootTransform = playerCapsule.transform.parent;
                    Debug.Log("Re-found player through capsule during Use()");
                }
                else
                {
                    Debug.LogError("Player root reference is still missing! Cannot use Portal.");
                    OnUse();
                    return;
                }
            }
        }

        // Store current player position
        Vector3 currentPlayerPos = playerRootTransform.position;
        Debug.Log($"Current player position: {currentPlayerPos}");

        // Find all keys in the scene
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        Debug.Log($"Found {keys.Length} keys in scene");

        List<Vector3> validPositions = new List<Vector3>();
        
        foreach (GameObject key in keys)
        {
            if (key == null) continue;
            
            Vector3 keyPos = key.transform.position;
            float dist = Vector2.Distance(
                new Vector2(keyPos.x, keyPos.y),
                new Vector2(currentPlayerPos.x, currentPlayerPos.y));
            
            Debug.Log($"Key at {keyPos}, Distance to player: {dist}, Search radius: {searchRadius}");
            
            if (dist <= searchRadius)
            {
                Vector3 validPos = new Vector3(keyPos.x, keyPos.y, currentPlayerPos.z);
                validPositions.Add(validPos);
                Debug.Log($"Added valid position: {validPos}");
            }
        }

        Debug.Log($"Found {validPositions.Count} valid positions");
        
        if (validPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validPositions.Count);
            Vector3 targetPos = validPositions[randomIndex];
            Debug.Log($"Selected position {randomIndex}: {targetPos}");
            
            playerRootTransform.position = targetPos;
            Debug.Log($"Player teleported to: {playerRootTransform.position}");
            
            OnUse();
            Debug.Log("Portal used and destroyed");
        }
        else
        {
            Debug.LogWarning("No valid teleport positions found!");
            OnUse();
        }
    }
}