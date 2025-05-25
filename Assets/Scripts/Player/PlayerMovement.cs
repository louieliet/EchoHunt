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

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        PlayerController.controller.Player.Move.performed += ctx => OnMovement(ctx.ReadValue<Vector2>());
        PlayerController.controller.Player.Move.canceled += ctx => OnMovement(Vector2.zero); // Reinicia el movimiento cuando no hay entrada

        GameManager.player = this.transform;

        StageBuilder.instance.OnLevelBuild += ResetPlayer;

        GameManager.instance.OnQTEExit += SuccesfulQTEDefense;
    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEExit -= SuccesfulQTEDefense;
    }

    void SuccesfulQTEDefense(Transform caller)
    {
        if (caller == null) return;

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

    void Update()
    {
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
        Vector3 moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;

        if (movementInput.magnitude > 0.1f)
        {
            // Apply force only when there's input
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (rb.linearVelocity.magnitude > 0.1f)
        {
            // Apply counter force when trying to stop
            Vector3 oppositeForce = -rb.linearVelocity.normalized * moveSpeed * stopForceMultiplier;
            rb.AddForce(oppositeForce, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
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
    }

    private void PlayFootstepSound()
    {
        footstepCooldown -= Time.deltaTime;
        // Considera movimiento en XZ (horizontal)
        bool isMoving = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > 0.1f;
        if (isMoving && footstepClip != null && footstepCooldown <= 0f)
        {
            audioSource.PlayOneShot(footstepClip);
            footstepCooldown = footstepInterval;
        }
    }
}