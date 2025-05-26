using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup[] uiElements;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float delayBetweenElements = 0.5f;

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void Start()
    {
        foreach (var element in uiElements)
        {
            element.alpha = 0f;
        }
        StartCoroutine(DelayedStartFadeIn());
    }

    private IEnumerator DelayedStartFadeIn()
    {
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeInUIElements());
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

            element.alpha = 1f; // Ensure it's fully visible
            yield return new WaitForSeconds(delayBetweenElements);
        }
    }
}
