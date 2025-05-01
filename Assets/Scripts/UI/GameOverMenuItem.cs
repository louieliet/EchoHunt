using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenuItem : MonoBehaviour
{
    TextMeshProUGUI textAsset;
    Image imageAsset;

    private type objecttype = type.garbage;
    private Color originalColor;
    private Color transparentColor;

    void Start()
    {
        objecttype = type.garbage;

        if (TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI txt))
        {
            objecttype = type.text;
            textAsset = txt;
            originalColor = txt.color;
        }

        if (TryGetComponent<Image>(out Image img))
        {
            objecttype = type.image;
            imageAsset = img;
            originalColor = img.color;
        }
        transparentColor = originalColor;
        transparentColor.a = 0f;
    }

    void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (objecttype == type.image)
            imageAsset.color = transparentColor;
        else if (objecttype == type.text)
            textAsset.color = transparentColor;

        float timer = 0f;
        while(timer < 1f)
        {
            Color transition = Color.Lerp(transparentColor, originalColor, timer);

            if (objecttype == type.image)
                imageAsset.color = transition;
            else if (objecttype == type.text)
                textAsset.color = transition;

            timer += Time.deltaTime;
            yield return null;
        }

        if (objecttype == type.image)
            imageAsset.color = originalColor;
        else if (objecttype == type.text)
            textAsset.color = originalColor;
    }

    private enum type
    {
        garbage,
        text,
        image
    }
}
