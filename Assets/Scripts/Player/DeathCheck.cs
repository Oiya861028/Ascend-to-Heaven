using UnityEngine;

public class DeathCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < -100)
        {
            Debug.Log("Player has fallen off the map!");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("LosingScene");
        }
    }
}
