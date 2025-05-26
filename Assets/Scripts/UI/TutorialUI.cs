using UnityEngine;
using System.Collections;

public class TutorialUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup[] uiElements;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float delayBetweenElements = 0.5f;

    void Start()
    {
        foreach (var element in uiElements)
        {
            element.alpha = 0f;
        }
        StartCoroutine(FadeInUIElements());
    }

    private IEnumerator FadeInUIElements()
    {
        foreach (var element in uiElements)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                element.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            element.alpha = 1f;
            yield return new WaitForSeconds(delayBetweenElements);
        }
        // Espera 5 segundos después de mostrar todos
        yield return new WaitForSeconds(5f);
        StartCoroutine(FadeOutUIElements());
    }

    private IEnumerator FadeOutUIElements()
    {
        foreach (var element in uiElements)
        {
            float elapsedTime = 0f;
            float startAlpha = element.alpha;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                element.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeInDuration);
                yield return null;
            }
            element.alpha = 0f;
            yield return new WaitForSeconds(delayBetweenElements);
        }
    }
}
