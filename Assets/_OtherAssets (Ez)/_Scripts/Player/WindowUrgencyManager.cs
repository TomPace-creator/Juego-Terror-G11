using UnityEngine;
using System.Collections;

public class WindowUrgencyManager : MonoBehaviour
{
    [Header("activacion por mision")]
    [Tooltip("titulo exacto de la mision para asomarse a la ventana")]
    [SerializeField] private string windowMissionTitle = "Investiga el ruido";

    [Header("tiempos")]
    [Tooltip("segundos antes del susto si ignora la ventana")]
    [SerializeField] private float timeBeforePanic = 60f;

    [Header("castigo")]
    [Tooltip("cordura que pierde por segundo en panico")]
    [SerializeField] private float sanityDrainPerSecond = 1f;

    [Header("efectos visuales")]
    [Tooltip("contenedor de sombras ui")]
    [SerializeField] private GameObject uiShadowsContainer;

    [Header("audio susurros")]
    [Tooltip("audiosource de susurros")]
    [SerializeField] private AudioSource whisperAudioSource;
    [SerializeField, Range(0f, 1f)] private float maxWhisperVolume = 0.8f;
    [SerializeField] private float volumeIncreaseSpeed = 0.05f;

    [Header("susto ventana")]
    [Tooltip("audio 3d del susto en la ventana")]
    [SerializeField] private AudioSource windowAudioSource;
    [Tooltip("sonido del vidrio o golpe")]
    [SerializeField] private AudioClip scaryWindowSound;
    [SerializeField] private string warningSubtitle = "<i>¡ese ruido me esta volviendo loca!... tengo que mirar que fue.</i>";
    [SerializeField] private float subtitleDuration = 4f;

    private PlayerSanity playerSanity;
    private float timer = 0f;

    private bool isTracking = false;
    private bool effectsActive = false;
    private bool windowChecked = false;

    // guardo la mision activa aca
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

        // arranco susurros en silencio
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

        // jump scare de ventana
        if (windowAudioSource != null && scaryWindowSound != null)
        {
            windowAudioSource.PlayOneShot(scaryWindowSound);
        }

        // arranco susurros
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

        // subo volumen de a poco
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