using UnityEngine;
using System.Collections;

public class WindowUrgencyManager : MonoBehaviour
{
    [Header("Activación por Misión")]
    [Tooltip("Título EXACTO de la misión donde debe asomarse a la ventana")]
    [SerializeField] private string windowMissionTitle = "Investiga el ruido";

    [Header("Configuración de Tiempos")]
    [Tooltip("Segundos que el jugador puede ignorar la ventana antes del susto")]
    [SerializeField] private float timeBeforePanic = 60f;

    [Header("Castigo (Sanidad)")]
    [Tooltip("Cordura perdida por segundo cuando entra en pánico")]
    [SerializeField] private float sanityDrainPerSecond = 1f;

    [Header("Efectos Visuales")]
    [Tooltip("El contenedor de sombras en la UI")]
    [SerializeField] private GameObject uiShadowsContainer;

    [Header("Audio (Susurros Constantes)")]
    [Tooltip("El AudioSource para los susurros en la cabeza de Ruth")]
    [SerializeField] private AudioSource whisperAudioSource;
    [SerializeField, Range(0f, 1f)] private float maxWhisperVolume = 0.8f;
    [SerializeField] private float volumeIncreaseSpeed = 0.05f;

    [Header("Audio y Narrativa (Jump Scare)")]
    [Tooltip("AudioSource para el susto (idealmente en la ventana real, en 3D)")]
    [SerializeField] private AudioSource windowAudioSource;
    [Tooltip("Sonido de golpe en el vidrio o arañazo")]
    [SerializeField] private AudioClip scaryWindowSound;
    [SerializeField] private string warningSubtitle = "<i>¡Ese ruido me está volviendo loca!... Tengo que mirar qué fue.</i>";
    [SerializeField] private float subtitleDuration = 4f;

    private PlayerSanity playerSanity;
    private float timer = 0f;

    private bool isTracking = false;
    private bool effectsActive = false;
    private bool windowChecked = false;

    // Variables internas para recordar la misión y refrescar el HUD
    private string activeMissionTitle = "";
    private string activeMissionDetails = "";

    private void Start()
    {
        playerSanity = GetComponent<PlayerSanity>();
        if (playerSanity == null) playerSanity = GetComponentInChildren<PlayerSanity>();

        if (uiShadowsContainer != null)
        {
            uiShadowsContainer.SetActive(false);
        }

        // Preparamos el audio de los susurros para que empiece silenciado
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
        if (windowChecked) return;

        if (!string.IsNullOrEmpty(title) && title.Contains(windowMissionTitle))
        {
            isTracking = true;
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
        if (!isTracking || windowChecked) return;

        timer += Time.deltaTime;

        if (timer >= timeBeforePanic)
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
            if (!string.IsNullOrEmpty(warningSubtitle))
            {
                GameManager.Instance.ShowSubtitle(warningSubtitle, subtitleDuration);
            }

            if (!string.IsNullOrEmpty(activeMissionTitle))
            {
                GameManager.Instance.UpdateMission(activeMissionTitle, activeMissionDetails);
            }
        }

        // Reproducimos el "Jump Scare" auditivo de la ventana
        if (windowAudioSource != null && scaryWindowSound != null)
        {
            windowAudioSource.PlayOneShot(scaryWindowSound);
        }

        // Encendemos los susurros progresivos
        if (whisperAudioSource != null && !whisperAudioSource.isPlaying)
        {
            whisperAudioSource.Play();
        }

        if (uiShadowsContainer != null)
        {
            uiShadowsContainer.SetActive(true);
        }
    }

    private void ApplyProgressiveEffects()
    {
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
        windowChecked = true;
        isTracking = false;
        effectsActive = false;

        StartCoroutine(FadeOutWhispers());

        if (uiShadowsContainer != null)
        {
            uiShadowsContainer.SetActive(false);
        }
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