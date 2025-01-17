using System;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Key : MonoBehaviour
{
    protected Transform playerTransform;
    public Image KeyImage;
    public TextMeshProUGUI KeyText;
    public float interactDistance = 2f;

    public static int keysCollected = 0;
    
    protected virtual void Awake()
    {
        InitializePlayerTransform();
    }

    protected virtual void Start()
    {
        if (playerTransform == null)
        {
            InitializePlayerTransform();
        }
    }

    private void InitializePlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has 'Player' tag");
        }
    }
    void Update()
    {
        if (KeyImage == null)
        {
            KeyImage = GameObject.Find("KeyImage").GetComponent<Image>();
        }
        if (KeyText == null)
        {
            KeyText = GameObject.Find("KeyText").GetComponent<TextMeshProUGUI>();
        }
        // Check if player is close enough and presses E
        if (Vector3.Distance(transform.position, playerTransform.position) <= interactDistance && 
            Input.GetKeyDown(KeyCode.E))
        {
        keysCollected++;
        KeyImage.sprite = GetComponent<SpriteRenderer>().sprite;
        KeyImage.enabled = true;
        if(KeyText.enabled == false)
        {
            KeyText.enabled = true;
        }
        KeyText.text = "X"+keysCollected.ToString();
        if(keysCollected == 3){
            KeyText.text = KeyText.text +" Find Exit";
        }
        Debug.Log("Key collected");
        Destroy(gameObject);
        }
    }
}
