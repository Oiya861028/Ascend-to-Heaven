using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource BreathingAudioSource;
    public AudioSource footStepAudioSource;
    public float audioFadeSpeed = 5f;
    public AgentController agent;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {   
            if (!BreathingAudioSource.isPlaying)
            {
                BreathingAudioSource.Play();
                // NotifyAgentOfSound();
            }
            BreathingAudioSource.volume = Mathf.Lerp(BreathingAudioSource.volume, .75f, audioFadeSpeed * Time.deltaTime);

            if (!footStepAudioSource.isPlaying)
            {
                footStepAudioSource.Play();
                // NotifyAgentOfSound();
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

    void NotifyAgentOfSound()
    {
        if (agent != null)
        {
            MazeGenerator maze = FindObjectOfType<MazeGenerator>();
            if (maze != null)
            {
                Vector2Int soundPos = new Vector2Int(
                    Mathf.RoundToInt(transform.position.x / 2),
                    Mathf.RoundToInt(transform.position.z / 2)
                );
                // agent.soundLocation = soundPos;
                Debug.Log($"Sound created at grid position: {soundPos}");
                Debug.DrawLine(transform.position, transform.position + Vector3.up * 5f, Color.red, 1f);
            }
        }
        else
        {
            Debug.LogWarning("Agent reference is missing in AudioPlayer!");
        }
    }
}