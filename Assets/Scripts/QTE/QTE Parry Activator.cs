using UnityEngine;

public class QTEParryActivator : MonoBehaviour
{
    private QTEManager manager;

    void Start()
    {
        manager = transform.parent.GetComponent<QTEManager>();
    }

    public void StartReaction()
    {
        manager.StartReaction();
    }
}
