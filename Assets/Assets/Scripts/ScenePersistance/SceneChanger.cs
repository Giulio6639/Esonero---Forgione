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
        }


        DontDestroyOnLoad(gameObject);
    }

    public void ChangeLevelTo(string levelName)
    {
        StartCoroutine(ChangeLevelCo(levelName));   
    }

    private IEnumerator ChangeLevelCo(string levelName)
    {
        GetFadeUI().DoFadeOut();

        yield return GetFadeUI().fadeEffectCo;

        SceneManager.LoadScene(levelName);
    }

    private UI_Fade GetFadeUI()
    {
        if (fadeUI == null)
            fadeUI = FindFirstObjectByType<UI_Fade>();
        return fadeUI;
    }
}