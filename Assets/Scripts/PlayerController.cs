using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 5f;
    public LayerMask groundMask;

    [Header("Sensing Settings")]
    public float senseRadius = 1.5f;
    public KeyCode senseKey = KeyCode.E;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("References")]
    public Camera playerCamera;  // Drag the camera in the inspector

    private float verticalRotation = 0f;
    private bool isGrounded;
    private Vector3 velocity;
    private float currentSpeed;
    private int score = 0;
    private Rigidbody rb;
    private MazeGenerator mazeGenerator;

    void Start()
    {
        // Set up references
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure Rigidbody
        rb.freezeRotation = true;
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // If camera not set in inspector, try to find it
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                // Create camera if none exists
                GameObject cameraObj = new GameObject("PlayerCamera");
                playerCamera = cameraObj.AddComponent<Camera>();
                cameraObj.transform.SetParent(transform);
                cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0); // Approximate eye height
            }
        }

        mazeGenerator = FindObjectOfType<MazeGenerator>();
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Handle camera rotation
        HandleMouseLook();

        // Handle movement
        HandleMovement();

        // Handle sensing
        if (Input.GetKeyDown(senseKey))
        {
            SenseNearbyTiles();
        }

        // Handle cursor lock toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        // Horizontal rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (camera only)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        // Check if sprinting
        currentSpeed = Input.GetKey(sprintKey) ? sprintSpeed : walkSpeed;

        // Get input directions
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction relative to where we're looking
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();

        // Apply movement
        if (moveDirection.magnitude > 0.1f)
        {
            rb.MovePosition(rb.position + moveDirection * currentSpeed * Time.deltaTime);
        }

        // Handle jumping
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void SenseNearbyTiles()
    {
        // Get current position in grid coordinates
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentY = Mathf.RoundToInt(transform.position.z);

        // Check surrounding cells (including current cell)
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                int checkX = currentX + xOffset;
                int checkY = currentY + yOffset;

                // Skip if out of bounds
                if (checkX < 0 || checkX >= mazeGenerator.width || 
                    checkY < 0 || checkY >= mazeGenerator.height)
                    continue;

                // Get the cell at this position
                float distance = Vector2.Distance(
                    new Vector2(currentX, currentY),
                    new Vector2(checkX, checkY)
                );

                if (distance <= senseRadius)
                {
                    CellState cellState = mazeGenerator.grid[checkX, checkY].CellState;
                    if (cellState != null && !cellState.isRevealed)
                    {
                        cellState.Reveal();
                        Debug.Log($"Revealed cell at ({checkX}, {checkY}) with reward: {cellState.hiddenReward}");
                    }
                }
            }
        }
    }

    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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
        AgentController agent = FindObjectOfType<AgentController>();
        int aiScore = agent != null ? agent.GetScore() : 0;
        
        GUI.Label(new Rect(10, 10, 300, 20), $"Player Score: {score} | AI Score: {aiScore}");
        GUI.Label(new Rect(10, 30, 300, 20), "Press E to Sense | Sprint with Shift");
    }
}