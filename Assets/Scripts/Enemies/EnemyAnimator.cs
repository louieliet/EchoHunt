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
        animator.SetBool("isWalking", newState == ZombieState.Alert);
        animator.SetBool("isChasing", newState == ZombieState.Chase);
    }
}