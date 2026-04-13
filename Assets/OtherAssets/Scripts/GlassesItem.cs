using UnityEngine;
using UnityEngine.Rendering; 

public class GlassesItem : InteractableObject
{
    [Header("Configuración del Ítem")]
    [SerializeField] private string itemName = "Lentes";

    [Header("Efecto de Miopía")]
    [SerializeField] private Volume globalVolume; 

    private void Start()
    {
      
        interactText = "\"Agarrar Lentes [E]\"";
    }

    public override void Interact()
    {
    
        GameManager.Instance.AddItemToInventory(itemName);


        if (globalVolume != null)
        {
            globalVolume.enabled = false;
        }

       
        Destroy(gameObject);
    }
}