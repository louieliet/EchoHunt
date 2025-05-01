using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class QTEManager : MonoBehaviour
{
    public float MaxDelayWait = 0.1f;
    public float ReactionWindowOpen = 0.01f;
    public float QTETimeScale = 0.3f;

    [Header("Renderers")]
    public ParticleSystem sparkRenderer;
    public Animator zombieAnimator;
    private Animator masterAnimator;

    private bool canDefend;
    private bool Clicked;

    void Awake()
    {
        masterAnimator = GetComponent<Animator>();

        GameManager.instance.OnQTEEnter += QTEEnter;
        GameManager.instance.OnQTEExit += QTEExit;

        PlayerController.controller.Player.Attack.performed += OnClick;

        Debug.Log("Assigned the everything");

        gameObject.SetActive(false);
    }

    void QTEEnter()
    {
        gameObject.SetActive(true);
    }

    void QTEExit()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        Debug.Log("QTE called");

        transform.position = GameManager.player.position;

        masterAnimator.SetTrigger("Enter");
        zombieAnimator.SetTrigger("Reset");
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        Clicked = true;
        if (canDefend)
        {
            sparkRenderer.Stop();

            zombieAnimator.SetTrigger("Blocked");
        }
    }

    public void SuccesfulExitEvent()
    {
        GameManager.instance.ExitQTE();
    }

    public void GameOverEvent()
    {
        GameManager.instance.GameOver();
    }

    public void StartReaction()
    {
        canDefend = false;
        Clicked = false;

        sparkRenderer.Stop();

        StartCoroutine(QTE());
    }

    private IEnumerator QTE()
    {
        Time.timeScale = QTETimeScale;

        yield return new WaitForSeconds(Random.Range(0, MaxDelayWait));
        sparkRenderer.Play();

        bool SuccesfulDefense = false;
        canDefend = true;

        Debug.Log("Initiate clicking listening. Was click already clicked? > " + Clicked);
        if (!Clicked)
        {
            yield return new WaitForSecondsRealtime(ReactionWindowOpen);

            Debug.Log("During window reaction, was click clicked? > " + Clicked);
            if (Clicked)
                SuccesfulDefense = true;
        }

        canDefend = false;

        Debug.Log("Was defense succesful? > " + SuccesfulDefense);

        Time.timeScale = 1f;

        if (SuccesfulDefense)
        {
            masterAnimator.SetTrigger("Exit");
        }
        else
        {
            masterAnimator.SetTrigger("Game Over");
        }
    }
}
