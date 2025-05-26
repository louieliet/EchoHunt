using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Range(0, 100)] private float moveSpeed;
    [SerializeField] private float groundDrag = 10f; // Increased drag for less sliding
    [SerializeField] private float stopForceMultiplier = 7f; // Force multiplier when stopping
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float sprintFootstepMultiplier = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private float groundCheckOffset = 0.1f;
    private bool isGrounded;
    private Collider playerCollider;

    [Header("References")]
    [SerializeField] private Transform orientation;
    private Vector2 movementInput;
    private Rigidbody rb;

    [Header("Sound")]
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private float sprintNoiseMultiplier = 1.5f;
    public bool IsMakingNoise { get; private set; }
    private float currentNoiseRadius;

    [Header("Sound Gizmos")]
    [SerializeField] private bool showNoiseGizmo = true;
    [SerializeField] private Color noiseGizmoColor = Color.cyan;

    [Header("Footstep Sound")]
    public AudioClip footstepClip;
    public float footstepInterval = 0.4f;
    private float footstepCooldown = 0f;
    private AudioSource audioSource;

    [Header("Parry Sound")]
    public AudioClip parryClip;

    private bool isSprinting;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        playerCollider = GetComponent<Collider>();

        PlayerController.controller.Player.Move.performed += ctx => OnMovement(ctx.ReadValue<Vector2>());
        PlayerController.controller.Player.Move.canceled += ctx => OnMovement(Vector2.zero);
        PlayerController.controller.Player.Sprint.performed += ctx => OnSprint(true);
        PlayerController.controller.Player.Sprint.canceled += ctx => OnSprint(false);

        GameManager.player = this.transform;

        StageBuilder.instance.OnLevelBuild += ResetPlayer;

        GameManager.instance.OnQTEExit += SuccesfulQTEDefense;
    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEExit -= SuccesfulQTEDefense;
        PlayerController.controller.Player.Sprint.performed -= ctx => OnSprint(true);
        PlayerController.controller.Player.Sprint.canceled -= ctx => OnSprint(false);
    }

    void SuccesfulQTEDefense(Transform caller)
    {
        if (caller == null) return;

        if (parryClip != null && audioSource != null)
            audioSource.PlayOneShot(parryClip, 2.5f);

        Vector3 newCallerPosition = caller.position;
        newCallerPosition.y = transform.position.y;

        Vector3 direction = (transform.position - newCallerPosition).normalized;
        rb.linearVelocity = direction * 5f;
    }

    void ResetPlayer()
    {
        transform.position = StageBuilder.instance.GetRandomPositionAtMaze();
    }

    void OnMovement(Vector2 input)
    {
        movementInput = input;
        IsMakingNoise = input.magnitude > 0.1f; // Actualiza el estado de ruido basado en la entrada
    }

    void OnSprint(bool sprinting)
    {
        isSprinting = sprinting;
    }

    void Update()
    {
        // Ground check
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * groundCheckOffset, Vector3.down, out hit, playerHeight + groundCheckOffset)
                     && hit.collider != playerCollider;

        if (IsMakingNoise)
        {
            // Si estás corriendo, el radio puede ser mayor, por ejemplo
            currentNoiseRadius = noiseRadius * (movementInput.magnitude > 0.5f ? sprintNoiseMultiplier : 1);
        }
        else
        {
            currentNoiseRadius = 0;
        }

        PlayFootstepSound();
    }

    public float GetCurrentNoiseRadius() => currentNoiseRadius;

    void FixedUpdate()
    {
        CalculateMovement();
        ApplyDrag();
        SpeedControl();
    }

    private void ApplyDrag()
    {
        // Apply drag when player is on ground
        rb.linearDamping = groundDrag;

        // Quick stop when no input and moving slowly
        if (movementInput.magnitude < 0.1f && rb.linearVelocity.magnitude < 0.5f)
        {
            // Hard stop when velocity is already low
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void CalculateMovement()
    {
        if (!isGrounded) return;

        Vector3 moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        if (movementInput.magnitude > 0.1f)
        {
            // Apply force only when there's input
            rb.AddForce(moveDirection.normalized * currentSpeed * 10f, ForceMode.Force);
        }
        else if (rb.linearVelocity.magnitude > 0.1f)
        {
            // Apply counter force when trying to stop
            Vector3 oppositeForce = -rb.linearVelocity.normalized * currentSpeed * stopForceMultiplier;
            rb.AddForce(oppositeForce, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showNoiseGizmo || Application.isPlaying == false) return;

        // Dibuja el área de sonido solo cuando está haciendo ruido
        if (IsMakingNoise)
        {
            Gizmos.color = noiseGizmoColor;
            Gizmos.DrawWireSphere(transform.position, currentNoiseRadius);
        }

        // Draw ground check ray
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * groundCheckOffset, Vector3.down * (playerHeight + groundCheckOffset));
    }

    private void PlayFootstepSound()
    {
        footstepCooldown -= Time.deltaTime;
        bool isMoving = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > 0.1f;

        if (isMoving && footstepClip != null && footstepCooldown <= 0f)
        {
            audioSource.PlayOneShot(footstepClip);
            float currentInterval = footstepInterval / (isSprinting ? sprintFootstepMultiplier : 1f);
            footstepCooldown = currentInterval;
        }
    }
}