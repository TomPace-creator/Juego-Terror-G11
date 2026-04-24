using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class PlayerSanity : MonoBehaviour
{
    [Header("Configuración de Cordura")]
    [SerializeField] private float maxSanity = 100f;
    public float currentSanity;

    [Header("HUD (Efecto Visual)")]
    [SerializeField] private Image sanityVignette;

    [Header("Visión de Túnel (Escala)")]
    [Tooltip("Tamaño del círculo cuando estás 100% cuerdo (visión abierta)")]
    [SerializeField] private float maxVisionScale = 1.8f;
    [Tooltip("Tamaño del círculo cuando estás en 0% de cordura (visión cerrada)")]
    [SerializeField] private float minVisionScale = 0.8f;

    [Header("Efecto de Latido")]
    [SerializeField] private bool enableHeartbeat = true;
    [SerializeField] private float minBeatSpeed = 3f;
    [SerializeField] private float maxBeatSpeed = 10f;
    [Tooltip("Cuánto se expande la imagen en cada latido (ej: 0.1)")]
    [SerializeField] private float pulseExpansion = 0.1f;

    private RectTransform vignetteRect;

    private void Start()
    {
        //cordura al 70
        currentSanity = maxSanity * 0.7f;

        if (sanityVignette != null)
        {
            vignetteRect = sanityVignette.GetComponent<RectTransform>();
        }

        UpdateSanityUI();
    }

    private void Update()
    {
        if (sanityVignette != null)
        {
            if (enableHeartbeat && currentSanity < maxSanity)
            {
                HandleVisionAndHeartbeat();
            }
            else if (vignetteRect != null)
            {
                // latido efecto hud
                vignetteRect.localScale = new Vector3(maxVisionScale, maxVisionScale, 1f);
            }
        }
    }

    private void HandleVisionAndHeartbeat()
    {
        float sanityPercentage = currentSanity / maxSanity;
        float insanityFactor = 1f - sanityPercentage;

       
        float baseScale = Mathf.Lerp(minVisionScale, maxVisionScale, sanityPercentage);

       
        float currentBeatSpeed = Mathf.Lerp(minBeatSpeed, maxBeatSpeed, insanityFactor);

       
        float wave = (Mathf.Sin(Time.time * currentBeatSpeed) + 1f) / 2f;

        float finalScale = baseScale + (wave * pulseExpansion);

        vignetteRect.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    public void LoseSanity(float amount)
    {
        currentSanity -= amount;
        if (currentSanity <= 0)
        {
            currentSanity = 0;
            TriggerInsanityGameOver();
        }
        UpdateSanityUI();
    }

    public void RestoreSanity(float amount)
    {
        currentSanity += amount;
        if (currentSanity > maxSanity)
        {
            currentSanity = maxSanity;
        }
        UpdateSanityUI();
    }

    private void UpdateSanityUI()
    {
        if (sanityVignette != null)
        {
            float sanityPercentage = currentSanity / maxSanity;
            float targetAlpha = 1f - sanityPercentage;

            Color currentColor = sanityVignette.color;
            currentColor.a = targetAlpha;
            sanityVignette.color = currentColor;
        }
    }

    private void TriggerInsanityGameOver()
    {
        Debug.Log("¡CORDURA AL CERO! Cargando escena Lose...");

       
        SceneManager.LoadScene("Lose");
    }

   
    public void RestoreSanityGradual(float amount, float duration)
    {
        StartCoroutine(GradualRestoreRoutine(amount, duration));

        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyPillsConsumed();
        }
    }

    private System.Collections.IEnumerator GradualRestoreRoutine(float amount, float duration)
    {
        float timePassed = 0;
        float sanityPerSecond = amount / duration;

        while (timePassed < duration)
        {
            currentSanity += sanityPerSecond * Time.deltaTime;

            if (currentSanity > maxSanity)
            {
                currentSanity = maxSanity;
            }

            UpdateSanityUI();

            timePassed += Time.deltaTime;
            yield return null;
        }
    }
}