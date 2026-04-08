using UnityEngine;
using System.Collections;

public class LevelMusic : MonoBehaviour
{
    [Header("Musica di questo Livello")]
    public AudioClip backgroundMusic;

    private IEnumerator Start()
    {
        // Aspettiamo 0.1 secondi. Questo risolve il problema della "Race Condition"!
        yield return new WaitForSeconds(0.1f);

        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusic);
        }
        else if (AudioManager.Instance == null)
        {
            // Se ancora non lo trova, ti stampa un errore rosso nella Console per avvisarti!
            Debug.LogError("ATTENZIONE: LevelMusic non trova l'AudioManager! Controlla di non averlo messo come 'Figlio' di un altro oggetto.");
        }
    }
}