using UnityEngine;
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnWalkAnimation()
    {
        animator.SetTrigger("isWalking");
    }

    public void OnIdleAnimation()
    {
        animator.SetTrigger("isIdle");
    }
}
