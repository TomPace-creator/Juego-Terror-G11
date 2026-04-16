using UnityEngine;

public class PickUpItem : InteractableObject
{
    [Header("Configuración del Objeto")]
  
    [SerializeField] private string itemName = "Linterna";

    [Header("Feedback (Opcional)")]
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private string thoughtOnPickUp = "<i>Con esto podré ver en la oscuridad...</i>";

    public override void Interact()
    {
        if (GameManager.Instance != null)
        {
   
            GameManager.Instance.AddItemToInventory(itemName);

         
            if (!string.IsNullOrEmpty(thoughtOnPickUp))
            {
                GameManager.Instance.ShowSubtitle(thoughtOnPickUp, 3f);
            }
        }

        
        if (pickUpSound != null)
        {
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position);
        }

        Destroy(gameObject);
    }
}