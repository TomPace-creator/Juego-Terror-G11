using UnityEngine;
using UnityEngine.InputSystem; // ya van varias veces que me olvido el inputsystem

[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Configuración de Sonido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Ritmo de los Pasos")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    private float stepTimer;


    private Vector2 moveInput;
    private bool isRunning;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        stepTimer = walkStepInterval;
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    void Update()
    {
        
        bool isMoving = moveInput.magnitude > 0.1f;

        if (isMoving)
        {
            float currentInterval = isRunning ? runStepInterval : walkStepInterval;

            stepTimer += Time.deltaTime;

            if (stepTimer >= currentInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        else
        {
           
            stepTimer = walkStepInterval;
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepClips.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, footstepClips.Length);

            // variacion de tone
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(footstepClips[randomIndex]);
        }
    }
}