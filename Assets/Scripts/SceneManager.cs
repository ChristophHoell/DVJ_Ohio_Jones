using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName; // Name of the scene to load
    
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}