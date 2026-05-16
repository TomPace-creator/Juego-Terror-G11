using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // Necesario para detectar el TAB

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

    [Header("Visibilidad y Control (TAB)")]
    [Tooltip("Segundos que los detalles se quedan en pantalla antes de ocultarse")]
    [SerializeField] private float autoHideTime = 4f;
    [Tooltip("Velocidad de desvanecimiento del HUD")]
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip updateSound;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;

    private float currentDisplayTimer = 0f;
    private bool isAnimatingJuice = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        canvasGroup.alpha = 0f; // Asegura que empiece oculto al abrir el juego
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

    private void Update()
    {
        // Si no hay teclado o se está ejecutando la animación de parpadeo, detenemos la lógica de visibilidad
        if (Keyboard.current == null || isAnimatingJuice) return;

        bool isTabPressed = Keyboard.current.tabKey.isPressed;

        if (currentDisplayTimer > 0f)
        {
            currentDisplayTimer -= Time.deltaTime;
        }

        // Determina si el objetivo del Alpha es 1 (visible) o 0 (oculto)
        float targetAlpha = (currentDisplayTimer > 0f || isTabPressed) ? 1f : 0f;

        // Aplica el efecto suave de fade
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    private void RefreshText(string title, string details)
    {
        StopAllCoroutines();
        StartCoroutine(UnfoldText(title, details));
    }

    private IEnumerator UnfoldText(string title, string details)
    {
        if (string.IsNullOrEmpty(title))
        {
            missionText.text = "";
            yield break;
        }

        // Bloqueamos el Update para tomar control total del CanvasGroup durante la animación
        isAnimatingJuice = true;
        canvasGroup.alpha = 1f;

        string titlePercentString = (titleFontSizePercent * 100).ToString("F0") + "%";
        missionText.text = "<color=#FF3333><b>[Principal]</b> <size=" + titlePercentString + ">" + title + "</size></color>";

        if (updateSound != null)
        {
            audioSource.PlayOneShot(updateSound);
        }

        yield return StartCoroutine(JuiceAnimation());

        // Devolvemos el control al Update
        isAnimatingJuice = false;

        // Le damos tiempo para que muestre el título principal antes de revelar los detalles (tus 5 segundos originales)
        currentDisplayTimer = 5f;
        yield return new WaitForSeconds(5f);

        // Agrega los detalles a la misión
        missionText.text = "<color=#FF3333><b>[Principal]</b> <size=" + titlePercentString + ">" + title + "</size></color>\n<color=#A0A0A0><size=60%>" + details + "</size></color>";

        // Reinicia el timer para que el jugador tenga tiempo de leer la nueva información
        currentDisplayTimer = autoHideTime;
    }

    private IEnumerator JuiceAnimation()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

        float timer = 0;
        while (timer < animationSpeed)
        {
            timer += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, timer / animationSpeed);
            yield return null;
        }

        for (int i = 0; i < flickerAmount; i++)
        {
            canvasGroup.alpha = 0.2f;
            yield return new WaitForSeconds(0.08f);

            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.08f);
        }

        timer = 0;
        while (timer < animationSpeed)
        {
            timer += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(targetScale, originalScale, timer / animationSpeed);
            yield return null;
        }

        rectTransform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }
}