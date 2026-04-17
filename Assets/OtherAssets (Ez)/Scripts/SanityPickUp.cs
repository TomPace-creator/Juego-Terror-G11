using UnityEngine;

public class SanityPickUp : InteractableObject
{
    [Header("Efecto de las Pastillas")]
    [SerializeField] private float sanityToRestore = 30f;
    [SerializeField] private float restoreDuration = 5f;

    [Header("Feedback")]
    [SerializeField] private AudioClip swallowSound;
    [SerializeField] private string thoughtText = "<i>Uff... necesitaba esto. Me siento un poco mejor.</i>";

    private void Start()
    {
        interactText = "\"Tomar Pastillas [E]\"";
    }

    public override void Interact()
    {
        // 1. Usamos la sintaxis moderna de Unity para buscar al jugador sin que tire advertencias
        PlayerSanity playerSanity = FindFirstObjectByType<PlayerSanity>();

        if (playerSanity != null)
        {
            // Le pasamos la orden de curarse al jugador
            playerSanity.RestoreSanityGradual(sanityToRestore, restoreDuration);
        }

        // 2. Reproducimos sonido en la posición del frasco justo antes de destruirlo
        if (swallowSound != null)
        {
            AudioSource.PlayClipAtPoint(swallowSound, transform.position);
        }

        // 3. Mostramos el pensamiento de alivio
        if (GameManager.Instance != null && !string.IsNullOrEmpty(thoughtText))
        {
            GameManager.Instance.ShowSubtitle(thoughtText, 3.5f);
        }

        // 4. Destruimos este frasco genérico
        Destroy(gameObject);
    }
}