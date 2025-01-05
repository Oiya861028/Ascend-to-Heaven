using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public Transform target;
    public float minDistance = 1.0f;
    public float maxDistance = 4.0f;
    public float smoothSpeed = 10.0f;
    public LayerMask collisionMask;

    private Vector3 dollyDir;
    private float distance;

    void Start()
    {
        Camera.main.nearClipPlane = 0.1f;
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }

    void LateUpdate()
    {
        Vector3 desiredCameraPos = target.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        if (Physics.Linecast(target.position, desiredCameraPos, out hit, collisionMask))
        {
            distance = Mathf.Clamp(hit.distance * 0.9f, minDistance, maxDistance);
        }
        else
        {
            distance = maxDistance;
        }

        Vector3 newPos = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.deltaTime * smoothSpeed);
        transform.localPosition = newPos;
    }
}