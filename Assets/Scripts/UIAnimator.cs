using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimator : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private Sprite[] animationSprites = new Sprite[5];
    
    [Header("Animation Settings")]
    [SerializeField] private float frameInterval = 0.2f;
    
    
    private Image uiImage;
    private Interactor playerInteractor;
    private int currentSpriteIndex = 0;
    private bool isAnimating = false;

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
        

        // Check if player has treasure
        if (GameManager.instance.holdTreasure == true)
        {
            if (!uiImage.enabled)
            {
                uiImage.enabled = true;
                if (!isAnimating)
                {
                    StartCoroutine(AnimateSprites());
                }
            }
        }
        else
        {
            if (uiImage.enabled)
            {
                uiImage.enabled = false;
                StopAllCoroutines();
                isAnimating = false;
            }
        }
    }

    private IEnumerator AnimateSprites()
    {
        isAnimating = true;
        
        while (true)
        {
           

            // Update sprite
            uiImage.sprite = animationSprites[currentSpriteIndex];
            
            // Move to next sprite
            currentSpriteIndex = (currentSpriteIndex + 1) % animationSprites.Length;
            
            // Wait for interval
            yield return new WaitForSeconds(frameInterval);
        }
    }

    private void OnDisable()
    {
        // Clean up when disabled
        StopAllCoroutines();
        isAnimating = false;
    }
}