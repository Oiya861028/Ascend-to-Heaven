using UnityEngine;

public class MapTimer : MonoBehaviour
{
    private float duration;
    private float timer;
    private GameObject cameraObject;

    public void Initialize(float displayDuration, GameObject camObj)
    {
        duration = displayDuration;
        timer = 0;
        cameraObject = camObj;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(cameraObject);
            Destroy(gameObject);
        }
    }
}