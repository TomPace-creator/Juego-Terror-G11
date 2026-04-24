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
        PlayerSanity playerSanity = FindFirstObjectByType<PlayerSanity>();

        if (playerSanity != null)
        {
            playerSanity.RestoreSanityGradual(sanityToRestore, restoreDuration);
        }

      
        if (swallowSound != null)
        {
            AudioSource.PlayClipAtPoint(swallowSound, transform.position);
        }

      
        if (GameManager.Instance != null && !string.IsNullOrEmpty(thoughtText))
        {
            GameManager.Instance.ShowSubtitle(thoughtText, 3.5f);
        }

    
        Destroy(gameObject);
    }
}