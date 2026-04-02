using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewPlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;          //This is the speed of the player movement
    [SerializeField] private float _rotationSpeed = 500f;    //This is the speed of the player rotation
    [SerializeField] private float _gravityMultiplier = 2f;      //This is the force applied to the player when they are in the air

    private CharacterController _controller;          // This is the call to the character controller placed into a variable
    private float _downwardForce;                   //This is the force applied to the player when they are in the air to make them fall down
    private Rigidbody body;              // This is the call to the rigidbody placed into a variable
    private Vector3 _movement;                   //This speed is the movement speed

    private Animator _animator;                   //This is the call to the animator placed into a variable

    private const string _horizontal = "Horizontal";     //This is the name of the horizontal input axis
    private const string _vertical = "Vertical";         //This is the name of the vertical
    private void Start()
    {
        _controller = GetComponent<CharacterController>(); //This code is used to get the character controller component from the player object
        body = GetComponent<Rigidbody>();     //This code is used to get the rigidbody component from the player object
        _animator = GetComponent<Animator>(); //This code is used to get the animator component from the player object
    }
    private void Update()
    {
        
        _movement.Set(InputManager.Movement.x, 0, InputManager.Movement.y);     //This code is used to set the movement vector to the input from the player
        _controller.Move(_movement * _moveSpeed * Time.deltaTime);

        body.linearVelocity = new Vector3(_movement.x, body.linearVelocity.y, _movement.z) * _moveSpeed;

        if (!_controller.isGrounded)
        {
            _downwardForce += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;     //This code is used to apply gravity to the player when they are in the air
        }
        else
        {
            _downwardForce = 0f;     //This code is used to reset the downward force when the player is on the ground
        }

        _movement.y = _downwardForce;
        _controller.Move(_movement * Time.deltaTime);

        if (_movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(_movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
        }

        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.z);

    }
}
