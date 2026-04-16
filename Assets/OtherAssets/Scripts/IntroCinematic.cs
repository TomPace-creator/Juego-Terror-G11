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
    [SerializeField] private AudioSource kitchenNoiseSource;
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
        // --- ESCENA 1: EL ESTRUENDO INICIAL ---
        // ¡PUM! El sonido suena primero en plena pantalla negra para despertar a Ruth.
        if (kitchenNoiseSource != null && spookySound != null)
        {
            kitchenNoiseSource.PlayOneShot(spookySound);
        }

        // --- ESCENA 2: ABRIENDO LOS OJOS ---
        // sleepTime actúa como el tiempo de reacción entre el ruido y abrir los ojos
        yield return new WaitForSeconds(sleepTime);
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * wakeUpSpeed;
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // --- ESCENA 3: MIRANDO EL TECHO Y LUEGO AL RELOJ ---
        yield return new WaitForSeconds(ceilingStareTime);

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
        yield return new WaitForSeconds(0.4f); // Pausa breve en la oscuridad
        if (bedsideLamp != null && !bedsideLamp.GetIsOn())
        {
            bedsideLamp.Interact();
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

        // --- ESCENA 6: RECUPERAR EL CONTROL Y SECUENCIA DE DIÁLOGOS ---
        playerController.EnableMovement();
        playerController.EnableLook();

        if (GameManager.Instance != null)
        {
            // 1. Primer pensamiento apenas recupere el control (dura 4 seg)
            GameManager.Instance.ShowSubtitle("<i>Ruth - ¿Qué fue eso?...</i>", 4f);

            // Esperamos 5 segundos (4 que dura el texto + 1 segundo de silencio)
            yield return new WaitForSeconds(5f);

            // 2. Segundo pensamiento deductivo (dura 4 seg)
            GameManager.Instance.ShowSubtitle("<i>Parece que vino desde afuera...</i>", 4f);

            // Esperamos otros 5 segundos antes de mandar la misión
            yield return new WaitForSeconds(5f);

            // 3. Actualizamos la misión para que vaya a la ventana
            GameManager.Instance.UpdateMission("> Investiga por la ventana", "");
        }

        // Fin de la cinemática
        Destroy(gameObject);
    }
}