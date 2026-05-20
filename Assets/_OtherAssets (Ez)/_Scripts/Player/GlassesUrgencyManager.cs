using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class GlassesUrgencyManager : MonoBehaviour
{
    [Header("Activación por Misión")]
    [Tooltip("Título EXACTO de la misión inicial de los lentes")]
    [SerializeField] private string glassesMissionTitle = "Ponte lentes";

    [Header("Configuración de Tiempos")]
    [Tooltip("Segundos de gracia antes de que la vista comience a empeorar")]
    [SerializeField] private float timeBeforeVisionFails = 45f;
    [Tooltip("Segundos que tarda la vista en llegar a su punto máximo de borrosidad")]
    [SerializeField] private float visionDegradationDuration = 60f;

    [Header("Castigo (Sanidad)")]
    [Tooltip("Cordura perdida por segundo cuando la visión está fallando")]
    [SerializeField] private float sanityDrainPerSecond = 1f;

    [Header("Efecto Visual (Borroso / URP)")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField, Range(0f, 1f)] private float maxVolumeWeight = 1f;

    [Header("Eventos de Sombras")]
    [SerializeField] private GameObject shadowsContainer;

    [Header("Audio (Susurros)")]
    [Tooltip("El AudioSource para los susurros en esta fase")]
    [SerializeField] private AudioSource whisperAudioSource;
    [Tooltip("Volumen máximo al que llegarán los susurros")]
    [SerializeField, Range(0f, 1f)] private float maxWhisperVolume = 0.8f;
    [Tooltip("Velocidad a la que sube el volumen")]
    [SerializeField] private float volumeIncreaseSpeed = 0.05f;

    private PlayerSanity playerSanity;
    private float timer = 0f;

    private bool isTracking = false;
    private bool effectsActive = false;
    private bool glassesEquipped = false;

    // Variables para recordar la misión y volver a mostrarla
    private string activeMissionTitle = "";
    private string activeMissionDetails = "";

    private void Start()
    {
        playerSanity = GetComponent<PlayerSanity>();
        if (playerSanity == null) playerSanity = GetComponentInChildren<PlayerSanity>();

        if (postProcessVolume != null) postProcessVolume.weight = 0f;
        if (shadowsContainer != null) shadowsContainer.SetActive(false);

        // Preparamos el audio de los susurros
        if (whisperAudioSource != null)
        {
            whisperAudioSource.volume = 0f;
            whisperAudioSource.loop = true;
            whisperAudioSource.Stop();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionChanged += EvaluateMissionStage;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionChanged -= EvaluateMissionStage;
        }
    }

    private void EvaluateMissionStage(string title, string details)
    {
        if (glassesEquipped) return;

        if (!string.IsNullOrEmpty(title) && title.Contains(glassesMissionTitle))
        {
            isTracking = true;
            // Guardamos los datos de la misión para usarlos como recordatorio después
            activeMissionTitle = title;
            activeMissionDetails = details;
        }
        else
        {
            if (isTracking)
            {
                StopUrgencyPermanently();
            }
        }
    }

    private void Update()
    {
        if (!isTracking || glassesEquipped) return;

        timer += Time.deltaTime;

        if (timer >= timeBeforeVisionFails)
        {
            if (!effectsActive)
            {
                TriggerEffects();
            }

            ApplyProgressiveEffects();
        }
    }

    private void TriggerEffects()
    {
        effectsActive = true;

        if (GameManager.Instance != null)
        {
            // 1. Mostrar el subtítulo
            GameManager.Instance.ShowSubtitle("<i>Necesito mis lentes...</i>", 4f);

            // 2. Volver a disparar la misión principal para que aparezca en el HUD
            if (!string.IsNullOrEmpty(activeMissionTitle))
            {
                GameManager.Instance.UpdateMission(activeMissionTitle, activeMissionDetails);
            }
        }

        if (shadowsContainer != null)
        {
            shadowsContainer.SetActive(true);
        }

        // Encendemos los susurros (empezarán en volumen 0 y subirán progresivamente)
        if (whisperAudioSource != null && !whisperAudioSource.isPlaying)
        {
            whisperAudioSource.Play();
        }
    }

    private void ApplyProgressiveEffects()
    {
        float timeElapsedSinceSymptoms = timer - timeBeforeVisionFails;
        float progressPercent = Mathf.Clamp01(timeElapsedSinceSymptoms / visionDegradationDuration);

        if (postProcessVolume != null)
        {
            postProcessVolume.weight = progressPercent * maxVolumeWeight;
        }

        if (playerSanity != null)
        {
            playerSanity.LoseSanity(sanityDrainPerSecond * Time.deltaTime);
        }

        // Subimos el volumen de los susurros progresivamente
        if (whisperAudioSource != null && whisperAudioSource.volume < maxWhisperVolume)
        {
            whisperAudioSource.volume += volumeIncreaseSpeed * Time.deltaTime;
        }
    }

    private void StopUrgencyPermanently()
    {
        glassesEquipped = true;
        isTracking = false;
        effectsActive = false;

        StartCoroutine(RestoreVisionRoutine());
        StartCoroutine(FadeOutWhispers());

        if (shadowsContainer != null) shadowsContainer.SetActive(false);
    }

    private IEnumerator RestoreVisionRoutine()
    {
        if (postProcessVolume == null) yield break;

        while (postProcessVolume.weight > 0f)
        {
            postProcessVolume.weight -= Time.deltaTime * 2.5f;
            yield return null;
        }

        postProcessVolume.weight = 0f;
    }

    private IEnumerator FadeOutWhispers()
    {
        if (whisperAudioSource == null) yield break;

        while (whisperAudioSource.volume > 0f)
        {
            whisperAudioSource.volume -= Time.deltaTime * 1.5f;
            yield return null;
        }

        whisperAudioSource.Stop();
    }
}