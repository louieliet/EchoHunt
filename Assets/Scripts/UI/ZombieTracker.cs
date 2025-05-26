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
        // Limpia todos los hijos antes de crear los nuevos
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Siempre crea 3 skulls (o el número que quieras)
        for (int i = 0; i < 3; i++)
        {
            Image thing = Instantiate(SkullPrefab);
            thing.transform.SetParent(transform);
            thing.color = Color.red;
        }

        ZombieCounter = 0; // Reinicia el contador de capturados
    }

    void UpdateCapturedZombieCount()
    {
        Transform skull = transform.GetChild(ZombieCounter);
        skull.GetComponent<Image>().color = Color.green;
        ZombieCounter++;
    }

}
