using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RegularZombieQTE : QTEManager
{
    [Header("Parry Sound")]
    public AudioClip parryClip;
    public AudioClip menuClip;
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

        if (this.gameObject.tag == "MenuZombie")
        {
            PlayMenuSound();
        }
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

    public void PlayMenuSound()
    {
        audioSource.PlayOneShot(menuClip);
    }
}
