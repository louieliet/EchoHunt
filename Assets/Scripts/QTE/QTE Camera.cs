using System.Collections;
using UnityEngine;

public class QTECamera : MonoBehaviour
{
    public float AngleShake = 0.5f;

    private Quaternion originalRotation;
    private Transform cameraTransform;

    private float slowerTiming;

    void Awake()
    {
        cameraTransform = transform;
        originalRotation = cameraTransform.localRotation;

        GameManager.instance.OnQTEEnter += StartShaking;
        GameManager.instance.OnQTEExit += StopShaking;
        GameManager.instance.OnGameOver += SlowShaking;
    }

    private void OnDestroy()
    {
        if (GameManager.instance == null) return;
        GameManager.instance.OnQTEEnter -= StartShaking;
        GameManager.instance.OnQTEExit -= StopShaking;
        GameManager.instance.OnGameOver -= SlowShaking;
    }

    private void StartShaking()
    {
        StopAllCoroutines();
        StartCoroutine(ShakePart(true));
    }

    private void SlowShaking()
    {
        if (!gameObject.activeSelf) return;
        StopAllCoroutines();
        slowerTiming = 0.1f;
        StartCoroutine(ShakePart(false));
    }

    private void StopShaking(Transform caller)
    {
        StopAllCoroutines();
    }

    IEnumerator ShakePart(bool isQuick)
    {
        float timer = 0f;
        float duration;
        if (isQuick)
            duration = Random.Range(0.05f, 0.1f);
        else
            duration = slowerTiming;

            Quaternion origin = cameraTransform.localRotation;
        Quaternion target = originalRotation;
        target *= Quaternion.Euler(Random.Range(-AngleShake, AngleShake), Random.Range(-AngleShake, AngleShake), Random.Range(-AngleShake, AngleShake));

        while (timer < 1f)
        {
            cameraTransform.localRotation = Quaternion.Lerp(origin, target, timer);

            timer += Time.deltaTime / duration;
            yield return null;
        }

        if (!isQuick)
            slowerTiming *= Random.Range(1f, 1.6f);

        StartCoroutine(ShakePart(isQuick));
    }
}
