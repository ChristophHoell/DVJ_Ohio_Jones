using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimator : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private Sprite[] animationSprites = new Sprite[5];
    
    [Header("Animation Settings")]
    [SerializeField] private float frameInterval = 0.2f;
    
    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject;
    
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
        
        // Get player's Interactor component
        if (playerObject)
        {
            playerInteractor = playerObject.GetComponent<Interactor>();
            if (!playerInteractor)
            {
                Debug.LogError("Player object is missing Interactor component!");
            }
        }
        else
        {
            Debug.LogError("Player object reference is missing!");
        }

        // Start hidden
        uiImage.enabled = false;
    }

    private void Update()
    {
        if (!playerInteractor) return;

        // Check if player has treasure
        if (playerInteractor.HoldTreasure)
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
            // Check if we have all sprites assigned
            if (animationSprites.Length != 5)
            {
                Debug.LogError("Please assign exactly 5 sprites in the inspector!");
                yield break;
            }

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