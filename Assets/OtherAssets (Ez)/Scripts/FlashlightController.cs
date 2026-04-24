using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class FlashlightController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Light playerLight;
    [SerializeField] private string requiredItem = "Linterna";

    [Header("HUD")]
    [SerializeField] private GameObject flashlightIconHUD;
    [SerializeField] private Image batteryFillImage;
    [Tooltip("El texto en el Canvas que dirá x0, x1, x2...")]
    [SerializeField] private TextMeshProUGUI batteryCountText;

    [Header("Misión Secundaria (Falta de Energía)")]
    [SerializeField] private string batteryQuestTitle = "[Opcional] A oscuras";
    [SerializeField] private string batteryQuestDetails = "Encuentra pilas de repuesto por la casa.";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip flickerSound;
    [SerializeField] private AudioClip reloadSound;

    [Header("Configuración de Batería")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainRate = 1.5f;
    [SerializeField] private string batteryItemName = "Pilas";

    public float currentBattery;

    [Header("Configuración de Parpadeo")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float minFlickerWait = 10f;
    [SerializeField] private float maxFlickerWait = 30f;

    private float defaultIntensity;
    private Coroutine flickerCoroutine;
    private bool hasUnlockedFlashlight = false;

    private void Start()
    {
        currentBattery = maxBattery;

        if (playerLight != null)
        {
            defaultIntensity = playerLight.intensity;
            playerLight.enabled = false;
        }

        if (flashlightIconHUD != null)
        {
            flashlightIconHUD.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Mostrar el icono cuando lo agarramos
        if (!hasUnlockedFlashlight && GameManager.Instance != null && GameManager.Instance.HasItem(requiredItem))
        {
            hasUnlockedFlashlight = true;
            if (flashlightIconHUD != null) flashlightIconHUD.SetActive(true);
        }

        // 1. PRENDER / APAGAR
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null && GameManager.Instance.HasItem(requiredItem))
            {
                if (!playerLight.enabled && currentBattery <= 0)
                {
                    if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound, 0.3f);

                    // Si intentamos prenderla y no tiene batería ni tenemos repuestos, lanzamos la misión
                    CheckAndTriggerBatteryQuest();
                    return;
                }

                ToggleFlashlight();
            }
        }

        // 2. RECARGAR
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            TryReload();
        }

        // 3. DRENAR BATERÍA
        if (playerLight.enabled)
        {
            currentBattery -= Time.deltaTime * drainRate;

            if (currentBattery <= 0)
            {
                currentBattery = 0;
                ToggleFlashlight();

                // Se nos murió la linterna en la mano. Revisamos si hay que lanzar la misión.
                CheckAndTriggerBatteryQuest();
            }
        }

        // 4. ACTUALIZAR UI CONSTANTEMENTE
        UpdateBatteryUI();
    }

    private void UpdateBatteryUI()
    {
        if (batteryFillImage != null)
        {
            batteryFillImage.fillAmount = currentBattery / maxBattery;
        }

        if (batteryCountText != null && GameManager.Instance != null)
        {
            int pilasEnInventario = GameManager.Instance.GetItemCount(batteryItemName);
            batteryCountText.text = "x" + pilasEnInventario.ToString();
        }
    }

    private void TryReload()
    {
        if (currentBattery >= maxBattery) return;

        if (GameManager.Instance != null && GameManager.Instance.HasItem(batteryItemName))
        {
            // ¡Recarga exitosa!
            GameManager.Instance.RemoveItemFromInventory(batteryItemName);
            currentBattery = maxBattery;

            if (audioSource != null && reloadSound != null) audioSource.PlayOneShot(reloadSound);
            GameManager.Instance.ShowSubtitle("<i>Batería reemplazada.</i>", 2f);

            // Borramos la misión secundaria porque ya logramos recargar
            if (!string.IsNullOrEmpty(batteryQuestTitle))
            {
                GameManager.Instance.UpdateSecondaryMission(batteryQuestTitle, "");
            }
        }
        else
        {
            // Intentó recargar pero no tiene pilas
            GameManager.Instance.ShowSubtitle("<i>Me quedé sin pilas...</i>", 2f);
            CheckAndTriggerBatteryQuest();
        }
    }

    // Nueva función para no repetir código. Evalúa si debe lanzar la misión.
    private void CheckAndTriggerBatteryQuest()
    {
        if (GameManager.Instance != null && GameManager.Instance.GetItemCount(batteryItemName) == 0)
        {
            if (!string.IsNullOrEmpty(batteryQuestTitle))
            {
                GameManager.Instance.UpdateSecondaryMission(batteryQuestTitle, batteryQuestDetails);
            }
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