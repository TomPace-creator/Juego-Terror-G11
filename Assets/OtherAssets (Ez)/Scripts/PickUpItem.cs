using UnityEngine;
using System.Collections;

public class PickUpItem : InteractableObject
{
    [Header("Configuración del Objeto")]
    [SerializeField] private string itemName = "Linterna";

    [Header("Feedback Inicial")]
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private string thoughtOnPickUp = "<i>Con esto podré ver en la oscuridad...</i>";

    [Header("Secuencia de Pensamientos")]
    [SerializeField] private string secondThought = "<i>Debería buscar baterías de repuesto... por si acaso.</i>";

    [Header("Misiones Secundarias y Terciarias")]
    [Tooltip("Activa esto si recoger el objeto da una misión secundaria")]
    [SerializeField] private bool updateSecondaryMission = true;
    [SerializeField] private string secondaryMissionTitle = "[Opcional] BUSCA PILAS";
    [SerializeField] private string secondaryMissionDetails = "Encuentra pilas por la casa";

    [Tooltip("Activa esto si además da una misión terciaria")]
    [SerializeField] private bool updateTertiaryMission = false;
    [SerializeField] private string tertiaryMissionTitle = "> Tarea extra";
    [SerializeField] private string tertiaryMissionDetails = "Detalles opcionales";

    private void Start()
    {
        interactText = "\"Agarrar " + itemName + " [E]\""; // Escala automáticamente con el nombre
    }

    public override void Interact()
    {
        // 1. Apagamos los gráficos y colisiones
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        // 2. Reproducimos el sonido
        if (pickUpSound != null)
        {
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position);
        }

        // 3. Iniciamos la secuencia
        StartCoroutine(PickUpSequence());
    }

    private IEnumerator PickUpSequence()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddItemToInventory(itemName);

            if (!string.IsNullOrEmpty(thoughtOnPickUp))
            {
                GameManager.Instance.ShowSubtitle(thoughtOnPickUp, 4f);
                yield return new WaitForSeconds(4.5f);
            }

            if (!string.IsNullOrEmpty(secondThought))
            {
                GameManager.Instance.ShowSubtitle(secondThought, 4f);
                yield return new WaitForSeconds(4f);
            }

            // --- ACTUALIZACIÓN DE MISIONES ESCALABLE ---

            // Solo actualiza la secundaria si el diseñador (tú) lo marcó en el Inspector
            if (updateSecondaryMission)
            {
                GameManager.Instance.UpdateSecondaryMission(secondaryMissionTitle, secondaryMissionDetails);
            }

            // Solo actualiza la terciaria si está marcada
            if (updateTertiaryMission)
            {
                GameManager.Instance.UpdateTertiaryMission(tertiaryMissionTitle, tertiaryMissionDetails);
            }
        }

        Destroy(gameObject);
    }
}