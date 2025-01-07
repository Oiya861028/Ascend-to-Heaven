using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Item Prefabs")]
    public GameObject portalItemPrefab;
    public GameObject compassItemPrefab;
    public GameObject gogglesItemPrefab;
    public GameObject spoonItemPrefab;
    public GameObject dynamiteItemPrefab;
    
    [Header("Settings")]
    public float interactRadius = 3f;
    public AudioClip openSound;
    
    private bool isOpen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isOpen && other.CompareTag("Player"))
        {
            // Show some visual indicator that chest can be opened
            // You can add a UI prompt or highlight effect here
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!isOpen && other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (isOpen) return;
        
        isOpen = true;
        
        // Play open sound
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }

        // Spawn random item
        SpawnRandomItem();
        
        // Optional: Play animation or particle effect
        
        // Destroy chest after delay
        Destroy(gameObject, 0.5f);
    }
    
    private void SpawnRandomItem()
    {
        // Create array of available item prefabs
        GameObject[] itemPrefabs = new GameObject[] 
        {
            portalItemPrefab,
            compassItemPrefab,
            gogglesItemPrefab,
            spoonItemPrefab,
            dynamiteItemPrefab
        };
        
        // Choose random item
        int randomIndex = Random.Range(0, itemPrefabs.Length);
        GameObject selectedPrefab = itemPrefabs[randomIndex];
        
        if (selectedPrefab != null)
        {
            // Spawn item slightly above chest
            Vector3 spawnPosition = transform.position + Vector3.up * 1f;
            Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // Optional: Visualize interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}