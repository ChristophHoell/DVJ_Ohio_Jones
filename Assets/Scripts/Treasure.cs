using UnityEngine;

public class Treasure : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite lootedSprite;
    
    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject;
    
    private SpriteRenderer spriteRenderer;
    private Interactor playerInteractor;
    private bool hasBeenLooted = false;

    private void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Set initial sprite
        if (spriteRenderer && baseSprite)
        {
            spriteRenderer.sprite = baseSprite;
        }
        
        // Get the Interactor component from the player
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
    }

    private void Update()
    {
        // Check if the treasure has been looted
        if (playerInteractor && playerInteractor.HoldTreasure && !hasBeenLooted)
        {
            UpdateTreasureAppearance();
        }
    }

    private void UpdateTreasureAppearance()
    {
        if (spriteRenderer && lootedSprite)
        {
            spriteRenderer.sprite = lootedSprite;
            hasBeenLooted = true;
            Debug.Log("Treasure appearance updated to looted state");
        }
    }
}