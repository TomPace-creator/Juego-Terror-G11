using UnityEngine;
using System.Collections;

public class KeyPickUp : InteractableObject
{
    [Header("Configuración de la Llave")]
    [Tooltip("Debe ser exactamente el mismo nombre que pusiste en 'Required Key' de la Puerta")]
    [SerializeField] private string itemName = "Llave_Atico";

    [Header("Feedback Visual y Sonoro")]
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private string thoughtText = "<i>Una llave vieja... ¿Será la que abre el ático?</i>";

    [Header("Actualización de Misión (Fase 2)")]
    [Tooltip("Título de la misión que dio la puerta, para poder borrarla")]
    [SerializeField] private string questToClear = "[Opcional] La llave del Ático";

    [Tooltip("La nueva misión que aparece tras agarrar la llave")]
    [SerializeField] private string newQuestTitle = "[Opcional] EL ATICO";
    [SerializeField] private string newQuestDetails = "Abre la puerta del ático usando la llave vieja.";

    private void Start()
    {
        interactText = "\"Agarrar Llave [E]\"";
    }

    public override void Interact()
    {
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

            if (!string.IsNullOrEmpty(thoughtText))
            {
                GameManager.Instance.ShowSubtitle(thoughtText, 4f);
                yield return new WaitForSeconds(4.5f);
            }

            // 1. Borramos la misión vieja mandando detalles vacíos ""
            if (!string.IsNullOrEmpty(questToClear))
            {
                GameManager.Instance.UpdateSecondaryMission(questToClear, "");
            }

            // 2. Agregamos la misión nueva
            if (!string.IsNullOrEmpty(newQuestTitle))
            {
                GameManager.Instance.UpdateSecondaryMission(newQuestTitle, newQuestDetails);
            }
        }

        Destroy(gameObject);
    }
}