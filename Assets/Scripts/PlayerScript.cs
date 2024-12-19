using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 6;

    [Header("Sprite Sets (4 frames each for walking)")]
    [SerializeField] private Sprite[] upSprites;   // 4 sprites for walking up
    [SerializeField] private Sprite[] downSprites; // 4 sprites for walking down
    [SerializeField] private Sprite[] sideSprites; // 4 sprites for walking sideways

    [Header("Standstill Sprites")]
    [SerializeField] private Sprite standstillUp;    // Standstill sprite for up
    [SerializeField] private Sprite standstillDown;  // Standstill sprite for down
    [SerializeField] private Sprite standstillSide;  // Standstill sprite for sideways

    [Header("Animation Settings")]
    public float animationSpeed = 0.1f; // Time between frames for walking animation

    [Header("Footstep Sound")]
    [SerializeField] private AudioSource footstepAudioSource; // Attach the footstep AudioSource here

    private Rigidbody2D _rigidbody;
    private Vector2 _velocity;
    private SpriteRenderer _spriteRenderer;
    private int _currentFrame;
    private float _frameTimer;
    private bool _isMoving;
    private string _lastDirection = "down"; // Track the last movement direction

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();

        if (_rigidbody == null)
        {
            Debug.LogError("Rigidbody2D not found!");
        }

        if (footstepAudioSource == null)
        {
            Debug.LogError("Footstep AudioSource not assigned!");
        }
        else
        {
            footstepAudioSource.loop = true; // Ensure the audio source is set to loop
        }
    }

    private void Update()
    {
        // Get movement input
        _velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * moveSpeed;

        // Determine if the player is moving
        _isMoving = _velocity.magnitude > 0;

        // Handle sprite animation and standstill logic
        UpdateSprite();

        // Handle footstep sound
        HandleFootstepSound();

        // Handle Sneak input
        if (Input.GetButtonDown("Sneak"))
        {
            moveSpeed -= 2;
            enabled = false;
            StartCoroutine(Unhide());
        }
    }

    private void UpdateSprite()
    {
        if (_isMoving)
        {
            _frameTimer += Time.deltaTime;
            if (_frameTimer >= animationSpeed)
            {
                _frameTimer = 0f;
                _currentFrame = (_currentFrame + 1) % 4; // Cycle through 4 frames
            }

            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                _spriteRenderer.sprite = sideSprites[_currentFrame];
                _spriteRenderer.flipX = false;
                _lastDirection = "side";
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                _spriteRenderer.sprite = sideSprites[_currentFrame];
                _spriteRenderer.flipX = true;
                _lastDirection = "side";
            }
            else if (Input.GetAxisRaw("Vertical") > 0)
            {
                _spriteRenderer.sprite = upSprites[_currentFrame];
                _lastDirection = "up";
            }
            else if (Input.GetAxisRaw("Vertical") < 0)
            {
                _spriteRenderer.sprite = downSprites[_currentFrame];
                _lastDirection = "down";
            }
        }
        else
        {
            // Display standstill sprite based on last direction
            _currentFrame = 0; // Reset to first frame when idle
            switch (_lastDirection)
            {
                case "side":
                    _spriteRenderer.sprite = standstillSide;
                    break;
                case "up":
                    _spriteRenderer.sprite = standstillUp;
                    break;
                case "down":
                default:
                    _spriteRenderer.sprite = standstillDown;
                    break;
            }
        }
    }

    private void HandleFootstepSound()
    {
        if (_isMoving)
        {
            if (!footstepAudioSource.isPlaying) // If not already playing, start the sound
            {
                footstepAudioSource.Play();
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying) // If playing, stop the sound
            {
                footstepAudioSource.Stop();
            }
        }
    }

    IEnumerator Unhide()
    {
        yield return new WaitForSeconds(2);
        moveSpeed += 2;
        enabled = true;
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Earnit"))
        {
            Debug.Log("Earnit collected");
            if (Input.GetButtonDown("Collect"))
            {
                Destroy(other.gameObject);
            }
        }
    }
}
