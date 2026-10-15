
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialMessage : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text messageText;
    public CanvasGroup canvasGroup;

    [Header("Timing")]
    public float visibleDuration = 10f;
    public float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (messageText != null)
            messageText.text = "";
    }

    public void ShowMessage(string message)
    {
        if (messageText == null || canvasGroup == null)
        {
            Debug.LogWarning(
                "Assign Message Text and Canvas Group in TutorialMessage."
            );
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        messageText.text = message;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        fadeCoroutine = StartCoroutine(FadeOutAfterDelay());
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(visibleDuration);

        float elapsed = 0f;
        float startingAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                startingAlpha,
                0f,
                elapsed / fadeDuration
            );

            yield return null;
        }

        canvasGroup.alpha = 0f;
        messageText.text = "";
        fadeCoroutine = null;
    }

    public void ClearMessage()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (messageText != null)
            messageText.text = "";
    }
}
