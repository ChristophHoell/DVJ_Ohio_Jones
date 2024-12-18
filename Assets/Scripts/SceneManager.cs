using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Array to store our scene names/indices
    [SerializeField] private string[] gameScenes;
    
    // Reference to keep track of current scene
    private int currentSceneIndex = 0;
    
    // Singleton pattern to ensure only one SceneLoader exists
    private static SceneLoader instance;
    
    private void Awake()
    {
        // If an instance already exists, destroy this one
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Set up the singleton instance
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void StartGame()
    {
        // Load the first game scene (index 1, assuming 0 is menu)
        currentSceneIndex = 1;
        SceneManager.LoadScene(gameScenes[currentSceneIndex]);
    }
    
    public void ReturnToMainMenu()
    {
        // Load the menu scene (index 0)
        currentSceneIndex = 0;
        SceneManager.LoadScene(gameScenes[0]);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is tagged as "Player"
        if (other.CompareTag("Goal"))
        {
            LoadNextLevel();
        }
    }
    
    private void LoadNextLevel()
    {
        // Increment scene index
        currentSceneIndex++;
        
        // Check if we've reached the end of our scenes
        if (currentSceneIndex >= gameScenes.Length)
        {
            // Option 1: Loop back to first game scene
            currentSceneIndex = 1;
            // Option 2: Return to menu (uncomment next line if preferred)
            // ReturnToMainMenu();
            // return;
        }
        
        // Load the next scene
        SceneManager.LoadScene(gameScenes[currentSceneIndex]);
    }
}