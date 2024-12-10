using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private TextMeshProUGUI tmPro;
    public static GameManager instance;
    private int points = 0;

    void Start(){
        tmPro = GetComponentInChildren<TextMeshProUGUI>();
        tmPro.text = points.ToString();
        AddPoints(0);
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

    public void AddPoints(int addpoints){
        points += addpoints;
        tmPro.text = points.ToString();
    }
}
