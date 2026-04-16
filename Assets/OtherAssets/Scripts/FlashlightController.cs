using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FlashlightController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Light playerLight;
    [SerializeField] private string requiredItem = "Linterna";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip flickerSound;
    [SerializeField] private AudioClip reloadSound; // Sonido de poner pilas nuevas

    [Header("Configuración de Batería")]
    [SerializeField] private float maxBattery = 100f;
    [Tooltip("Cuánta batería gasta por segundo")]
    [SerializeField] private float drainRate = 1.5f;
    [SerializeField] private string batteryItemName = "Pilas"; // El nombre que le pondrás al PickUpItem

    // Público para que si luego haces una barra de energía en el HUD, la pueda leer
    public float currentBattery;

    [Header("Configuración de Parpadeo")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float minFlickerWait = 10f;
    [SerializeField] private float maxFlickerWait = 30f;

    private float defaultIntensity;
    private Coroutine flickerCoroutine;

    private void Start()
    {
        currentBattery = maxBattery; // Arrancas la noche con la batería al 100%

        if (playerLight != null)
        {
            defaultIntensity = playerLight.intensity;
            playerLight.enabled = false;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // --- 1. PRENDER / APAGAR (Tecla F) ---
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null && GameManager.Instance.HasItem(requiredItem))
            {
                // Si la quieres prender pero tienes 0 batería, no te deja (y hace ruido de fallo)
                if (!playerLight.enabled && currentBattery <= 0)
                {
                    if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound, 0.3f);
                    return;
                }

                ToggleFlashlight();
            }
        }

        // --- 2. RECARGAR (Tecla R) ---
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            TryReload();
        }

        // --- 3. DRENAR BATERÍA ---
        if (playerLight.enabled)
        {
            currentBattery -= Time.deltaTime * drainRate;

            // ¡Se acabó la luz!
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                ToggleFlashlight(); // Forzamos el apagado
            }
        }
    }

    private void TryReload()
    {
        // Si ya está llena, no desperdiciamos pilas
        if (currentBattery >= maxBattery) return;

        if (GameManager.Instance != null && GameManager.Instance.HasItem(batteryItemName))
        {
            // 1. Gastamos la pila de tu inventario
            GameManager.Instance.RemoveItemFromInventory(batteryItemName);

            // 2. Llenamos la energía al máximo
            currentBattery = maxBattery;

            // 3. Sonido y texto
            if (audioSource != null && reloadSound != null) audioSource.PlayOneShot(reloadSound);
            GameManager.Instance.ShowSubtitle("<i>Batería reemplazada.</i>", 2f);
        }
        else
        {
            // Feedback si apretas R y no tienes pilas
            GameManager.Instance.ShowSubtitle("<i>Me quedé sin pilas...</i>", 2f);
        }
    }

    private void ToggleFlashlight()
    {
        if (playerLight == null) return;

        playerLight.enabled = !playerLight.enabled;

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        if (playerLight.enabled && enableFlicker)
        {
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }
        else
        {
            if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
            playerLight.intensity = defaultIntensity;
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minFlickerWait, maxFlickerWait);
            yield return new WaitForSeconds(waitTime);

            int flickerCount = Random.Range(2, 6);
            for (int i = 0; i < flickerCount; i++)
            {
                playerLight.intensity = Random.Range(0.1f, defaultIntensity * 0.3f);

                if (audioSource != null && flickerSound != null)
                {
                    audioSource.PlayOneShot(flickerSound, 0.4f);
                }

                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

                playerLight.intensity = defaultIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
        }
    }
}