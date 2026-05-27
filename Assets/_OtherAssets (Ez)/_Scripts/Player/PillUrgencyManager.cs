using UnityEngine;
using System.Collections;

public class PillUrgencyManager : MonoBehaviour
{
    [Header("activacion")]
    [Tooltip("titulo exacto de la mision que activa el timer")]
    [SerializeField] private string triggerMissionTitle = "Encuentra las pastillas";

    [Header("tiempo")]
    [Tooltip("segundos sin pastillas antes de los sintomas")]
    [SerializeField] private float timeBeforeSymptoms = 120f;

    [Header("castigo")]
    [SerializeField] private float sanityDrainPerSecond = 1.5f;

    [Header("audio")]
    [SerializeField] private AudioSource whisperAudioSource;
    [SerializeField, Range(0f, 1f)] private float maxWhisperVolume = 0.8f;
    [SerializeField] private float volumeIncreaseSpeed = 0.05f;

    [Header("visuales")]
    [Tooltip("contenedor de sombras en la ui")]
    [SerializeField] private GameObject uiShadowsContainer;

    [Header("narrativa")]
    [SerializeField] private string warningSubtitle = "<i>no me siento muy bien... deberia tomar mis pastillas...</i>";
    [SerializeField] private float subtitleDuration = 5f;

    private PlayerSanity playerSanity;
    private float timer = 0f;

    private bool isUrgencyActive = false;
    private bool symptomsActive = false;
    private bool pillsConsumed = false;

    // guardo la mision activa aca
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

            // guardo titulo y detalles
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
            // muestro texto
            if (!string.IsNullOrEmpty(warningSubtitle))
            {
                GameManager.Instance.ShowSubtitle(warningSubtitle, subtitleDuration);
            }

            // refresco la mision
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