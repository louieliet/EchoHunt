using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximityMusic : MonoBehaviour
{
    public AudioClip musicClip;
    public float maxAudioDistance = 20f;
    public Transform player; // Asigna el jugador en el inspector o por código

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;
        Transform player = playerObj.transform;

        float distance = Vector3.Distance(transform.position, player.position);
        float volume = 1f - Mathf.Clamp01(distance / maxAudioDistance);
        audioSource.volume = volume;
    }

    void Awake()
    {
        // Si ya existe otro ProximityMusic, destrúyete
        if (FindObjectsOfType<ProximityMusic>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
}
