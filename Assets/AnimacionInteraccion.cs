using UnityEngine;
using UnityEngine.InputSystem;

public class AnimacionInteraccion : MonoBehaviour
{
    public Animator miAnimator;

    public void DispararAnimacion(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Esta línea imprimirá un mensaje en la consola de Unity
            Debug.Log("¡El script recibió la tecla E!");
            miAnimator.SetTrigger("Interactuar");
        }
    }
}