using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;
    private UI_Fade fadeUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ChangeLevelTo(string levelName)
    {
        StartCoroutine(ChangeLevelCo(levelName));
    }

    private IEnumerator ChangeLevelCo(string levelName)
    {
        // 1. Fai diventare lo schermo nero
        GetFadeUI().DoFadeOut();
        yield return GetFadeUI().fadeEffectCo; // Aspetta che finisca il fade out

        // 2. Carica la scena in modo ASINCRONO e aspetta che finisca
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        while (!asyncLoad.isDone)
        {
            yield return null; // Aspetta il frame successivo finché non ha finito
        }

        // 3. Ora che la scena è caricata, riporta lo schermo trasparente
        GetFadeUI().DoFadeIn();
    }

    private UI_Fade GetFadeUI()
    {
        if (fadeUI == null)
            fadeUI = FindFirstObjectByType<UI_Fade>();
        return fadeUI;
    }
}