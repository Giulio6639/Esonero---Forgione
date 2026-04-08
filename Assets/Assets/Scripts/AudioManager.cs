using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Creiamo un Singleton, così è accessibile da qualsiasi script nel gioco
    public static AudioManager Instance;

    [Header("Componenti")]
    [Tooltip("L'AudioSource dedicato SOLO alla musica di sottofondo")]
    public AudioSource musicSource;

    private void Awake()
    {
        // Se non esiste ancora un AudioManager, questo diventa quello ufficiale
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Non distruggerlo ai cambi scena
        }
        else
        {
            // Se ne esiste già uno (es. tornando al Main Menu), distruggi questo clone
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        // Se la traccia è la stessa che sta già suonando, non farla ripartire da capo!
        if (musicSource.clip == musicClip) return;

        musicSource.clip = musicClip;
        musicSource.Play();
    }

    private void Update()
    {
        // --- LOGICA DI PAUSA ---
        // Controlliamo costantemente se il tempo di gioco è fermo (Pausa, Game Over, ecc.)
        if (Time.timeScale == 0f)
        {
            // Se la musica sta suonando, mettila in pausa
            if (musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
        else
        {
            // Se il tempo è ripartito (Time.timeScale = 1f) e la musica era in pausa, falla ripartire
            if (!musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.UnPause();
            }
        }
    }
}