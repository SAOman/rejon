using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{

    public float speed = 5f;
    public float runSpeed = 9f;
    public bool canRun = true;
    public KeyCode runningKey = KeyCode.LeftShift;
    public bool IsRunning { get; private set; }

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    public bool isGrounded;

    [Header("Wall Check")]
    public float wallDetectionDistance = 0.1f;
    public float capsuleRadius = 0.45f;
    public float capsuleHeight = 1.8f;
    public LayerMask wallMask;

    private const float MIN_WALL_NORMAL_Y = 0.2f;

    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    private Rigidbody rb;
    private Vector2 move_vector;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

public void Run_Start()
  {
  IsRunning=canRun;
  }
public void Run_Stop()
  {
  IsRunning=false;
  }
public void Move_update(Vector2 _v)
 {
 move_vector=_v;
 }
    void FixedUpdate()
    {
        GroundCheck();

        float currentSpeed = speed;

        if (IsRunning && canRun)
        {
                currentSpeed = runSpeed;
        }

        if (speedOverrides.Count > 0)
            currentSpeed = speedOverrides[speedOverrides.Count - 1]();

        Vector3 move = (transform.right *  move_vector.x + transform.forward * move_vector.y).normalized * currentSpeed;

        // ❗ KLUCZOWE: anty-ściana tylko gdy STOISZ NA ZIEMI
        if (isGrounded && CheckForWall(out Vector3 wallNormal))
        {
            Vector3 projected = Vector3.ProjectOnPlane(move, wallNormal);

            if (projected.sqrMagnitude > 0.001f)
                move = projected.normalized * currentSpeed;
            else
                move = Vector3.zero;
        }

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    void GroundCheck()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);
    }

    bool CheckForWall(out Vector3 wallNormal)
    {
        wallNormal = Vector3.zero;

        Vector3 start = transform.position + Vector3.up * capsuleRadius;
        Vector3 end = transform.position + Vector3.up * (capsuleHeight - capsuleRadius);

        Vector3 inputDir = new Vector3(move_vector.x, 0,move_vector.y);
        if (inputDir.sqrMagnitude < 0.01f) return false;

        inputDir.Normalize();
        Vector3 worldDir = transform.TransformDirection(inputDir);

        if (Physics.CapsuleCast(start, end, capsuleRadius, worldDir,
            out RaycastHit hit, wallDetectionDistance, wallMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal.y < MIN_WALL_NORMAL_Y)
            {
                wallNormal = hit.normal;
                return true;
            }
        }

        return false;
    }
}
