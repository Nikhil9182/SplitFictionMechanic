using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Transform cameraTransform;
    private CharacterController controller;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedGravity = -2f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int maxJumps = 2;

    [Header("Jump Assist")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 6f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.75f;

    [Header("Shoot")]
    [SerializeField] private Transform aimTarget;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private StickyExplosive projectilePrefab;

    private StickyExplosive activeExplosive;

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController animationController;

    public bool IsGrounded => controller.isGrounded;
    public bool IsDashing => isDashing;
    public bool IsAiming => isAiming;
    public float MoveMagnitude => moveInput.magnitude;
    public float VerticalVelocity => velocity.y;

    Coroutine currentShootRoutine;

    private Vector2 moveInput;
    private Vector3 velocity;

    private int jumpsUsed;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isAiming;
    private bool isDashing;
    private bool canDash = true;

    private Vector3 dashVelocity;

    private void Awake()
    {
        controller = GetComponentInChildren<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
        animationController = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        UpdateGroundState();
        UpdateJumpBuffer();
        UpdateAimTargetPosition();
        HandleJump();
        HandleGravity();
        HandleMovement();

        MoveCharacter();
    }

    public void SetSpawnPoint(Transform spawnPoint)
    {
        if (controller == null) controller = GetComponentInChildren<CharacterController>(true);
        controller.transform.position = spawnPoint.position;
        controller.transform.rotation = spawnPoint.rotation;
        controller.enabled = true;
    }

    #region Ground

    private void UpdateGroundState()
    {
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;

            if (velocity.y < 0f)
                velocity.y = groundedGravity;

            jumpsUsed = 0;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    #endregion

    #region Jump

    private void UpdateJumpBuffer()
    {
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;
    }

    private void HandleJump()
    {
        if (jumpBufferTimer <= 0f)
            return;

        bool canGroundJump =
            coyoteTimer > 0f &&
            jumpsUsed == 0;

        bool canAirJump =
            jumpsUsed > 0 &&
            jumpsUsed < maxJumps;

        if (!canGroundJump && !canAirJump)
            return;

        velocity.y =
            Mathf.Sqrt(jumpHeight * -2f * gravity);

        jumpsUsed++;

        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    #endregion

    #region Movement

    private void HandleMovement()
    {
        if (isDashing)
            return;

        Vector3 moveDirection =
            GetCameraRelativeDirection(moveInput);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            float rotationLerp =
                1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationLerp);
        }
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return (
            forward * input.y +
            right * input.x
        ).normalized;
    }

    private void MoveCharacter()
    {
        Vector3 moveDirection =
            GetCameraRelativeDirection(moveInput);

        Vector3 horizontalVelocity =
            moveDirection * moveSpeed;

        Vector3 finalVelocity =
            horizontalVelocity +
            dashVelocity;

        finalVelocity.y = velocity.y;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    #endregion

    #region Input

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        jumpBufferTimer = jumpBufferTime;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!canDash || isDashing)
            return;

        StartCoroutine(DashRoutine());
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;


        if (activeExplosive != null)
        {
            activeExplosive.Detonate();
        }
        else if (currentShootRoutine == null)
        {
            currentShootRoutine = StartCoroutine(ShootRoutine());
        }
    }

    #endregion

    #region Shoot

    private void FireProjectile()
    {
        StickyExplosive projectile =
            Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                projectileSpawnPoint.rotation);

        projectile.Initialize(this, aimTarget.position - projectileSpawnPoint.position);

        activeExplosive = projectile;
    }

    private void UpdateAimTargetPosition()
    {
        if (!isAiming) return;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, 200f))
        {
            aimTarget.position = hit.point;
        }
        else
        {
            aimTarget.position = cameraTransform.position + cameraTransform.forward * 200f;
        }

        //aimTarget.position = cameraTransform.position + cameraTransform.forward * 100f;
        aimTarget.forward = cameraTransform.forward;
    }

    private IEnumerator ShootRoutine()
    {
        isAiming = true;

        yield return new WaitForSeconds(0.08f);

        FireProjectile();
        currentShootRoutine = null;

        yield return new WaitForSeconds(0.15f);

        isAiming = false;
    }

    #endregion

    #region Dash

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDirection;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            dashDirection =
                GetCameraRelativeDirection(moveInput);
        }
        else
        {
            dashDirection = transform.forward;
        }

        float dashSpeed =
            dashDistance / dashDuration;

        dashVelocity =
            dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        dashVelocity = Vector3.zero;

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    #endregion
}