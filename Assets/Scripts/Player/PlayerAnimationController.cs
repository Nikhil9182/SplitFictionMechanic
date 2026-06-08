using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private float crossFadeDuration = 0.15f;
    [SerializeField] private float fallThreshold = -0.5f;

    [SerializeField] private Rig aimRig;
    [SerializeField] private float blendSpeed = 12f;

    private AnimationState currentState;

    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int DashHash = Animator.StringToHash("Dash");
    private static readonly int FallHash = Animator.StringToHash("Fall");

    public enum AnimationState
    {
        Idle,
        Run,
        Jump,
        Dash,
        Fall
    }

    private void Update()
    {
        UpdateState();
        UpdateAimRig();
    }

    private void UpdateAimRig()
    {
        float target =
        playerController.IsAiming ? 1f : 0f;

        aimRig.weight =
            Mathf.MoveTowards(
                aimRig.weight,
                target,
                blendSpeed * Time.deltaTime);
    }

    private void UpdateState()
    {
        AnimationState targetState;

        if (playerController.IsDashing)
        {
            targetState = AnimationState.Dash;
        }
        else if (!playerController.IsGrounded)
        {
            if (playerController.VerticalVelocity > 0.1f)
            {
                targetState = AnimationState.Jump;
            }
            else if (playerController.VerticalVelocity < fallThreshold)
            {
                targetState = AnimationState.Fall;
            }
            else
            {
                targetState = AnimationState.Jump;
            }
        }
        else if (playerController.MoveMagnitude > 0.01f)
        {
            targetState = AnimationState.Run;
        }
        else
        {
            targetState = AnimationState.Idle;
        }

        Play(targetState);
    }

    private void Play(AnimationState state)
    {
        if (currentState == state)
            return;

        currentState = state;

        switch (state)
        {
            case AnimationState.Idle:
                animator.CrossFade(IdleHash, crossFadeDuration);
                break;

            case AnimationState.Run:
                animator.CrossFade(RunHash, crossFadeDuration);
                break;

            case AnimationState.Jump:
                animator.CrossFade(JumpHash, crossFadeDuration);
                break;

            case AnimationState.Fall:
                animator.CrossFade(FallHash, crossFadeDuration);
                break;

            case AnimationState.Dash:
                animator.CrossFade(DashHash, crossFadeDuration);
                break;
        }
    }
}