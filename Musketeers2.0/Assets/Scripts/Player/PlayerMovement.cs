using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;          //This is the speed of the player movement
    private Rigidbody body;              // This is the call to the rigidbody placed into a variable
    private Vector3 _movement;                   //This speed is the movement speed

    private Animator _animator;                   //This is the call to the animator placed into a variable

    private const string _horizontal = "Horizontal";     //This is the name of the horizontal input axis
    private const string _vertical = "Vertical";         //This is the name of the vertical
    private void Awake()
    {
        body = GetComponent<Rigidbody>();     //This code is used to get the rigidbody component from the player object
        _animator = GetComponent<Animator>(); //This code is used to get the animator component from the player object
    }
    private void Update()
    {
        
        _movement.Set(InputManager.Movement.x, 0, InputManager.Movement.y);     //This code is used to set the movement vector to the input from the player

        body.linearVelocity = new Vector3(_movement.x, body.linearVelocity.y, _movement.z) * _moveSpeed;

        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.z);

    }
}
