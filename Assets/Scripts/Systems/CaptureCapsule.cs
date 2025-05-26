using UnityEngine;

public class CaptureCapsule : MonoBehaviour
{
    public Transform CaptureTransform;
    public Light capsuleLight;

    public AudioClip captureClip;

    private bool Used;
    private Animator animator;
    private AudioSource audioSource;

    private void Start()
    {
        Used = false;

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Used) return;

        if(other.TryGetComponent<ICapturable>(out ICapturable capturable))
        {
            Used = true;
            audioSource.PlayOneShot(captureClip);
            animator.SetTrigger("Use");
            capturable.Capture(CaptureTransform);
        }
    }

    void Update()
    {
        capsuleLight.intensity = 15 + (5f * Mathf.Sin(Time.time * 5f));
    }
}
