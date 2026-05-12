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

    [Header("Detección de Enemigo")]
    [Tooltip("Distancia a la que la luz puede frenar al enemigo")]
    [SerializeField] private float flashlightRange = 25f;
    [Tooltip("Grosor del raycast linterna")] 
    [SerializeField] private float flashlightRadius = 2.5f; 
    [Tooltip("Multiplicador de gasto de batería al iluminar al monstruo (Ej: 1.5 gasta un 50% más rápido)")]
    [SerializeField] private float stunDrainMultiplier = 1.5f;

    [Header("HUD")]
    [SerializeField] private GameObject flashlightIconHUD;
    [SerializeField] private Image batteryFillImage;
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

        if (flashlightIconHUD != null) flashlightIconHUD.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (!hasUnlockedFlashlight && GameManager.Instance != null && GameManager.Instance.HasItem(requiredItem))
        {
            hasUnlockedFlashlight = true;
            if (flashlightIconHUD != null) flashlightIconHUD.SetActive(true);
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null && GameManager.Instance.HasItem(requiredItem))
            {
                if (!playerLight.enabled && currentBattery <= 0)
                {
                    if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound, 0.3f);
                    CheckAndTriggerBatteryQuest();
                    return;
                }
                ToggleFlashlight();
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) TryReload();

        if (playerLight != null && playerLight.enabled)
        {
            // Evaluamos si estamos aturdiendo al enemigo en este frame
            bool isStunningEnemy = StunEnemyIfLooking();

            // Calculamos el drenaje: si lo miramos, se gasta más rápido
            float currentDrain = isStunningEnemy ? (drainRate * stunDrainMultiplier) : drainRate;
            currentBattery -= Time.deltaTime * currentDrain;

            if (currentBattery <= 0)
            {
                currentBattery = 0;
                ToggleFlashlight();
                CheckAndTriggerBatteryQuest();
            }
        }

        UpdateBatteryUI();
    }

    // Ahora devuelve un BOOL para saber si acertamos
    private bool StunEnemyIfLooking()
    {
        // Cambiamos Physics.Raycast por Physics.SphereCast y le pasamos el flashlightRadius
        if (Physics.SphereCast(transform.position, flashlightRadius, transform.forward, out RaycastHit hit, flashlightRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy == null) enemy = hit.collider.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.StunByFlashlight();
                return true; // Acertamos al monstruo
            }
        }
        return false; // No le estamos dando a nada
    }

    private void UpdateBatteryUI()
    {
        if (batteryFillImage != null) batteryFillImage.fillAmount = currentBattery / maxBattery;
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
            GameManager.Instance.RemoveItemFromInventory(batteryItemName);
            currentBattery = maxBattery;
            if (audioSource != null && reloadSound != null) audioSource.PlayOneShot(reloadSound);
            GameManager.Instance.ShowSubtitle("<i>Pilas reemplazadas.</i>", 2f);
            if (!string.IsNullOrEmpty(batteryQuestTitle)) GameManager.Instance.UpdateSecondaryMission(batteryQuestTitle, "");
        }
        else
        {
            GameManager.Instance.ShowSubtitle("<i>Me quedé sin pilas...</i>", 2f);
            CheckAndTriggerBatteryQuest();
        }
    }

    private void CheckAndTriggerBatteryQuest()
    {
        if (GameManager.Instance != null && GameManager.Instance.GetItemCount(batteryItemName) == 0)
        {
            if (!string.IsNullOrEmpty(batteryQuestTitle)) GameManager.Instance.UpdateSecondaryMission(batteryQuestTitle, batteryQuestDetails);
        }
    }

    private void ToggleFlashlight()
    {
        if (playerLight == null) return;
        playerLight.enabled = !playerLight.enabled;
        if (audioSource != null && clickSound != null) audioSource.PlayOneShot(clickSound);

        if (playerLight.enabled && enableFlicker) flickerCoroutine = StartCoroutine(FlickerRoutine());
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
                if (audioSource != null && flickerSound != null) audioSource.PlayOneShot(flickerSound, 0.4f);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
                playerLight.intensity = defaultIntensity;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            }
        }
    }
}