using UnityEngine;
using TMPro;

public class GameClock : MonoBehaviour
{
    public TextMeshProUGUI clockText; // Asigna el TextMeshProUGUI de la UI donde se muestra la hora
    public int startHour = 0; // 0 = 12 AM
    public int endHour = 6;   // 6 = 6 AM
    public int minutes = 0;
    public int hour = 0;
    [Range(1, 100)]
    public int timeMultiplier = 1; // Multiplicador de velocidad del tiempo

    private float timer = 0f;
    private bool clockRunning = true;

    void Start()
    {
        hour = startHour;
        minutes = 0;
        UpdateClockText();
    }

    void Update()
    {
        if (!clockRunning) return;

        timer += Time.deltaTime * timeMultiplier;
        if (timer >= 1f)
        {
            timer = 0f;
            minutes++;
            if (minutes >= 60)
            {
                minutes = 0;
                hour++;
            }

            // Detener el reloj exactamente al llegar a las 6:00 AM
            if (hour == endHour)
            {
                minutes = 0; // Opcional: fuerza a mostrar 6:00 AM exacto
                UpdateClockText();
                clockRunning = false;
                GameManager.GameOver();
                return; // Sale para que no siga sumando
            }

            UpdateClockText();
        }
    }

    void UpdateClockText()
    {
        int displayHour = (hour == 0) ? 12 : hour;
        string ampm = (hour < 6) ? "AM" : "AM"; // Cambia a PM si tu juego lo requiere
        clockText.text = string.Format("{0:00}:{1:00} {2}", displayHour, minutes, ampm);
    }

    public void ResetClock()
    {
        hour = startHour;
        minutes = 0;
        timer = 0f;
        clockRunning = true;
        UpdateClockText();
    }
}
