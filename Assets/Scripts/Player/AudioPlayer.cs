using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource BreathingAudioSource; // Reference to the first audio source
    public AudioSource footStepAudioSource; // Reference to the second audio source
    public float audioFadeSpeed = 5f; // Speed at which the audio fades in and out


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!BreathingAudioSource.isPlaying)
            {
                BreathingAudioSource.Play();
            }
            BreathingAudioSource.volume = Mathf.Lerp(BreathingAudioSource.volume, .75f, audioFadeSpeed * Time.deltaTime);

            if (!footStepAudioSource.isPlaying)
            {
                footStepAudioSource.Play();
            }
            footStepAudioSource.volume = Mathf.Lerp(footStepAudioSource.volume, 1f, audioFadeSpeed * Time.deltaTime);
        }
        else
        {
            if (BreathingAudioSource.isPlaying)
            {
                BreathingAudioSource.volume = Mathf.Lerp(BreathingAudioSource.volume, 0f, audioFadeSpeed * Time.deltaTime);
                if (BreathingAudioSource.volume <= 0.01f)
                {
                    BreathingAudioSource.Stop();
                }
            }

            if (footStepAudioSource.isPlaying)
            {
                footStepAudioSource.volume = Mathf.Lerp(footStepAudioSource.volume, 0f, audioFadeSpeed * Time.deltaTime);
                if (footStepAudioSource.volume <= 0.01f)
                {
                    footStepAudioSource.Stop();
                }
            }
        }
    }
}
