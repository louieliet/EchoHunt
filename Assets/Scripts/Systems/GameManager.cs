using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action OnQTEEnter;
    public event Action<Transform> OnQTEExit;
    public event Action OnGameOver;

    public static GameManager instance { get; private set; }
    public static Transform player;

    public Animator GameOverMenu;

    public bool InQTE { get; private set; }

    void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void StartGame()
    {
        instance.GameOverMenu.SetTrigger("Reset");

        QTEVolumeEffect.SetTension(0.5f, 0.05f, 0.418f);

        ExitQTE(null);
    }

    public static void GameOver()
    {
        instance.OnGameOver?.Invoke();
        instance.GameOverMenu.SetTrigger("Fade In");
    }

    public static void EnterQTE()
    {
        instance.InQTE = true;
        instance.OnQTEEnter?.Invoke();
    }

    public static void ExitQTE(Transform initiator)
    {
        instance.InQTE = false;
        instance.OnQTEExit?.Invoke(initiator);
    }
}
