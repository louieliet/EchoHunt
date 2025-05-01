using UnityEngine;

public class RegularZombieQTE : QTEManager
{
    public static RegularZombieQTE instance;

    protected override void Init()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void EnterRegularZombieQTE(Transform caller)
    {
        initiator = caller;
        instance.StartQTE();
    }
}
