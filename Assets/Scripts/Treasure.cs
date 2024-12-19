using UnityEngine;

public class Treasure : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite lootedSprite;
    
    private SpriteRenderer spriteRenderer;
    private Interactor playerInteractor;
    private bool hasBeenLooted = false;
    
    private GameManager gameManager;

    private void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Set initial sprite
        if (spriteRenderer && baseSprite)
        {
            spriteRenderer.sprite = baseSprite;
        }
        
        gameManager = GameManager.instance;
    }

    private void Update()
    {
        // Check if the treasure has been looted
        if (gameManager.holdTreasure && !hasBeenLooted)
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