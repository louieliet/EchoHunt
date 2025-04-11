using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Range(0, 100)] private float moveSpeed;
    [SerializeField, Range(0, 100)] private float groundDrag;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    public LayerMask groundMask;

    [Header("References")]
    [SerializeField] private Transform orientation;
    private Vector2 movementInput;
    private Rigidbody rb;
    private PlayerControls controls;

    [Header("Sound")]
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private float sprintNoiseMultiplier = 1.5f;
    public bool IsMakingNoise { get; private set; }
    private float currentNoiseRadius;

    [Header("Sound Gizmos")]
    [SerializeField] private bool showNoiseGizmo = true;
    [SerializeField] private Color noiseGizmoColor = Color.cyan;


    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => OnMovement(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => OnMovement(Vector2.zero); // Reinicia el movimiento cuando no hay entrada
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        StageBuilder.instance.OnLevelBuild += ResetPlayer;
    }

    void ResetPlayer()
    {
        transform.position = StageBuilder.instance.GetRandomPositionAtMaze();

    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
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
    }


    public float GetCurrentNoiseRadius() => currentNoiseRadius;

    void FixedUpdate()
    {
        CalculateMovement();
        SpeedControl();
    }

    private void CalculateMovement()
    {
        Vector3 moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
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
}