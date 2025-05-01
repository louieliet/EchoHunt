using UnityEngine;

public class ShakyText : MonoBehaviour
{

    private RectTransform rectTransform;
    private Vector2 OGposition;

    public float shakyness;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        OGposition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        rectTransform.anchoredPosition = OGposition + new Vector2(Random.Range(-shakyness, shakyness), Random.Range(-shakyness, shakyness));
    }
}
