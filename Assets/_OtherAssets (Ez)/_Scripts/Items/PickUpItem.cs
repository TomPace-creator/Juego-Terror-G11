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
        interactText = "\"Agarrar " + itemName + " [E]\""; 
    }

    public override void Interact()
    {
        // apag collis
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        
        if (pickUpSound != null)
        {
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position);
        }

    
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

            

            
            if (updateSecondaryMission)
            {
                GameManager.Instance.UpdateSecondaryMission(secondaryMissionTitle, secondaryMissionDetails);
            }

            
            if (updateTertiaryMission)
            {
                GameManager.Instance.UpdateTertiaryMission(tertiaryMissionTitle, tertiaryMissionDetails);
            }
        }

        Destroy(gameObject);
    }
}