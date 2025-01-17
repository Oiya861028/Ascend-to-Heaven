using UnityEngine;
using UnityEngine.SceneManagement;
public class Exit: MonoBehaviour{
    public static bool canExit = false;
    private Transform playerTransform;
    public float interactDistance = 2f;
    public void Start(){
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update(){
        if(KeyItem.keysCollected == 3){
            Debug.Log("All keys collected! You can now exit the maze!");
            canExit = true;
        }
        // Check if player is close enough to exit and has pressed 'E'
        if (Input.GetKeyDown(KeyCode.E)){
            if(canExit && Vector3.Distance(playerTransform.position, transform.position) < interactDistance){
                Debug.Log("Player has exited the maze!");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SceneManager.LoadScene("WinningScene");
            }
        }
    }

}