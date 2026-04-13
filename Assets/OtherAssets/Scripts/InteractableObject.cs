using UnityEngine;

// "abstract" significa que este script es solo una plantilla. 
// No puedes pegarle "InteractableObject" directamente a un objeto en Unity, tienes que pegarle a un Hijo.
public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Configuración de Interacción Base")]
    // "protected" significa que las variables son privadas, pero los Hijos sí pueden verlas y usarlas.
    [SerializeField] protected string interactText = "Press E to Interact";

    // 1. Cumplimos la primera regla de la interfaz (El texto para el HUD)
    // Como es igual para todos los objetos, lo programamos aquí una sola vez.
    public string GetInteractText()
    {
        return interactText;
    }

    // 2. Cumplimos la segunda regla de la interfaz (La acción)
    // Le ponemos "abstract" porque el Padre no sabe qué hacer (¿prender luz? ¿abrir puerta?). 
    // Esto obliga a cada Hijo a programar su propia acción.
    public abstract void Interact();
}