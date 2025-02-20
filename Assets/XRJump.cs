using UnityEngine;
using UnityEngine.InputSystem;

public class XRJump : MonoBehaviour
{
    private CharacterController characterController;
    private Vector3 playerVelocity;
    private bool isGrounded;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f; // Jump strength
    public float gravity = -9.81f; // Gravity strength
    public float groundCheckDistance = 0.2f; // Distance to check if grounded
    public LayerMask groundLayer; // Assign ground layer in inspector

    [Header("Input Settings")]
    public InputActionProperty jumpAction; // Assign in inspector

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        jumpAction.action.Enable(); // Ensure action is enabled
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Keep player on ground
        }

        HandleJump();
        ApplyGravity();
        MoveCharacter();
    }

    private void HandleJump()
    {
        if (jumpAction.action.WasPressedThisFrame())
        {
            Debug.Log("Jump button pressed!"); // Debugging input
            if (isGrounded)
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                Debug.Log("Jump applied: " + playerVelocity.y);
            }
        }
    }

    private void ApplyGravity()
    {
        playerVelocity.y += gravity * Time.deltaTime;
    }

    private void MoveCharacter()
    {
        Vector3 move = playerVelocity * Time.deltaTime;
        characterController.Move(move);
    }
}

