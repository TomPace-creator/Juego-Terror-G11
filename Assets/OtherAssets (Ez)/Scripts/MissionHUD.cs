using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class MissionHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI missionText;

    [Header("Formatting (Juice)")]
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float animationSpeed = 0.2f;
    [SerializeField] private int flickerAmount = 3;

    [Header("Tamanos")]
    [Tooltip("El tamaño del título en porcentaje relativo al tamaño base del texto (ej: 0.8 = 80%)")]
    [SerializeField, Range(0f, 1f)] private float titleFontSizePercent = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioClip updateSound;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionChanged += RefreshText;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionChanged -= RefreshText;
        }
    }

    private void RefreshText(string title, string details)
    {
        StopAllCoroutines();
        StartCoroutine(UnfoldText(title, details));
    }

    private IEnumerator UnfoldText(string title, string details)
    {
        // Si mandamos un texto vacío, borramos el texto para que no quede el [Principal] flotando
        if (string.IsNullOrEmpty(title))
        {
            missionText.text = "";
            yield break;
        }

        // --- MAGIA DE FORMATO ---
        string titlePercentString = (titleFontSizePercent * 100).ToString("F0") + "%";

        // 1. Mostramos la etiqueta y el título achicado (SIN los dos puntos al final)
        missionText.text = "<color=#FF3333><b>[Principal]</b> <size=" + titlePercentString + ">" + title + "</size></color>";

        // 2. Reproducimos el sonido de actualización
        if (updateSound != null)
        {
            audioSource.PlayOneShot(updateSound);
        }

        // 3. Ejecutamos la animación de Pop y Flicker al mismo tiempo
        yield return StartCoroutine(JuiceAnimation());

        // 4. Esperamos los 5 segundos de suspenso
        yield return new WaitForSeconds(5f);

        // 5. Desplegamos el detalle abajo, manteniendo el formato del título arriba (SIN los dos puntos)
        missionText.text = "<color=#FF3333><b>[Principal]</b> <size=" + titlePercentString + ">" + title + "</size></color>\n<color=#A0A0A0><size=60%>" + details + "</size></color>";
    }

    // --- LA MAGIA VISUAL ---
    private IEnumerator JuiceAnimation()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

        // FASE 1: Se agranda rápido (POP!)
        float timer = 0;
        while (timer < animationSpeed)
        {
            timer += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, timer / animationSpeed);
            yield return null;
        }

        // FASE 2: Titila el HUD simulando falla eléctrica (Flicker)
        for (int i = 0; i < flickerAmount; i++)
        {
            canvasGroup.alpha = 0.2f;
            yield return new WaitForSeconds(0.08f);

            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.08f);
        }

        // FASE 3: Vuelve a achicarse a su tamaño normal
        timer = 0;
        while (timer < animationSpeed)
        {
            timer += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, timer / animationSpeed);
            yield return null;
        }

        // Nos aseguramos de que quede perfecto al terminar
        rectTransform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }
}