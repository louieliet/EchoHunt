using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QTEVolumeEffect : MonoBehaviour
{
    private static QTEVolumeEffect instance;
    private Vignette vignetteComponent;

    private static float vignetteTension = 0.05f;
    private static float vignettePalpitationSpeed = 0.5f;
    private static float vignetteBaseIntensity = 0.418f;

    private float timeStamp;

    public float OpenTime = 0.3f;
    public float timerMultiplier;

    bool canPalpitate;

    void Start()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        UnityEngine.Rendering.Volume volume = gameObject.GetComponent<UnityEngine.Rendering.Volume>();
        if (volume.profile.TryGet<Vignette>(out Vignette tmp))
        {
            vignetteComponent = tmp;
        }

        canPalpitate = true;

        GameManager.instance.OnQTEEnter += StartFadeOut;
        GameManager.instance.OnQTEExit += StartFadeOut;
        GameManager.instance.OnGameOver += StartFadeIn;
    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEEnter -= StartFadeOut;
        GameManager.instance.OnQTEExit -= StartFadeOut;
        GameManager.instance.OnGameOver -= StartFadeIn;

    }

    public static void SetTension(float speed = 1f, float intensity = 0.2f, float baseint = 0.418f)
    {
        instance.timeStamp = Time.time;

        vignettePalpitationSpeed = speed;
        vignetteTension = intensity;
        vignetteBaseIntensity = baseint;
    }

    private void StartFadeOut(Transform c)
    {
        StartCoroutine(FadeOut());
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private void StartFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        canPalpitate = false;

        float timer = 0f;
        while(timer < 1f)
        {
            vignetteComponent.intensity.value = Mathf.SmoothStep(1f, vignetteBaseIntensity, timer);

            timer += Time.deltaTime / OpenTime;
            yield return null;
        }

        canPalpitate = true;
    }

    IEnumerator FadeIn()
    {
        canPalpitate = false;

        float timer = 0f;
        while (timer < 1f)
        {
            vignetteComponent.intensity.value = Mathf.SmoothStep(vignetteBaseIntensity, 1f, timer);

            timer += Time.deltaTime / OpenTime;
            yield return null;
        }

        canPalpitate = true;
    }

    void Update()
    {
        if(canPalpitate)
            vignetteComponent.intensity.value = Mathf.Abs(Mathf.Sin((Time.time - timeStamp) * vignettePalpitationSpeed)) * vignetteTension + vignetteBaseIntensity;
    }
}
