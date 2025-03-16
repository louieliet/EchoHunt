using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Range(0, 100)] private float moveSpeed;
    [SerializeField, Range(0, 100)] private float groundDrag;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private bool isGrounded;
    public LayerMask groundMask;

    [Header("References")]
    [SerializeField] private Transform orientation;
    private Vector2 movementInput;
    private Rigidbody rb;
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => OnMovement(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => OnMovement(Vector2.zero); // Reinicia el movimiento cuando no hay entrada
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
        movementInput = input; // Actualiza el valor de movementInput
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundMask);
        rb.linearDamping = isGrounded ? groundDrag : 0; // Usa rb.drag en lugar de rb.linearDamping
    }

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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
}