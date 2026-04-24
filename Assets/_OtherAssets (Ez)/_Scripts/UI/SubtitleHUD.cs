using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;

    private void Start()
    {
        subtitleText.text = ""; 
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
        StopAllCoroutines(); // corta el sub anterior
        StartCoroutine(SubtitleRoutine(text, duration));
    }

    private IEnumerator SubtitleRoutine(string text, float duration)
    {
        subtitleText.text = text;
        yield return new WaitForSeconds(duration); 
        subtitleText.text = ""; 
    }
}