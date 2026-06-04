using UnityEngine;
using System.Collections;

public class NarrativeTrigger : MonoBehaviour
{
    [Header("Configuración Base")]
    [Tooltip("La etiqueta del objeto que puede activar esto (casi siempre 'Player')")]
    [SerializeField] private string targetTag = "Player";

    [Header("Condición de Activación (Opcional)")]
    [Tooltip("El trigger SOLO funcionará si el jugador tiene este ítem en el inventario. Déjalo en blanco si quieres que se active siempre.")]
    [SerializeField] private string requiredItem = "Lentes"; // <-- Pon el nombre exacto de tu ítem aquí

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Si ya se activó antes o no es el jugador, salimos
        if (hasTriggered || !other.CompareTag(targetTag)) return;

        // 2. Comprobamos la condición del ítem
        if (GameManager.Instance != null && !string.IsNullOrEmpty(requiredItem))
        {
            if (!GameManager.Instance.HasItem(requiredItem))
            {
                // El jugador pasó por la zona pero aún no tiene los lentes. 
                // Ignoramos el choque y dejamos el trigger intacto para cuando vuelva.
                return;
            }
        }

        // 3. Si tiene los lentes (o si no pedimos ningún ítem), iniciamos la cinemática
        hasTriggered = true;
        StartCoroutine(PlayWindowSequence());
    }

    private IEnumerator PlayWindowSequence()
    {
        if (GameManager.Instance != null)
        {
            // Pequeño ajuste: le sumé 0.5s a los WaitForSeconds para que los subtítulos 
            // no se pisen entre sí justo cuando el anterior desaparece (4f de duración + 0.5f de pausa)

            GameManager.Instance.ShowSubtitle("<i>Mmmm... no hay nada.</i>", 4f);
            yield return new WaitForSeconds(4.5f);

            GameManager.Instance.ShowSubtitle("<i>Otra vez imaginándome cosas raras...</i>", 4f);
            yield return new WaitForSeconds(4.5f);

            GameManager.Instance.ShowSubtitle("<i>Debería tomar mis pastillas...</i>", 4f);
            yield return new WaitForSeconds(4.5f);

            GameManager.Instance.UpdateMission("Toma tus pastillas", "Encuentra y toma las pastillas en la cocina");
        }

        Destroy(gameObject);
    }
}