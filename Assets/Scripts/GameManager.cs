using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    public bool GameOver = false;
    public int detections = 0;
    public bool holdTreasure = false;

    void Start(){
     
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Update()
    {
        if(detections == 4) {
            GameOver = true;
        }



    }


    public void PlayerDetected()
    {
        Debug.Log("Player detected");
        detections++;
    }
}
