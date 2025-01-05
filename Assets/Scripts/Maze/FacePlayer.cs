using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Vector3 rotationOffset = new Vector3(0, 90, 0); // Rotation offset to correct orientation

    void Update()
    {
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0; // Keep the object upright
            Quaternion rotation = Quaternion.LookRotation(direction);
            Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation * offsetRotation, Time.deltaTime * 5f);
        }
    }
}