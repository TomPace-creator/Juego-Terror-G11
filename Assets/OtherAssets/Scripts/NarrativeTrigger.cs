using UnityEngine;
using System.Collections;

public class NarrativeTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("La etiqueta del objeto que puede activar esto (casi siempre 'Player')")]
    [SerializeField] private string targetTag = "Player";

    // Para asegurarnos de que no se dispare dos veces si el jugador entra y sale rápido
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Si el que entró en la caja invisible tiene la etiqueta correcta y no se ha activado antes...
        if (!hasTriggered && other.CompareTag(targetTag))
        {
            hasTriggered = true;
            StartCoroutine(PlayWindowSequence());
        }
    }

    private IEnumerator PlayWindowSequence()
    {
        if (GameManager.Instance != null)
        {
            // 1. Primer pensamiento al asomarse
            yield return new WaitForSeconds(5f);

            GameManager.Instance.ShowSubtitle("<i>Mmmm... no hay nada.</i>", 5f);

            // Esperamos 5 segundos (4 del texto + 1 de silencio)
            yield return new WaitForSeconds(10f);

            // 2. Segundo pensamiento relajándose
            GameManager.Instance.ShowSubtitle("<i>Otra vez imaginándome cosas raras...</i>", 5f);

            yield return new WaitForSeconds(10f);

            GameManager.Instance.ShowSubtitle("<i>Uff que sed...</i>", 5f);

            yield return new WaitForSeconds(10f);

            // 3. Actualizamos la misión para hacerla volver a la cama o ir a otro lado
            GameManager.Instance.UpdateMission("> Ve a la cocina por agua", "");
        }

        // Destruimos la caja invisible para que este evento no vuelva a ocurrir nunca más
        Destroy(gameObject);
    }
}