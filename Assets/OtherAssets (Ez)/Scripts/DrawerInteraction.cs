using UnityEngine;
using System.Collections;

public class DrawerInteraction : MonoBehaviour, IInteractable
{
    [Header("Configuración del Movimiento")]
    [SerializeField] private Vector3 openOffset = new Vector3(0, 0, 0.5f);
    [SerializeField] private float speed = 3f;

    [Header("Sonidos")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private bool isMoving = false;

    private void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + openOffset;
    }

    public void Interact()
    {
        if (isMoving) return;

        isOpen = !isOpen;

        AudioClip soundToPlay = isOpen ? openSound : closeSound;
        if (soundToPlay != null)
        {
            AudioSource.PlayClipAtPoint(soundToPlay, transform.position);
        }

        StartCoroutine(SlideDrawer());
    }

    // --- ¡AQUÍ ESTÁ LA FUNCIÓN QUE FALTABA PARA CUMPLIR EL CONTRATO! ---
    public string GetInteractText()
    {
        // Si se está moviendo, no mostramos texto para no confundir
        if (isMoving) return "";

        // Si está abierto devuelve "Cerrar", si está cerrado devuelve "Abrir"
        return isOpen ? "Cerrar Cajón [E]" : "Abrir Cajón [E]";
    }

    private IEnumerator SlideDrawer()
    {
        isMoving = true;

        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = isOpen ? openPosition : closedPosition;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        isMoving = false;
    }
}