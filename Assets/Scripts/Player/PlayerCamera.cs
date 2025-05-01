using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Range(0, 100)] public float mouseSensitivity = 100.0f;
    [Range(0, 100)] public float controllerSensitivity = 50.0f;
    public Transform orientation;
    private float xRotation;
    private float yRotation;
    private bool isUsingController = false;

    void Awake()
    {
        PlayerController.controller.Player.Look.performed += OnLook;

        GameManager.instance.OnQTEExit += SuccesfulQTEDefense;
    }

    void OnDestroy()
    {
        PlayerController.controller.Player.Look.performed -= OnLook;

        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEExit -= SuccesfulQTEDefense;
    }

    void SuccesfulQTEDefense(Transform caller)
    {
        if (caller == null) return;

        // ayudame aqui
        transform.forward = (caller.position - transform.position).normalized;
        yRotation = transform.localRotation.eulerAngles.y;
        xRotation = transform.localRotation.eulerAngles.x;

        orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        Vector2 lookInput = ctx.ReadValue<Vector2>();

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