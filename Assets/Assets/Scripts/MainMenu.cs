using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Impostazioni Livello")]
    [Tooltip("Il nome esatto della scena del primo livello")]
    public string firstLevelName = "Level01";

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelName);
    }

    public void QuitGame()
    {
        Debug.Log("Il gioco è stato chiuso!");
        Application.Quit();
    }
}