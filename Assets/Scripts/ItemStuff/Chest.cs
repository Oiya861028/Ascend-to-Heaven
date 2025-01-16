using UnityEngine;
public class Chest : MonoBehaviour
{
    // Array to store item prefabs
    public GameObject[] itemPrefabs;
    
    // Distance player needs to be to interact
    public float interactDistance = 2f;
    
    private bool isOpen = false;
    private Transform playerTransform;
    
    void Start()
    {
        // Get player reference
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void Update()
    {
        // Check if player is close enough and presses E
        if (!isOpen && 
            Vector3.Distance(transform.position, playerTransform.position) <= interactDistance && 
            Input.GetKeyDown(KeyCode.E))
        {
            // Spawn random item
            int randomIndex = Random.Range(0, itemPrefabs.Length);
            Instantiate(itemPrefabs[randomIndex], transform.position + Vector3.up, Quaternion.identity);
            
            // Mark as opened and destroy chest
            isOpen = true;
            Destroy(gameObject);
        }
    }
}