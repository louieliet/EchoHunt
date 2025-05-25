using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        GameManager.instance.OnQTEExit += Stun;

    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEExit -= Stun;
    }

    private void Stun(Transform caller)
    {
        animator.SetTrigger("Stun");
    }

    public void Capture()
    {
        animator.SetTrigger("Capture");
    }

    public void StateChange(ZombieState newState)
    {
        // El zombie camina en Alert e Investigate
        bool walking = newState == ZombieState.Alert || newState == ZombieState.Investigate;
        // El zombie corre en Chase
        bool chasing = newState == ZombieState.Chase;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isChasing", chasing);
    }

    public void SetWalking(bool walking)
    {
        animator.SetBool("isWalking", walking);
    }
}