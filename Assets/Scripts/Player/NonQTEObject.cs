using UnityEngine;

public class NonQTEObject : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.OnQTEEnter += Deactivate;
        GameManager.instance.OnQTEExit += Activate;
    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEEnter -= Deactivate;
        GameManager.instance.OnQTEExit -= Activate;
    }

    void Activate(Transform caller)
    {
        gameObject.SetActive(true);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
