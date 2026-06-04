using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private int maxJumps = 2;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 6f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.75f;

    private CharacterController controller;

    private Vector2 moveInput;
    private Vector3 velocity;

    private int jumpsRemaining;
    private bool isDashing;
    private bool canDash = true;

    private void Awake()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        controller = GetComponentInChildren<CharacterController>();
    }

    private void Start()
    {
        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }

    #region Movement

    private void HandleMovement()
    {
        if (isDashing)
            return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection =
            forward * moveInput.y +
            right * moveInput.x;

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            jumpsRemaining = maxJumps;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    #endregion

    #region Input Callbacks

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (jumpsRemaining <= 0)
            return;

        velocity.y =
            Mathf.Sqrt(jumpHeight * -2f * gravity);

        jumpsRemaining--;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!canDash || isDashing)
            return;

        StartCoroutine(DashRoutine());
    }

    #endregion

    #region Dash

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDirection;

        if (moveInput.magnitude > 0.1f)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            dashDirection =
                (forward * moveInput.y +
                 right * moveInput.x).normalized;
        }
        else
        {
            dashDirection = transform.forward;
        }

        float elapsed = 0f;
        float dashSpeed = dashDistance / dashDuration;

        while (elapsed < dashDuration)
        {
            controller.Move(
                dashDirection *
                dashSpeed *
                Time.deltaTime);

            elapsed += Time.deltaTime;

            yield return null;
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    #endregion
}