using UnityEngine;
using System.Collections; // <-- Necesario para el IEnumerator (las pausas de tiempo)

public class PickUpItem : InteractableObject
{
    [Header("Configuración del Objeto")]
    [SerializeField] private string itemName = "Linterna";

    [Header("Feedback Inicial")]
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private string thoughtOnPickUp = "<i>Con esto podré ver en la oscuridad...</i>";

    [Header("Secuencia de Baterías")]
    [SerializeField] private string secondThought = "<i>Debería buscar baterías de repuesto... por si acaso.</i>";
    [SerializeField] private string newMissionTitle = "> Busca Baterías";
    [SerializeField] private string newMissionDetails = "Encuentra baterías en la casa";

    private void Start()
    {
        interactText = "\"Agarrar Linterna [E]\"";
    }

    public override void Interact()
    {
        // 1. Apagamos los gráficos y colisiones de la linterna para que "desaparezca" de la mesa al instante
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        // 2. Reproducimos el sonido de agarrar
        if (pickUpSound != null)
        {
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position);
        }

        // 3. Iniciamos la película mental de Ruth
        StartCoroutine(PickUpSequence());
    }

    private IEnumerator PickUpSequence()
    {
        if (GameManager.Instance != null)
        {
            // Agregamos al inventario y lanzamos el primer pensamiento
            GameManager.Instance.AddItemToInventory(itemName);

            if (!string.IsNullOrEmpty(thoughtOnPickUp))
            {
                GameManager.Instance.ShowSubtitle(thoughtOnPickUp, 4f);
            }

            // Esperamos 4.5 segundos (4 que dura el texto + medio segundo para respirar)
            yield return new WaitForSeconds(4.5f);

            // Lanzamos el segundo pensamiento
            GameManager.Instance.ShowSubtitle(secondThought, 4f);

            // Esperamos otros 4 segundos a que termine de leer eso
            yield return new WaitForSeconds(4f);

            // ¡Actualizamos el HUD con la nueva misión!
            GameManager.Instance.UpdateMission(newMissionTitle, newMissionDetails);
        }

        // Ahora sí, destruimos el objeto vacío para limpiar la memoria
        Destroy(gameObject);
    }
}