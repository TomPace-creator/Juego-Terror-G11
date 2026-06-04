using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // Necesario para usar Listas/Arrays

[RequireComponent(typeof(Image))]
public class UIShadowHallucination : MonoBehaviour
{
    private enum HallucinationType { CrossScreen, Approach }

    [Header("Variedad de Imágenes (NUEVO)")]
    [Tooltip("Arrastra aquí todas las siluetas que cruzarán la pantalla aleatoriamente.")]
    [SerializeField] private Sprite[] crossScreenSprites; // Array de imágenes para variedad

    [Tooltip("LA imagen específica que se usará ÚNICAMENTE cuando la sombra se abalance a la cámara.")]
    [SerializeField] private Sprite approachSpecificSprite; // Imagen única para el jumpscare

    [Header("Configuración de Velocidad (Cross)")]
    [SerializeField] private float minSpeed = 2500f;
    [SerializeField] private float maxSpeed = 4500f;

    [Header("Configuración de Tamaño (Cross)")]
    [SerializeField] private float minScale = 0.7f;
    [SerializeField] private float maxScale = 1.3f;

    [Header("Configuración de Tiempo (Frecuencia)")]
    [SerializeField] private float minDelay = 1.5f;
    [SerializeField] private float maxDelay = 5f;

    [Header("Configuración del 'Acercamiento'")]
    [SerializeField, Range(0f, 1f)] private float approachFrequency = 0.3f;
    [SerializeField] private float approachDuration = 0.4f;
    [SerializeField] private float approachMaxScale = 10f;

    private RectTransform rectTransform;
    private Image shadowImage;
    private Color originalColor;
    private float offScreenOffset = 900f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        shadowImage = GetComponent<Image>();
        originalColor = shadowImage.color;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        shadowImage.enabled = false;

        // Validación de seguridad para el desarrollador
        if (crossScreenSprites == null || crossScreenSprites.Length == 0)
        {
            Debug.LogError($"[UIShadowHallucination] en {gameObject.name} no tiene Sprites asignados en 'Cross Screen Sprites'.");
        }
        if (approachSpecificSprite == null)
        {
            Debug.LogError($"[UIShadowHallucination] en {gameObject.name} no tiene el Sprite específico asignado en 'Approach Specific Sprite'.");
        }
    }

    private void OnEnable()
    {
        StartCoroutine(HallucinationLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (shadowImage != null) shadowImage.enabled = false;
    }

    private IEnumerator HallucinationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            HallucinationType chosenType = (Random.value < approachFrequency) ? HallucinationType.Approach : HallucinationType.CrossScreen;

            if (chosenType == HallucinationType.CrossScreen)
            {
                yield return StartCoroutine(DoCrossScreen());
            }
            else
            {
                yield return StartCoroutine(DoApproach());
            }

            shadowImage.enabled = false;
        }
    }

    // --- PATRÓN 1: CRUCES LATERALES (Variedad de imágenes) ---
    private IEnumerator DoCrossScreen()
    {
        // --- NUEVO: ASIGNAR IMAGEN ALEATORIA DE LA LISTA ---
        if (crossScreenSprites != null && crossScreenSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, crossScreenSprites.Length);
            shadowImage.sprite = crossScreenSprites[randomIndex];
        }

        shadowImage.enabled = true;
        shadowImage.color = originalColor;

        float randomScale = Random.Range(minScale, maxScale);
        rectTransform.localScale = new Vector3(randomScale, randomScale, 1f);

        float currentSpeed = Random.Range(minSpeed, maxSpeed);

        float minY = -(Screen.height / 2f);
        float maxY = 0f;
        float randomY = Random.Range(minY + 100f, maxY - 100f);

        bool moveRight = Random.value > 0.5f;
        float startX = moveRight ? -(Screen.width / 2f + offScreenOffset) : (Screen.width / 2f + offScreenOffset);
        float targetX = moveRight ? (Screen.width / 2f + offScreenOffset) : -(Screen.width / 2f + offScreenOffset);

        rectTransform.anchoredPosition = new Vector2(startX, randomY);
        rectTransform.localScale = new Vector3(moveRight ? randomScale : -randomScale, randomScale, 1f);

        while (Mathf.Abs(rectTransform.anchoredPosition.x - targetX) > 20f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(
                rectTransform.anchoredPosition,
                new Vector2(targetX, randomY),
                currentSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    // --- PATRÓN 2: EL ACERCAMIENTO GIGANTE (Imagen Específica) ---
    private IEnumerator DoApproach()
    {
        // --- NUEVO: ASIGNAR LA IMAGEN ESPECÍFICA DEL SUSTO ---
        if (approachSpecificSprite != null)
        {
            shadowImage.sprite = approachSpecificSprite;
        }

        shadowImage.enabled = true;

        float groundY = -(Screen.height / 3f);
        rectTransform.anchoredPosition = new Vector2(0f, groundY);
        rectTransform.localScale = Vector3.one * 0.1f;

        shadowImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        float timer = 0f;
        while (timer < approachDuration)
        {
            timer += Time.deltaTime;
            float percent = Mathf.Clamp01(timer / approachDuration);

            rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.1f, Vector3.one * approachMaxScale, percent);

            shadowImage.color = Color.Lerp(new Color(originalColor.r, originalColor.g, originalColor.b, 0f), originalColor, percent);

            yield return null;
        }

        rectTransform.localScale = Vector3.one * approachMaxScale;
        shadowImage.color = originalColor;

        yield return new WaitForSeconds(0.1f);
    }
}