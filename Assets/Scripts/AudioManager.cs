using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip shootClip;
    public AudioClip burstClip;

    private void Awake()
    {
        // Singleton pattern to ensure only one AudioManager exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayShoot()
    {
        // PlayOneShot allows multiple sounds to overlap
        sfxSource.PlayOneShot(shootClip);
    }

    public void PlayBurst()
    {
        sfxSource.PlayOneShot(burstClip);
    }
}