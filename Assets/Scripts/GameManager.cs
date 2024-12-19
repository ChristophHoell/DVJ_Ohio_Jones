using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    private int points = 0;

    public int detections = 0;

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


    public void PlayerDetected()
    {
        Debug.Log("Player detected");
        detections++;
    }
}
