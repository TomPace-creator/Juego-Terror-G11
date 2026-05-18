using UnityEngine;
using System.Collections;

public class PillUrgencyManager : MonoBehaviour
{
    [Header("Activación")]
    [Tooltip("El título EXACTO de la misión que activa este temporizador (Ej: 'Encuentra las pastillas')")]
    [SerializeField] private string triggerMissionTitle = "Encuentra las pastillas";

    [Header("Configuración de Tiempo")]
    [Tooltip("Segundos que el jugador puede estar sin tomar las pastillas antes de que empiecen los síntomas")]
    [SerializeField] private float timeBeforeSymptoms = 120f;

    [Header("Castigo (Sanidad)")]
    [SerializeField] private float sanityDrainPerSecond = 1.5f;

    [Header("Audio (Susurros)")]
    [SerializeField] private AudioSource whisperAudioSource;
    [SerializeField, Range(0f, 1f)] private float maxWhisperVolume = 0.8f;
    [SerializeField] private float volumeIncreaseSpeed = 0.05f;

    [Header("Efectos Visuales (Alucinaciones)")]
    [Tooltip("El contenedor o la imagen de la sombra en el Canvas de la UI")]
    [SerializeField] private GameObject uiShadowsContainer;

    [Header("Narrativa")]
    [SerializeField] private string warningSubtitle = "<i>No me siento muy bien... realmente debería tomar mis pastillas...</i>";
    [SerializeField] private float subtitleDuration = 5f;

    private PlayerSanity playerSanity;
    private float timer = 0f;

    private bool isUrgencyActive = false;
    private bool symptomsActive = false;
    private bool pillsConsumed = false;

    // Variables internas para recordar la misión y refrescar el HUD
    private string activeMissionTitle = "";
    private string activeMissionDetails = "";

    private void Start()
    {
        playerSanity = GetComponent<PlayerSanity>();
        if (playerSanity == null) playerSanity = GetComponentInChildren<PlayerSanity>();

        if (whisperAudioSource != null)
        {
            whisperAudioSource.volume = 0f;
            whisperAudioSource.loop = true;
            whisperAudioSource.Stop();
        }

        if (uiShadowsContainer != null)
        {
            uiShadowsContainer.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPillsConsumed += HandlePillsConsumed;
            GameManager.Instance.OnMissionChanged += CheckMissionStart;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPillsConsumed -= HandlePillsConsumed;
            GameManager.Instance.OnMissionChanged -= CheckMissionStart;
        }
    }

    private void CheckMissionStart(string title, string details)
    {
        if (!string.IsNullOrEmpty(title) && title.Contains(triggerMissionTitle))
        {
            isUrgencyActive = true;

            // Guardamos el título y los detalles exactos de la misión de las pastillas
            activeMissionTitle = title;
            activeMissionDetails = details;
        }
    }

    private void Update()
    {
        if (!isUrgencyActive || pillsConsumed) return;

        if (!symptomsActive)
        {
            timer += Time.deltaTime;
            if (timer >= timeBeforeSymptoms)
            {
                TriggerSymptoms();
            }
        }
        else
        {
            ApplySymptoms();
        }
    }

    private void TriggerSymptoms()
    {
        symptomsActive = true;

        if (GameManager.Instance != null)
        {
            // 1. Mostramos el subtítulo de Ruth
            if (!string.IsNullOrEmpty(warningSubtitle))
            {
                GameManager.Instance.ShowSubtitle(warningSubtitle, subtitleDuration);
            }

            // 2. Refrescamos la misión en pantalla para recordar al jugador su objetivo
            if (!string.IsNullOrEmpty(activeMissionTitle))
            {
                GameManager.Instance.UpdateMission(activeMissionTitle, activeMissionDetails);
            }
        }

        if (whisperAudioSource != null && !whisperAudioSource.isPlaying)
        {
            whisperAudioSource.Play();
        }

        if (uiShadowsContainer != null)
        {
            uiShadowsContainer.SetActive(true);
        }
    }

    private void ApplySymptoms()
    {
        if (playerSanity != null)
        {
            playerSanity.LoseSanity(sanityDrainPerSecond * Time.deltaTime);
        }

        if (whisperAudioSource != null && whisperAudioSource.volume < maxWhisperVolume)
        {
            whisperAudioSource.volume += volumeIncreaseSpeed * Time.deltaTime;
        }
    }

    private void HandlePillsConsumed()
    {
        pillsConsumed = true;
        symptomsActive = false;

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