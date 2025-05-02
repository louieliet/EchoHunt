using UnityEngine;

public class AmbienceMusic : MonoBehaviour
{
    public AudioClip[] ambienceTracks;
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    private void PlayTrack()
    {
        source.clip = ambienceTracks[Random.Range(0, ambienceTracks.Length)];
        source.Play();
    }

    void Update()
    {
        if (!source.isPlaying)
            PlayTrack();
    }
}
