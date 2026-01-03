using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInEffect : MonoBehaviour
{
    public CanvasGroup blackOverlay;
    public float fadeDuration = 2.0f;
    void Start()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            blackOverlay.alpha = 1.0f - (elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        blackOverlay.alpha = 0f;
        blackOverlay.gameObject.SetActive(false);
    }
}
