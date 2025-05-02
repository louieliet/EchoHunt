using UnityEngine;

public class CaptureCapsule : MonoBehaviour
{
    public Transform CaptureTransform;
    public Light capsuleLight;

    private bool Used;

    private void Start()
    {
        StageBuilder.instance.OnLevelBuild += ResetCapsule;
    }

    private void ResetCapsule()
    {
        transform.position = StageBuilder.instance.GetRandomPositionAtMaze();
        Used = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Used) return;

        if(other.TryGetComponent<ICapturable>(out ICapturable capturable))
        {
            Used = true;
            capturable.Capture(CaptureTransform.position);
        }
    }

    void Update()
    {
        capsuleLight.intensity = 15 + (5f * Mathf.Sin(Time.time * 5f));
    }
}
