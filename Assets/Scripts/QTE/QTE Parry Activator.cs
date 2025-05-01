using UnityEngine;

public class QTEParryActivator : MonoBehaviour
{
    private QTEManager qtemanager;

    void Start()
    {
        qtemanager = transform.parent.GetComponent<QTEManager>();
    }

    public void StartReaction()
    {
        qtemanager.StartReaction();
    }
}
