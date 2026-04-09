using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance; // Per accedervi da altri script, se serve
    public static bool isGamePaused = false; // Variabile globale per sapere se siamo in pausa

    [Header("UI Riferimenti")]
    public GameObject pauseMenuPanel; // Trascina qui il pannello della pausa

    [Header("Impostazioni Uscita")]
    public string mainMenuSceneName = "MainMenu"; // Il nome della tua scena del menu principale

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ci assicuriamo che il gioco parta senza pausa e col pannello nascosto
        pauseMenuPanel.SetActive(false);
        isGamePaused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Intercettiamo il tasto ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1. SE IL GIOCO E' GIA' IN PAUSA, LO RIPRENDIAMO
            if (isGamePaused)
            {
                Resume();
            }
            // 2. SE NON E' IN PAUSA, VERIFICHIAMO CHE NON CI SIANO ALTRI MENU APERTI
            else
            {
                // Controlliamo se c'è un dialogo, il negozio o l'inventario a schermo
                bool isShopOpen = ShopManager.Instance != null && ShopManager.Instance.shopPanel.activeSelf;
                bool isDialogueOpen = DialogueController.isDialogueActive;
                bool isInventoryOpen = InventoryManager.isInventoryOpen;

                // Se uno di questi menu è aperto, NON apriamo la pausa (ci penseranno loro a chiudersi)
                if (isShopOpen || isDialogueOpen || isInventoryOpen)
                {
                    return;
                }

                // Se è tutto libero, mettiamo in pausa!
                Pause();
            }
        }
    }

    // --- METODI PUBBLICI (DA ASSEGNARE AI BOTTONI DELLA UI) ---

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeMusic();// Il tempo riparte
        isGamePaused = false;
    }

    private void Pause()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Il tempo si ferma
        if (AudioManager.Instance != null) 
            AudioManager.Instance.PauseMusic();
        isGamePaused = true;
    }

    public void LoadMenu()
    {
        // FONDAMENTALE: Rimettiamo il tempo a 1 PRIMA di caricare la scena, 
        // altrimenti il Menu Principale sarà congelato!
        Time.timeScale = 1f;
        isGamePaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Uscita dal gioco...");
        Application.Quit(); // Nota: Application.Quit funziona solo nella build esportata, non nell'editor di Unity.
    }
}