using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerControls controller;

    void Awake()
    {
        if(controller == null)
            controller = new PlayerControls();
    }

    void OnEnable()
    {
        controller.Enable();
        Debug.Log("Enabled controller");
    }

    void OnDisable()
    {
        controller.Disable();
        Debug.Log("Disabled controller");
    }
}
