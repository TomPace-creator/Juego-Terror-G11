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
    [SerializeField] private string thoughtOnPickUp = "<i>Mucho mejor... ahora a ver qué fue ese ruido.</i>";
    [SerializeField] private string newMissionTitle = "> Investiga la fuente del ruido";
    [SerializeField] private string newMissionDetails = "Asómate por la ventana";

    private void Start()
    {
        interactText = "\"Agarrar Lentes [E]\"";
    }

    public override void Interact()
    {
        if (GameManager.Instance != null)
        {
            // 1. Guardamos en el inventario
            GameManager.Instance.AddItemToInventory(itemName);

            // 2. Mostramos el pensamiento
            GameManager.Instance.ShowSubtitle(thoughtOnPickUp, 4f);

            
            GameManager.Instance.UpdateMission(newMissionTitle, newMissionDetails);
        }

        // 4. ¡Tu magia visual! Apagamos la miopía
        if (globalVolume != null)
        {
            globalVolume.enabled = false;
        }

        // 5. Destruimos el objeto de la mesita
        Destroy(gameObject);
    }
}