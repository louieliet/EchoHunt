using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class RegularZombieQTE : QTEManager
{
    [Header("Parry Sound")]
    public AudioClip parryClip;
    public static RegularZombieQTE instance;
    private AudioSource audioSource;

    protected override void Init()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public static void EnterRegularZombieQTE(Transform caller)
    {
        initiator = caller;
        instance.StartQTE();
        instance.PlayParrySound();
    }

    public void PlayParrySound()
    {
        Debug.Log("Intentando reproducir parry sound en: " + gameObject.name);
        audioSource.PlayOneShot(parryClip);
        Debug.Log("Parry sound played");
    }
}
