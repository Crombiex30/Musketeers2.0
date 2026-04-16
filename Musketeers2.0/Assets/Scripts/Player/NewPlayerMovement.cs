using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewPlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 500f;
    [SerializeField] private float _gravityMultiplier = 2f;

    [SerializeField] private AudioSource _footstepSource; 

    private CharacterController _controller;
    private float _downwardForce;
    private Rigidbody body;
    private Vector3 _movement;

    private Animator _animator;

    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        
        if (_footstepSource == null) 
        {
            Debug.LogWarning("Footstep Source is missing on the Player!");
        }
    }

    private void Update()
    {
        _movement.Set(InputManager.Movement.x, 0, InputManager.Movement.y);
        _controller.Move(_movement * _moveSpeed * Time.deltaTime);

        body.linearVelocity = new Vector3(_movement.x, body.linearVelocity.y, _movement.z) * _moveSpeed;

        if (!_controller.isGrounded)
        {
            _downwardForce += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        }
        else
        {
            _downwardForce = 0f;
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

        HandleFootstepSound();
    }

    private void HandleFootstepSound()
    {
        bool isMoving = InputManager.Movement.sqrMagnitude > 0.01f;

        if (isMoving && _controller.isGrounded)
        {
            if (!_footstepSource.isPlaying)
            {
                _footstepSource.Play();
            }
        }
        else
        {
            if (_footstepSource.isPlaying)
            {
                _footstepSource.Stop();
            }
        }

        
    }
}