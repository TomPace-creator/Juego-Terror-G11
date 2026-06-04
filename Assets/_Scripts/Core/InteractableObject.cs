using UnityEngine;


public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Configuración de Interacción Base")]
   
    [SerializeField] protected string interactText = "Agarra Linterna [E]";

  
    public string GetInteractText()
    {
        return interactText;
    }

    public abstract void Interact();
}