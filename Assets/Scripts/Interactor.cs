using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactor : MonoBehaviour
{
    public bool HoldTreasure = false;
    public float interactionDistance = 1f; // Distance at which interaction occurs
    
    public Transform treasureTransform; // Reference to treasure's transform
    public Transform exitTransform;     // Reference to exit's transform
    
    // Array of scene names in build order
    private string[] sceneNames = {
        "Level1",
        "Level2",
        "Level3",
        // Add more scene names as needed
    };

    private void Update()
    {
        // Check distance to treasure
        if (treasureTransform != null && !HoldTreasure)
        {
            float distanceToTreasure = Vector2.Distance(transform.position, treasureTransform.position);
            if (distanceToTreasure <= interactionDistance)
            {
                HoldTreasure = true;
                Debug.Log("Treasure collected!");
                // Optionally disable or destroy the treasure object
                // treasureTransform.gameObject.SetActive(false);
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
        // Get current scene index
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(sceneNames, currentSceneName);
        
        // If current scene is found in array
        if (currentIndex != -1)
        {
            // If there's a next scene, load it
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