using UnityEngine;
using UnityEngine.Rendering;

public class GlassesItem : InteractableObject
{
    [Header("Configuración del Ítem")]
    [SerializeField] private string itemName = "Lentes";

    [Header("Efecto de Miopía")]
    [Tooltip("El Volumen de Post-Processing que hace que se vea borroso")]
    [SerializeField] private Volume globalVolume;

    [Header("Narrativa y Misión")]
    [SerializeField] private string thoughtOnPickUp = "<i>Mucho mejor...</i>";
    [SerializeField] private string newMissionTitle = "INVESTIGAR EL SONIDO";
    [SerializeField] private string newMissionDetails = "Asómate por la ventana";

    private void Start()
    {
        interactText = "\"Agarrar Lentes [E]\"";
    }

    public override void Interact()
    {
        if (GameManager.Instance != null)
        {
            
            GameManager.Instance.AddItemToInventory(itemName);

          
            GameManager.Instance.ShowSubtitle(thoughtOnPickUp, 4f);

            
            GameManager.Instance.UpdateMission(newMissionTitle, newMissionDetails);
        }

        // apaga miopia
        if (globalVolume != null)
        {
            globalVolume.enabled = false;
        }

        // rompe lente
        Destroy(gameObject);
    }
}