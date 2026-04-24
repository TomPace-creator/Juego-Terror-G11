using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; // list

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

   
    private void ManageMissions(string title, string details)
    {
        bool isNewMission = false;

       
        if (string.IsNullOrEmpty(title))
        {
            activeMissions.Clear();
        }
        
        else if (string.IsNullOrEmpty(details))
        {
            if (activeMissions.ContainsKey(title))
            {
                activeMissions.Remove(title);
            }
        }
       
        else
        {
            if (!activeMissions.ContainsKey(title))
            {
                isNewMission = true;
            }
            activeMissions[title] = details;
        }

      
        StopAllCoroutines();
        StartCoroutine(UpdateVisuals(isNewMission));
    }

    private IEnumerator UpdateVisuals(bool isNewMission)
    {
      
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

      
        if (isNewMission)
        {
            yield return new WaitForSeconds(delayBeforeShow);
        }

        
        string finalText = "";
        foreach (KeyValuePair<string, string> mission in activeMissions)
        {
            
            finalText += "<color=#FFD700><size=70%>" + mission.Key + "</size></color>\n<color=#A0A0A0><size=55%>" + mission.Value + "</size></color>\n\n";
        }

        secondaryMissionText.text = finalText;

        
        if (isNewMission && updateSound != null)
        {
            audioSource.PlayOneShot(updateSound);
        }

       
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * 2f;
            yield return null;
        }
    }
}