
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : MonoBehaviour
{
    [Header("점프 충전 설정")]
    [SerializeField] private float maxChargeTime = 1.0f;
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float maxJumpForce = 12f;
    [SerializeField] private float minForwardForce = 3f;
    [SerializeField] private float maxForwardForce = 10f;

    [Header("지면 판정 설정")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isCharging;
    private bool isJumping;

    private float chargeTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // X축과 Z축 회전을 고정해 캐릭터가 넘어지지 않도록 설정
        rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        CheckGround();

        if (Keyboard.current == null)
            return;

        // 스페이스바를 누르면 점프 충전 시작
        if (Keyboard.current.spaceKey.wasPressedThisFrame
            && isGrounded
            && !isCharging
            && !isJumping)
        {
            isCharging = true;
            chargeTime = 0f;
        }

        // 스페이스바를 누르는 동안 충전
        if (isCharging && Keyboard.current.spaceKey.isPressed)
        {
            chargeTime = Mathf.Min(
                chargeTime + Time.deltaTime,
                maxChargeTime
            );
        }

        // 스페이스바를 놓으면 점프 실행
        if (isCharging
            && Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            LaunchJump();
        }
    }

    private void LaunchJump()
    {
        isCharging = false;
        isJumping = true;
        isGrounded = false;

        // 충전 시간에 따른 비율 계산 (0~1)
        float chargeRatio = maxChargeTime > 0f
            ? Mathf.Clamp01(chargeTime / maxChargeTime)
            : 1f;

        // 충전량에 따라 수직 및 수평 도약 속도 계산
        float verticalForce = Mathf.Lerp(
            minJumpForce,
            maxJumpForce,
            chargeRatio
        );

        float forwardForce = Mathf.Lerp(
            minForwardForce,
            maxForwardForce,
            chargeRatio
        );

        // 캐릭터가 바라보는 방향을 수평면에 투영
        Vector3 direction = Vector3.ProjectOnPlane(
            transform.forward,
            Vector3.up
        ).normalized;

        // 바라보는 방향이 유효하지 않을 때 기본 방향 사용
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector3.forward;
        }

        // 기존 속도를 초기화하고 도약 속도 적용
        rb.linearVelocity = Vector3.zero;

        rb.linearVelocity =
            direction * forwardForce
            + Vector3.up * verticalForce;
    }

    private void CheckGround()
    {
        if (groundCheck == null)
            return;

        wasGrounded = isGrounded;

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        // 공중에서 착지하면 다음 점프 가능
        if (isGrounded && !wasGrounded)
        {
            isJumping = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
