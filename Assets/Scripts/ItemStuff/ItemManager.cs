// ItemManager.cs
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    public GameObject[] keyObjects; // Assign all key objects in the scene
    public LayerMask wallLayer;    // Assign the layer containing walls

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Transform GetNearestKey(Vector3 position)
    {
        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject key in keyObjects)
        {
            float distance = Vector3.Distance(position, key.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = key.transform;
            }
        }

        return nearest;
    }

    public Vector3 GetRandomPositionNearKey(float radius)
    {
        if (keyObjects.Length == 0) return Vector3.zero;
        
        GameObject randomKey = keyObjects[Random.Range(0, keyObjects.Length)];
        
        // Try to find valid position
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPosition = randomKey.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            if (!Physics.CheckSphere(randomPosition, 0.5f, wallLayer))
            {
                return randomPosition;
            }
        }
        
        // If no valid position found, return key position
        return randomKey.transform.position;
    }
}