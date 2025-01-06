using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public bool sprint = false; // Boolean to check if sprint is true
    public AudioSource audioSource1; // Reference to the first audio source
    public AudioSource audioSource2; // Reference to the second audio source

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the audio sources are not playing at the start
        audioSource1.Stop();
        audioSource2.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (sprint)
        {
            if (!audioSource1.isPlaying)
            {
                audioSource1.Play();
            }
            if (!audioSource2.isPlaying)
            {
                audioSource2.Play();
            }
        }
        else
        {
            if (audioSource1.isPlaying)
            {
                audioSource1.Stop();
            }
            if (audioSource2.isPlaying)
            {
                audioSource2.Stop();
            }
        }
    }
}
