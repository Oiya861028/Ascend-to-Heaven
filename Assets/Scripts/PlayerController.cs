using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private int score = 0;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true; // Prevent the player from tipping over
            rb.useGravity = true;
        }
    }

    void Update()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Create movement vector
        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        
        // Normalize movement to prevent faster diagonal movement
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }

        // Move the player
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        // Optional: Rotate player to face movement direction
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoodItem"))
        {
            score += 10;
            Debug.Log($"Player collected a good item! Score: {score}");
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("BadItem"))
        {
            score -= 10;
            Debug.Log($"Player hit a bad item! Score: {score}");
            Destroy(other.gameObject);
        }
    }

    private void OnGUI()
    {
        // Display score in top-left corner
        GUI.Label(new Rect(10, 10, 100, 20), $"Score: {score}");
    }
}