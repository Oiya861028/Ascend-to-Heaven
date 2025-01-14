using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    // Method to load the InstructionScene1
    public void LoadInstructionScene()
    {
        SceneManager.LoadScene("InstructionScene1");
    }
}
