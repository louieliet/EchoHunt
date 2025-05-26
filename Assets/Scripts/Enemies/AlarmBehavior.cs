using System.Collections;
using System.Linq;
using UnityEngine;

public class AlarmBehavior : MonoBehaviour
{
    public Light alarmLight;
    public AudioSource alarmSound;

    public float alarmDuration = 10f;

    private IEnvironmentListener[] listeners;

    private float fireTimestamp;
    private float duration;

    private bool active;

    void Start()
    {
        listeners = Object
            .FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None) // `true` incluye objetos inactivos
            .OfType<IEnvironmentListener>()
            .ToArray();

        alarmLight.enabled = false;
        alarmSound.Stop();

        fireTimestamp = Time.time - 10f;

        active = false;

        StageBuilder.instance.OnLevelBuild += ResetAlarm;
    }

    protected virtual void ResetAlarm()
    {
        transform.position = StageBuilder.instance.GetRandomMazePosition();
        active = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - fireTimestamp < alarmDuration) return;
        if (!active) return;

        fireTimestamp = Time.time;
        duration = alarmDuration;
        StartCoroutine(Alarm());
        foreach(IEnvironmentListener l in listeners)
        {
            l.Hear(transform.position);
        }
    }

    IEnumerator Alarm()
    {
        alarmSound.Play();
        while(duration > 0)
        {
            alarmLight.enabled = (duration % 1 > 0.5f);

            duration -= Time.deltaTime;
            yield return null;
        }
        alarmLight.enabled = false;
        alarmSound.Stop();
    }
}
