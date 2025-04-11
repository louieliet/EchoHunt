using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetWalking(bool isWalking)
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isIdle", !isWalking);
    }

    public void SetIdle(bool isIdle)
    {
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isWalking", !isIdle);
    }
}