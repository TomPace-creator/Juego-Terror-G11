using UnityEngine;
using TMPro;
using System.Collections; // Necesario para las Corrutinas

public class MissionHUD : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI missionText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionChanged += RefreshText;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMissionChanged -= RefreshText;
        }
    }

    // Ahora recibimos los dos textos
    private void RefreshText(string title, string details)
    {
        // Frenamos cualquier animación anterior por las dudas
        StopAllCoroutines();
        // Iniciamos el efecto de despliegue
        StartCoroutine(UnfoldText(title, details));
    }

    private IEnumerator UnfoldText(string title, string details)
    {
        // 1. Mostramos solo el título impactante. 
        // Le ponemos <size=130%> para que sea un 30% más grande que el tamaño base.
        missionText.text = "<size=80%>" + title + "</size>";

        // 2. Esperamos 2 segundos de suspenso
        yield return new WaitForSeconds(10f);

        // 3. Desplegamos el detalle abajo. 
        // Mantenemos el título grande y hacemos el detalle más chico (60%) y gris.
        missionText.text = "<size=80%>" + title + "</size>\n<color=#A0A0A0><size=60%>" + details + "</size></color>";
    }
}