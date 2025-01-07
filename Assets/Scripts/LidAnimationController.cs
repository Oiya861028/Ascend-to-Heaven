using UnityEngine;

//Take the Player distance from the object and rotate the object based on the distance
public class LidAnimationController : MonoBehaviour
{
    
    public Transform player;
    public float maxDistance = 5f; // Maximum distance to consider for rotation
    public Transform lidPivot;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance < maxDistance)
        {
            // Calculate the rotation angle based on the distance
            float rotationZ = (maxDistance - distance / maxDistance) * 90f; // Full rotation (360 degrees) when distance is 0
            lidPivot.transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        }
    }
}
