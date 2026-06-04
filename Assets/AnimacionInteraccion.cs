using UnityEngine;

public class AnimacionInteraccion : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator miAnimator;

    // Eliminamos el InputAction de aquí para que no se dispare solo con la E.
    // Ahora esta función es pública y la llamaremos desde el script que detecta los objetos.
    public void EjecutarAnimacionInteraccion()
    {
        if (miAnimator != null)
        {
            // Esta línea imprimirá un mensaje en la consola de Unity
            Debug.Log("¡Animación de interacción disparada con éxito!");
            miAnimator.SetTrigger("Interactuar");
        }
    }
}