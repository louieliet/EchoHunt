using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class QTEManager : MonoBehaviour
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

    public static QTEManager currentQTE { get; protected set; }
    public static Transform initiator { get; protected set; }

    void Awake()
    {
        Init();

        masterAnimator = GetComponent<Animator>();

        PlayerController.controller.Player.Attack.performed += OnClick;

        Debug.Log("Assigned the everything");

        GameManager.instance.OnQTEExit += Deactivate;

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEExit -= Deactivate;
    }

    abstract protected void Init();

    void Deactivate(Transform caller)
    {
        gameObject.SetActive(false);
    }

    protected void StartQTE()
    {
        gameObject.SetActive(true);

        QTEVolumeEffect.SetTension(10f, 0.2f, 0.3f);

        currentQTE = this;
        GameManager.EnterQTE();
    }

    void OnEnable()
    {
        Debug.Log("QTE called");

        transform.position = GameManager.player.position;
        transform.rotation = initiator.rotation;

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
        currentQTE = null;
        GameManager.ExitQTE(initiator);

        QTEVolumeEffect.SetTension(1f, 0.05f, 0.418f);

        gameObject.SetActive(false);
    }

    public void GameOverEvent()
    {
        currentQTE = null;

        QTEVolumeEffect.SetTension(1f, 0.05f, 0.418f);

        GameManager.GameOver();
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

        if (!Clicked)
        {
            yield return new WaitForSecondsRealtime(ReactionWindowOpen);

            if (Clicked)
                SuccesfulDefense = true;
        }

        canDefend = false;


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
