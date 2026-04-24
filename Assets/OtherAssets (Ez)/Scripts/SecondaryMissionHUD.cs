using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; // <-- NECESARIO PARA LAS LISTAS (Dictionary)

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(AudioSource))]
public class SecondaryMissionHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI secondaryMissionText;

    [Header("Efecto Visual y Tiempos")]
    [SerializeField] private float delayBeforeShow = 2.5f;
    [SerializeField] private AudioClip updateSound;

    private CanvasGroup canvasGroup;
    private AudioSource audioSource;

    // Nuestro "bloc de notas" interno para guardar las misiones activas. 
    // Clave (Key) = Título | Valor (Value) = Detalles
    private Dictionary<string, string> activeMissions = new Dictionary<string, string>();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSecondaryMissionChanged += ManageMissions;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSecondaryMissionChanged -= ManageMissions;
        }
    }

    // Esta función analiza qué queremos hacer antes de tocar la pantalla
    private void ManageMissions(string title, string details)
    {
        bool isNewMission = false;

        // 1. Si ambos están vacíos, borramos TODAS las misiones
        if (string.IsNullOrEmpty(title))
        {
            activeMissions.Clear();
        }
        // 2. Si solo los detalles están vacíos, COMPLETAMOS (borramos) esa misión específica
        else if (string.IsNullOrEmpty(details))
        {
            if (activeMissions.ContainsKey(title))
            {
                activeMissions.Remove(title);
            }
        }
        // 3. Si tiene título y detalle, la AGREGAMOS (o actualizamos si ya existía)
        else
        {
            if (!activeMissions.ContainsKey(title))
            {
                isNewMission = true; // Marcamos que es nueva para hacer la pausa y el sonido
            }
            activeMissions[title] = details;
        }

        // Frenamos cualquier animación en curso y actualizamos la pantalla
        StopAllCoroutines();
        StartCoroutine(UpdateVisuals(isNewMission));
    }

    private IEnumerator UpdateVisuals(bool isNewMission)
    {
        // Si ya no queda ninguna misión activa, nos desvanecemos suavemente
        if (activeMissions.Count == 0)
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime * 2f;
                yield return null;
            }
            secondaryMissionText.text = "";
            yield break;
        }

        // Si agregamos una misión nueva, hacemos la pausa cinematográfica
        if (isNewMission)
        {
            yield return new WaitForSeconds(delayBeforeShow);
        }

        // Reconstruimos el texto apilando todas las misiones guardadas
        string finalText = "";
        foreach (KeyValuePair<string, string> mission in activeMissions)
        {
            // Le agregamos dos saltos de línea al final (\n\n) para separar una misión de otra
            finalText += "<color=#FFD700><size=70%>" + mission.Key + "</size></color>\n<color=#A0A0A0><size=55%>" + mission.Value + "</size></color>\n\n";
        }

        secondaryMissionText.text = finalText;

        // Solo hacemos ruido si es una misión nueva (no queremos que suene al completarlas)
        if (isNewMission && updateSound != null)
        {
            audioSource.PlayOneShot(updateSound);
        }

        // Aparecemos suavemente
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * 2f;
            yield return null;
        }
    }
}