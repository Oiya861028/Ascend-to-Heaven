using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes
using UnityEngine.UI; // Required for UI Button interaction

public class SceneController : MonoBehaviour
{
    // This function is called when the "Start Game" button is clicked on the StartScene
    public void LoadInstructionScene()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("InstructionScene");
    }

    // This function is called when the "Play Again" button is clicked on the LosingScene or WinningScene
    public void LoadStartScene()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Key.keysCollected = 0;
        SceneManager.LoadSceneAsync("GameScene");
        print("The button is working");
    }
}

