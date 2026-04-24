using UnityEngine;

public class BatteryPickUp : InteractableObject
{
    [Header("Configuración")]
    [SerializeField] private string itemName = "Pilas";
    [SerializeField] private int amountToGive = 1;
    [SerializeField] private AudioClip pickUpSound;

    private void Start()
    {
        interactText = "\"Agarrar Pilas [E]\"";
    }

    public override void Interact()
    {
        if (GameManager.Instance != null)
        {
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