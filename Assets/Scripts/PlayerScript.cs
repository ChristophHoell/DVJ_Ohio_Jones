using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    
    public float moveSpeed = 6;
    
    [Header("Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite sideSprite;
    
    private Rigidbody2D _rigidbody;
    private Vector2 _velocity;
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            Debug.Log("Rigidbody2D not found");
        }
    }

    private void Update()
    {
        _velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * moveSpeed;

        UpdateSprite();
        
        if (Input.GetButtonDown("Sneak"))
        {
            moveSpeed -= 2;
            enabled = false;
            
            StartCoroutine(Unhide());
        }
    }
    
    private void UpdateSprite()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            _spriteRenderer.sprite = sideSprite;
            _spriteRenderer.flipX = false;
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            _spriteRenderer.sprite = sideSprite;
            _spriteRenderer.flipX = true;
        }
        else if (Input.GetAxisRaw("Vertical") > 0)
        {
            _spriteRenderer.sprite = upSprite;
        }
        else if (Input.GetAxisRaw("Vertical") < 0)
        {
            _spriteRenderer.sprite = downSprite;
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
