using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Range(0, 100)] public float mouseSensitivity = 100.0f;
    [Range(0, 100)] public float controllerSensitivity = 50.0f;
    public Transform orientation;
    private float xRotation;
    private float yRotation;
    private PlayerControls controls;
    private bool isUsingController = false;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Look.performed += ctx => OnLook(ctx.ReadValue<Vector2>());
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void OnLook(Vector2 lookInput)
    {
        // Detectar dispositivo actual
        isUsingController = lookInput.magnitude > 0.1f && Gamepad.all.Count > 0;

        // Calcular sensibilidad según dispositivo
        float sensitivity = isUsingController ? controllerSensitivity : mouseSensitivity;

        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
    }
}