using System;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class KeyItem : Item
{
    protected Transform playerTransform;
    public Text keyCountText;

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
    public override void Use()
    {
        keysCollected++;
        UpdateKeyUICount();
        Debug.Log("Key used! Keys collected: " + keysCollected);
    }

    private void UpdateKeyUICount()
    {
        if(keyCountText.enabled == false)
        {
            keyCountText.enabled = true;
        }
        keyCountText.text = "X"+keysCollected.ToString();
        if(keysCollected == 3){
            keyCountText.text = keyCountText.text +" All keys collected!";
        }
    }
}
