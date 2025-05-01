using System.Collections;
using UnityEngine;

public class QTECamera : MonoBehaviour
{
    public float AngleShake = 0.5f;

    private Quaternion originalRotation;
    private Transform cameraTransform;

    void Awake()
    {
        cameraTransform = transform;
        originalRotation = cameraTransform.localRotation;

        StartCoroutine(ShakePart());
    }

    IEnumerator ShakePart()
    {
        float timer = 0f;
        float duration = Random.Range(0.1f, 0.2f);

        Quaternion origin = cameraTransform.localRotation;
        Quaternion target = originalRotation;
        target *= Quaternion.Euler(Random.Range(-AngleShake, AngleShake), Random.Range(-AngleShake, AngleShake), Random.Range(-AngleShake, AngleShake));

        while (timer < 1f)
        {
            cameraTransform.localRotation = Quaternion.Lerp(origin, target, timer);

            timer += Time.deltaTime / duration;
            yield return null;
        }

        StartCoroutine(ShakePart());
    }
}
