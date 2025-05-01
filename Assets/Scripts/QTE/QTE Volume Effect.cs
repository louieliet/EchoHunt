using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QTEVolumeEffect : MonoBehaviour
{
    private Vignette vignetteComponent;

    public float OpenTime = 0.3f;
    public float timerMultiplier;

    bool isStarting;

    void Start()
    {
        UnityEngine.Rendering.Volume volume = gameObject.GetComponent<UnityEngine.Rendering.Volume>();
        if (volume.profile.TryGet<Vignette>(out Vignette tmp))
        {
            vignetteComponent = tmp;
        }

        GameManager.instance.OnQTEEnter += () => { StartCoroutine(StartQTE()); };
    }

    IEnumerator StartQTE()
    {
        isStarting = true;

        float timer = 0f;
        while(timer < 1f)
        {
            vignetteComponent.intensity.value = Mathf.SmoothStep(1f, 0.3f, timer);

            timer += Time.deltaTime / OpenTime;
            yield return null;
        }

        isStarting = false;
    }

    void Update()
    {
        if (GameManager.instance.InQTE && !isStarting)
            vignetteComponent.intensity.value = Mathf.Abs(Mathf.Sin(Time.time * timerMultiplier)) * 0.1f + 0.3f;
        else if (!GameManager.instance.InQTE)
            vignetteComponent.intensity.value = 0.418f;
    }
}
