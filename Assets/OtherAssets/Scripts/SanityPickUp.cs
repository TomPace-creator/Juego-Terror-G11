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
        // 1. Buscamos al jugador y le damos la orden de curarse gradualmente
        PlayerSanity playerSanity = FindObjectOfType<PlayerSanity>();
        if (playerSanity != null)
        {
            playerSanity.RestoreSanityGradual(sanityToRestore, restoreDuration);
        }

        // 2. Reproducimos sonido
        if (swallowSound != null)
        {
            AudioSource.PlayClipAtPoint(swallowSound, transform.position);
        }

        // 3. Mostramos el subtítulo si existe el GameManager
        if (GameManager.Instance != null && !string.IsNullOrEmpty(thoughtText))
        {
            GameManager.Instance.ShowSubtitle(thoughtText, 3.5f);
        }

        // 4. Destruimos el frasco de la mesa
        Destroy(gameObject);
    }
}