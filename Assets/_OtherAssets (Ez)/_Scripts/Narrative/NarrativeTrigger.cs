using UnityEngine;
using System.Collections;

public class NarrativeTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("La etiqueta del objeto que puede activar esto (casi siempre 'Player')")]
    [SerializeField] private string targetTag = "Player";

    
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        
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
            
            yield return new WaitForSeconds(4f);

            GameManager.Instance.ShowSubtitle("<i>Mmmm... no hay nada.</i>", 4f);

            
            yield return new WaitForSeconds(6f);

            
            GameManager.Instance.ShowSubtitle("<i>Otra vez imaginándome cosas raras...</i>", 4f);

            yield return new WaitForSeconds(6f);

            GameManager.Instance.ShowSubtitle("<i>Debería tomar mis pastillas...</i>", 4f);

    

        
            GameManager.Instance.UpdateMission("Toma tus pastillas", "Encuentra y toma las pastillas en la cocina");
        }

        Destroy(gameObject);
    }
}