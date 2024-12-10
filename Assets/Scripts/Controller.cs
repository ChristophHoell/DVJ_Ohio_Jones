using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    
    public float moveSpeed = 6;
    
    private Rigidbody2D _rigidbody;
    private Camera _viewCamera;
    private Vector2 _velocity;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
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
    }

    private void FixedUpdate()
    {
        // _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }
}
