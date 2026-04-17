using UnityEngine;
using System.Collections;

public class SlideDoor : MonoBehaviour, IInteractable
{
    [Header("Configuración del Movimiento")]
    [SerializeField] private Vector3 openOffset = new Vector3(4, 0, 0f);
    [SerializeField] private float speed = 1f;

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

    
    public string GetInteractText()
    {
        
        if (isMoving) return "";

        // Si está abierto devuelve "Cerrar", si está cerrado devuelve "Abrir"
        return isOpen ? "Cerrar Puerta [E]" : "Abrir Puerta [E]";
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