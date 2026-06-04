using UnityEngine;

public class AjustarHueso : MonoBehaviour
{
    [Header("Arrastra aquí el hueso del brazo izquierdo")]
    [SerializeField] private Transform hueso;

    [Header("Ajuste de Rotación (Cámbialo en Play Mode)")]
    [SerializeField] private Vector3 rotacionExtra;

    private void LateUpdate()
    {
        if (hueso != null)
        {
            // Aplica la rotación extra después de la animación
            hueso.localRotation *= Quaternion.Euler(rotacionExtra);
        }
    }
}