using UnityEngine;

public class BatteryPickUp : InteractableObject
{
    [Header("Configuración")]
    [SerializeField] private string itemName = "Pilas";
    [SerializeField] private int amountToGive = 2; // ¡Agrega 2 al inventario!
    [SerializeField] private AudioClip pickUpSound;

    private void Start()
    {
        interactText = "\"Agarrar Pilas [E]\"";
    }

    public override void Interact()
    {
        if (GameManager.Instance != null)
        {
            // Hacemos un bucle para agregar la cantidad exacta
            for (int i = 0; i < amountToGive; i++)
            {
                GameManager.Instance.AddItemToInventory(itemName);
            }

            GameManager.Instance.ShowSubtitle("<i>Genial, pilas de repuesto.</i>", 3f);
        }

        if (pickUpSound != null)
        {
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position);
        }

        Destroy(gameObject);
    }
}