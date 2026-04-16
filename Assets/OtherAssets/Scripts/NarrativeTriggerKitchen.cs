using UnityEngine;
using System.Collections;

public class NarrativeTriggerKitchen : MonoBehaviour
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
            // 
            yield return new WaitForSeconds(5f);

            GameManager.Instance.ShowSubtitle("<i>gluglgulgug</i>", 5f);

            
            yield return new WaitForSeconds(10f);

         
            GameManager.Instance.ShowSubtitle("<i>el diablo</i>", 5f);

            yield return new WaitForSeconds(10f);

            GameManager.Instance.ShowSubtitle("<i>asdasdasd.</i>", 5f);

            yield return new WaitForSeconds(10f);

          
            GameManager.Instance.UpdateMission("> chucha la wea", "");
        }

        // Destruimos la caja invisible para que este evento no vuelva a ocurrir nunca más
        Destroy(gameObject);
    }
}