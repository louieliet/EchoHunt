using UnityEngine;

public class NonQTEObject : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.OnQTEEnter += () => { gameObject.SetActive(false); };
        GameManager.instance.OnQTEExit += () => { gameObject.SetActive(true); };
    }
}
