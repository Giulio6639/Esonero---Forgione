using System.Collections;
using UnityEngine;

public class UI_Fade : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;
    public Coroutine fadeEffectCo { get; private set; }
    
    private void Start()
    {
        DoFadeIn();
    }

    public void DoFadeIn()
    {
        canvasGroup.blocksRaycasts = false;
        FadeEffect(0);
    }

    public void DoFadeOut()
    {
        canvasGroup.blocksRaycasts = true;
        FadeEffect(1);
    }

    private void FadeEffect(float targetAlpha)
    {
        if (fadeEffectCo != null)
        {
            StopCoroutine(fadeEffectCo);
        }

        fadeEffectCo = StartCoroutine(ChangeAlphaCo(targetAlpha));
    }

    private IEnumerator ChangeAlphaCo(float targetAlpha)
    {
        float timePassed = 0;
        float startAlpha = canvasGroup.alpha;

        while (timePassed < fadeDuration)
        {
            timePassed = timePassed + Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timePassed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
