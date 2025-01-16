using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes
using UnityEngine.UI; // Required for UI Button interaction

public class SceneController : MonoBehaviour
{
    // This function is called when the "Start Game" button is clicked on the StartScene
    public void LoadInstructionScene()
    {
        SceneManager.LoadScene("InstructionScene");
    }

    // This function is called when the "Play Again" button is clicked on the LosingScene or WinningScene
    public void LoadStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}

