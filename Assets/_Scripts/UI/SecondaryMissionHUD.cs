using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class SecondaryMissionHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI secondaryMissionText;

    [Header("Formatting (Juice)")]
    [Tooltip("Cuánto crece el texto al actualizarse")]
    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float animationSpeed = 0.2f;
    [SerializeField] private int flickerAmount = 3;

    [Header("Visibilidad y Control (TAB)")]
    [Tooltip("Segundos que la misión se queda en pantalla antes de ocultarse")]
    [SerializeField] private float autoHideTime = 4f;
    [Tooltip("Velocidad de desvanecimiento del HUD")]
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip updateSound;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private Dictionary<string, string> activeMissions = new Dictionary<string, string>();

    private float currentDisplayTimer = 0f;
    private bool isAnimatingJuice = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        canvasGroup.alpha = 0f; // Asegura que inicie invisible
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSecondaryMissionChanged += ManageMissions;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSecondaryMissionChanged -= ManageMissions;
        }
    }

    private void Update()
    {
        // Si está en medio de la animación de parpadeo, no alteramos el Alpha
        if (Keyboard.current == null || isAnimatingJuice) return;

        bool isTabPressed = Keyboard.current.tabKey.isPressed;

        if (currentDisplayTimer > 0f)
        {
            currentDisplayTimer -= Time.deltaTime;
        }

        float targetAlpha = (activeMissions.Count > 0 && (currentDisplayTimer > 0f || isTabPressed)) ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    private void ManageMissions(string title, string details)
    {
        bool isNewMission = false;

        if (string.IsNullOrEmpty(title))
        {
            activeMissions.Clear();
        }
        else if (string.IsNullOrEmpty(details))
        {
            if (activeMissions.ContainsKey(title))
            {
                activeMissions.Remove(title);
            }
        }
        else
        {
            if (!activeMissions.ContainsKey(title))
            {
                isNewMission = true;
            }
            activeMissions[title] = details;
        }

        StopAllCoroutines();
        StartCoroutine(UpdateVisuals(isNewMission));
    }

    private IEnumerator UpdateVisuals(bool isNewMission)
    {
        if (activeMissions.Count == 0)
        {
            secondaryMissionText.text = "";
            currentDisplayTimer = 0f;
            yield break;
        }

        // 1. Armamos todo el texto del diccionario
        string finalText = "";
        foreach (KeyValuePair<string, string> mission in activeMissions)
        {
            finalText += "<color=#FFD700><size=70%>" + mission.Key + "</size></color>\n<color=#A0A0A0><size=55%>" + mission.Value + "</size></color>\n\n";
        }
        secondaryMissionText.text = finalText;

        // 2. Si hay una misión nueva, hacemos el show (Sonido + Parpadeo)
        if (isNewMission)
        {
            if (updateSound != null)
            {
                audioSource.PlayOneShot(updateSound);
            }

            isAnimatingJuice = true;
            canvasGroup.alpha = 1f; // Lo forzamos a ser visible al instante
            yield return StartCoroutine(JuiceAnimation());
            isAnimatingJuice = false;
        }

        // 3. Reiniciamos el tiempo para que se quede en pantalla antes de desvanecerse
        currentDisplayTimer = autoHideTime;
    }

    // --- LA ANIMACIÓN DE PARPADEO (JUICE) ---
    private IEnumerator JuiceAnimation()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);

        // Agrandar
        float timer = 0;
        while (timer < animationSpeed)
        {
            timer += Time.deltaTime;
            rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, timer / animationSpeed);
            yield return null;
        }

        // Parpadear
        for (int i = 0; i < flickerAmount; i++)
        {
            canvasGroup.alpha = 0.2f;
            yield return new WaitForSeconds(0.08f);

            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(0.08f);
        }

        // Achicar
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