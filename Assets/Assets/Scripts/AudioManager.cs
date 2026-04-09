using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource musicSource;
    private AudioSource sfxSource; // <--- NUOVO: L'altoparlante per i suoni brevi!

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Peschiamo tutti gli AudioSource attaccati a questo oggetto
            AudioSource[] sources = GetComponents<AudioSource>();
            musicSource = sources[0];

            // Se non c'è un secondo AudioSource per gli SFX, lo creiamo noi via codice!
            if (sources.Length < 2)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                sfxSource = sources[1];
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource == null) return;
        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.clip = musicClip;
        musicSource.Play();
    }

    // --- NUOVA FUNZIONE PER I SUONI DELLA UI E DEGLI EFFETTI ---
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            // PlayOneShot permette ai suoni di sovrapporsi se clicchi velocemente!
            sfxSource.PlayOneShot(clip);
        }
    }
    // -----------------------------------------------------------

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying) musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying) musicSource.UnPause();
    }
}