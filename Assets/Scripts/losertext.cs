using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // For scene loading

public class LoserText : MonoBehaviour
{
    private Image uiImage;
    [Header("Game Over Sound")]
    [SerializeField] private AudioSource gameOverAudioSource; // Attach the AudioSource here

    private void Start()
    {
        // Get the Image component
        uiImage = GetComponent<Image>();
        if (!uiImage)
        {
            Debug.LogError("UI Image component missing!");
            return;
        }

        // Start hidden
        uiImage.enabled = false;

        // Ensure the AudioSource is set up correctly
        if (gameOverAudioSource == null)
        {
            Debug.LogError("Game Over AudioSource is not assigned!");
        }
    }

    private void Update()
    {
        if (GameManager.instance.GameOver == true && !uiImage.enabled)
        {
            // Show the image and start the delay coroutine
            uiImage.enabled = true;

            // Play the game-over sound if it exists
            if (gameOverAudioSource != null)
            {
                gameOverAudioSource.Play();
            }

            // Start the delay coroutine to switch scenes
            StartCoroutine(GoToSceneAfterDelay("MainMenu")); // Replace "MainMenu" with your desired scene name
        }
    }

    private IEnumerator GoToSceneAfterDelay(string sceneName)
    {   GameManager.instance.GameOver = false;
        yield return new WaitForSeconds(3); // Wait for 3 seconds
        GameManager.instance.GameOver = false;
        SceneManager.LoadScene(sceneName); // Load the specified scene
        GameManager.instance.GameOver = false;
    }
}
