using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IntroCinematic : MonoBehaviour
{
    [Header("Referencias de la Escena")]
    [SerializeField] private Controller playerController;
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private Transform clockTarget;
    [SerializeField] private Image blackScreen;
    [SerializeField] private Transform sleepPosition;
    [SerializeField] private LightSwitch bedsideLamp;
    [SerializeField] private AudioSource kitchenNoiseSource; // El parlante de la cocina
    [SerializeField] private AudioClip spookySound;

    [Header("Tiempos de la Cinemática")]
    [SerializeField] private float sleepTime = 2f;
    [SerializeField] private float wakeUpSpeed = 1f;
    [SerializeField] private float ceilingStareTime = 2f;
    [SerializeField] private float lookAtClockSpeed = 2f;
    [SerializeField] private float clockStareTime = 3f;
    [SerializeField] private float standUpSpeed = 1.5f;

    private Vector3 defaultStandPos;
    private Quaternion defaultStandRot;

    private void Start()
    {
        playerController.DisableMovement();
        playerController.DisableLook();

        defaultStandPos = cameraContainer.localPosition;
        defaultStandRot = cameraContainer.localRotation;

        cameraContainer.position = sleepPosition.position;
        cameraContainer.rotation = sleepPosition.rotation;

        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // --- ESCENA 1 y 2: DORMIDA Y ABRIENDO LOS OJOS ---
        yield return new WaitForSeconds(sleepTime);
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * wakeUpSpeed;
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // --- ESCENA 2.5: MIRANDO EL TECHO ---
        yield return new WaitForSeconds(ceilingStareTime);

        // --- ESCENA 3: GIRANDO HACIA EL RELOJ EN LA OSCURIDAD ---
        Quaternion startRotation = cameraContainer.rotation;
        Vector3 directionToClock = clockTarget.position - cameraContainer.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToClock);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * lookAtClockSpeed;
            cameraContainer.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        // --- ESCENA 4: EL PERSONAJE PRENDE LA LUZ ---
        yield return new WaitForSeconds(0.5f);
        if (bedsideLamp != null && !bedsideLamp.GetIsOn())
        {
            bedsideLamp.Interact();
        }

        // --- ESCENA 4.5: EL SUSPENSO Y EL RUIDO EN 3D ---
        yield return new WaitForSeconds(1f);

        // Usamos el parlante de la cocina para "disparar" el sonido del golpe
        if (kitchenNoiseSource != null && spookySound != null)
        {
            kitchenNoiseSource.PlayOneShot(spookySound);
        }

        yield return new WaitForSeconds(clockStareTime);
        // --- ESCENA 5: LEVANTÁNDOSE DE LA CAMA ---
        Vector3 currentBedPos = cameraContainer.localPosition;
        Quaternion currentBedRot = cameraContainer.localRotation;

        float standT = 0;
        while (standT < 1)
        {
            standT += Time.deltaTime * standUpSpeed;
            cameraContainer.localPosition = Vector3.Lerp(currentBedPos, defaultStandPos, standT);
            cameraContainer.localRotation = Quaternion.Slerp(currentBedRot, defaultStandRot, standT);
            yield return null;
        }
        // --- ESCENA 6: RECUPERAR EL CONTROL ---
        playerController.EnableMovement();
        playerController.EnableLook();

       

        if (GameManager.Instance != null)
        {
            // 1. Lanzamos el pensamiento como Subtítulo (durará 4 segundos)
            GameManager.Instance.ShowSubtitle("<i>Ruth- ¿Qué fue eso?...</i>", 5f);

            yield return new WaitForSeconds(10f);
            // 2. Actualizamos la misión general en la esquina
            GameManager.Instance.UpdateMission("> Investiga la cocina", "");
        }

        Destroy(gameObject);
    }
}