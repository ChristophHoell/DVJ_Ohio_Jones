using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactor : MonoBehaviour
{
    public bool HoldTreasure = false;
    public float interactionDistance = 1f;
    
    private Transform treasureTransform;
    private Transform exitTransform;
    
    private string[] sceneNames = {
        "Level1",
        "Level2",
        "Level3",
        // Add more scene names as needed
    };

    private void Start()
    {
        // Find objects by tags
        GameObject treasureObj = GameObject.FindGameObjectWithTag("treasure");
        GameObject exitObj = GameObject.FindGameObjectWithTag("exit");
        
        // Get their transforms if found
        if (treasureObj != null) treasureTransform = treasureObj.transform;
        if (exitObj != null) exitTransform = exitObj.transform;
        
        // Error checking
        if (treasureTransform == null) Debug.LogWarning("No object with 'treasure' tag found in scene!");
        if (exitTransform == null) Debug.LogWarning("No object with 'exit' tag found in scene!");
    }

    private void Update()
    {
        // Check distance to treasure
        if (treasureTransform != null && !HoldTreasure)
        {
            float distanceToTreasure = Vector2.Distance(transform.position, treasureTransform.position);
            if (distanceToTreasure <= interactionDistance)
            {
                HoldTreasure = true;
                GameManager.instance.holdTreasure = true;
            
            }
        }
        
        // Check distance to exit
        if (exitTransform != null && HoldTreasure)
        {
            float distanceToExit = Vector2.Distance(transform.position, exitTransform.position);
            if (distanceToExit <= interactionDistance)
            {
                LoadNextScene();
            }
        }
    }

    private void LoadNextScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(sceneNames, currentSceneName);
        
        if (currentIndex != -1)
        {
            if (currentIndex + 1 < sceneNames.Length)
            {
                Debug.Log($"Loading next scene: {sceneNames[currentIndex + 1]}");
                SceneManager.LoadScene(sceneNames[currentIndex + 1]);
            }
            else
            {
                Debug.Log("No more scenes to load!");
                // Optionally handle game completion here
            }
        }
        else
        {
            Debug.LogError("Current scene not found in scenes array!");
        }
    }
}