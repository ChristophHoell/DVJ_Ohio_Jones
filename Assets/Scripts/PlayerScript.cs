using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    
    public float moveSpeed = 6;
    
    private Rigidbody2D _rigidbody;
    private Camera _viewCamera;
    private Vector2 _velocity;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            Debug.Log("Rigidbody2D not found");
        }
        _viewCamera = Camera.main;
    }

    private void Update()
    {
        _velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * moveSpeed;
        
        // Rotate the player to look at the cursor
        var mousePosition = _viewCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - transform.position).normalized;
        var angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle));
        
        if (Input.GetButtonDown("Sneak"))
        {
            moveSpeed -= 2;
            enabled = false;
            
            StartCoroutine(Unhide());
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
