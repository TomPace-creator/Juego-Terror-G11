using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;

    private void Start()
    {
        subtitleText.text = ""; // Arranca invisible
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSubtitleTriggered += DisplaySubtitle;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSubtitleTriggered -= DisplaySubtitle;
        }
    }

    private void DisplaySubtitle(string text, float duration)
    {
        StopAllCoroutines(); // Si había otro subtítulo, lo cortamos
        StartCoroutine(SubtitleRoutine(text, duration));
    }

    private IEnumerator SubtitleRoutine(string text, float duration)
    {
        subtitleText.text = text; // Mostramos el pensamiento
        yield return new WaitForSeconds(duration); // Esperamos el tiempo que le digamos
        subtitleText.text = ""; // Lo borramos
    }
}