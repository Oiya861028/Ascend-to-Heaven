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
        //check if player is close enough to exit and has press e
        if(Vector3.Distance(playerTransform.position, transform.position) < interactDistance && Input.GetKeyDown(KeyCode.E) && canExit){
            Debug.Log("Player has exited the maze!");
            SceneManager.LoadScene("WinningScene");
        }
    }

}