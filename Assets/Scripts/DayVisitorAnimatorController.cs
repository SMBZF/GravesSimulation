using UnityEngine;

public class DayVisitorAnimatorController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetWalking(bool walking)
    {
        animator?.SetBool("isWalking", walking);
    }

    public void PlayOfferingAnimation()
    {
        animator?.SetTrigger("offer");
    }
}
