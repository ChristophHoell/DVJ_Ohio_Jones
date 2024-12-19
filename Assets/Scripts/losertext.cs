using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // For scene loading

public class LoserText : MonoBehaviour
{
    private Image uiImage;

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
    }

    private void Update()
    {
        if (GameManager.instance.GameOver == true && !uiImage.enabled)
        {
            // Show the image and start the delay coroutine
            uiImage.enabled = true;
            StartCoroutine(GoToSceneAfterDelay("MainMenu")); // Replace "YourSceneNameHere" with the actual scene name
        }
    }

    private IEnumerator GoToSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(3); // Wait for 3 seconds
        SceneManager.LoadScene(sceneName); // Load the specified scene
    }
}
