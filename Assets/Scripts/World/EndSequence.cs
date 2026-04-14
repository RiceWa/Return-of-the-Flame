using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndSequence : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private TextMeshProUGUI endText;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float textDelay = 0.5f;
    [SerializeField] private float textFadeDuration = 1f;

    public void PlayEndSequence(float finalTime)
    {
        StartCoroutine(EndRoutine(finalTime));
    }

    private IEnumerator EndRoutine(float finalTime)
{
    // Fade screen to black
    float timer = 0f;
    Color panelColor = fadePanel.color;

    while (timer < fadeDuration)
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / fadeDuration);
        fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
        yield return null;
    }

    fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, 1f);

    // Small pause before text
    yield return new WaitForSeconds(0.5f);

    // Set final text
    endText.text = "The flame has returned in\n" + finalTime.ToString("F2") + " seconds";

    // Fade text in
    timer = 0f;
    Color textColor = endText.color;

    while (timer < textFadeDuration)
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / textFadeDuration);
        endText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
        yield return null;
    }

    endText.color = new Color(textColor.r, textColor.g, textColor.b, 1f);

    // Wait 5 seconds before returning to menu
    yield return new WaitForSeconds(5f);

    // Load Main Menu scene
    SceneManager.LoadScene("MainMenu");
}
}