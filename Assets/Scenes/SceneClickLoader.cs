using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneClickLoader : MonoBehaviour
{
    [Tooltip("The name of the scene to load on click")]
    public string nextSceneName;

    void Update()
    {
        // Check if the mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Load the next scene
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
