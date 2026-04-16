using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))] // Se agrega solo en Unity para controlar la transparencia
[RequireComponent(typeof(AudioSource))] // Se agrega solo para el sonido
public class MissionHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI missionText;

    [Header("Efecto Visual (Juice)")]
    [SerializeField] private float scaleMultiplier = 1.2f; // Se agranda un 20%
    [SerializeField] private float animationSpeed = 0.2f;  // Velocidad del salto
    [SerializeField] private int flickerAmount = 3;        // Cantidad de parpadeos

    [Header("Audio")]
    [SerializeField] private AudioClip updateSound; // El sonido de radio/interferencia

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();

        // Configuramos el audio para que no se escuche en 3D sino plano en la UI
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
        // 1. Mostramos solo el título
        missionText.text = "<size=80%>" + title + "</size>";

        // 2. Reproducimos el sonido de actualización
        if (updateSound != null)
        {
            audioSource.PlayOneShot(updateSound);
        }

        // 3. Ejecutamos la animación de Pop y Flicker al mismo tiempo
        yield return StartCoroutine(JuiceAnimation());

        // 4. Esperamos los 10 segundos de suspenso que tenías configurados
        yield return new WaitForSeconds(5f);

        // 5. Desplegamos el detalle abajo
        missionText.text = "<size=80%>" + title + "</size>\n<color=#A0A0A0><size=60%>" + details + "</size></color>";

        // Opcional: Podrías poner otro sonidito más suave acá para indicar que apareció el subtítulo
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