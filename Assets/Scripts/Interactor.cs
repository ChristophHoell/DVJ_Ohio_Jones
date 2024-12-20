using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactor : MonoBehaviour
{
    public float interactionDistance = 1f;
    
    private Transform treasureTransform;
    private Transform exitTransform;
    
    private GameManager gameManager;
    
    private string[] sceneNames = {
        "SceneOne",
        "SceneTwo",
        "SceneThree",
        "SceneFour",
        "SceneFive",
        "SceneSix",
        // Add more scene names as needed
    };

    private void Start()
    {
        gameManager = GameManager.instance;
        
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
        if (treasureTransform != null && !gameManager.holdTreasure)
        {
            float distanceToTreasure = Vector2.Distance(transform.position, treasureTransform.position);
            Debug.Log($"Transform: {transform.position}, Treasure: {treasureTransform.position}, Distance: {distanceToTreasure}");
            if (distanceToTreasure <= interactionDistance)
            {
                Debug.Log("Found treasure");
                gameManager.holdTreasure = true;
            
            }
        }
        
        // Check distance to exit
        if (exitTransform != null && gameManager.holdTreasure)
        {
            float distanceToExit = Vector2.Distance(transform.position, exitTransform.position);
            if (distanceToExit <= interactionDistance)
            {
                LoadNextScene();
            }
        }
    }

    private void LoadNextScene()
    {   gameManager.holdTreasure = false;
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