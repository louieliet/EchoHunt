using UnityEngine;
using UnityEngine.UI;

public class ZombieTracker : MonoBehaviour
{
    public Image SkullPrefab;

    private int ZombieCounter;

    void Awake()
    {
        ZombieCounter = 0;

        GameManager.instance.OnZombieCapture += UpdateCapturedZombieCount;
        GameManager.instance.OnTotalZombieAmountChange += UpdateTotalZombieCount;
    }

    void OnDestroy()
    {
        if (GameManager.instance == null) return;

        GameManager.instance.OnZombieCapture -= UpdateCapturedZombieCount;
        GameManager.instance.OnTotalZombieAmountChange -= UpdateTotalZombieCount;
    }

    void UpdateTotalZombieCount()
    {
        Image thing = Instantiate(SkullPrefab);
        thing.transform.SetParent(transform);
        thing.color = Color.red;
    }

    void UpdateCapturedZombieCount()
    {
        Transform skull = transform.GetChild(ZombieCounter);
        skull.GetComponent<Image>().color = Color.green;
        ZombieCounter++;
    }

}
