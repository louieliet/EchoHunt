using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action OnQTEEnter;
    public event Action OnQTEExit;
    public event Action OnGameOver;

    public static GameManager instance;
    public static Transform player;

    public GameObject GameOverCanvas;

    public bool InQTE { get; private set; }

    void Awake()
    {
        instance = this;
        GameOverCanvas.SetActive(false);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void GameOver()
    {
        OnGameOver?.Invoke();
        GameOverCanvas.SetActive(true);
    }

    public void EnterQTE()
    {
        InQTE = true;
        OnQTEEnter?.Invoke();
    }

    public void ExitQTE()
    {
        InQTE = false;
        OnQTEExit?.Invoke();
    }
}
