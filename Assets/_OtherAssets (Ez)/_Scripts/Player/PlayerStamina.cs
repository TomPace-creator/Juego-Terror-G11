using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("configuracion de estamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrain = 20f;
    [SerializeField] private float staminaRegen = 15f;
    [SerializeField] private float staminaCooldown = 2f;

    [Header("audio de cansancio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip breathingClip;
    [SerializeField] private float maxBreathingVolume = 1f; // nuevo: limite de volumen para que no sature

    private float currentStamina;
    private float cooldownTimer;
    private bool isExhausted = false;

    private Controller playerController;

    void Start()
    {
        currentStamina = maxStamina;
        playerController = GetComponent<Controller>();

        if (audioSource != null && breathingClip != null)
        {
            audioSource.clip = breathingClip;
            audioSource.loop = true;
        }
    }

    void Update()
    {
        bool tryingToSprint = playerController.IsSprinting && playerController.IsMoving;

        if (tryingToSprint && !isExhausted)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            cooldownTimer = 0f;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;

                // arranca la respiracion al volumen maximo que le pusiste
                if (audioSource != null && !audioSource.isPlaying)
                {
                    audioSource.volume = maxBreathingVolume;
                    audioSource.Play();
                }
            }
        }
        else
        {
            if (cooldownTimer < staminaCooldown)
            {
                cooldownTimer += Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRegen * Time.deltaTime;

                // fade out: le baja el volumen a medida que recupera el aire
                if (audioSource != null && audioSource.isPlaying)
                {
                    // calcula el porcentaje de energia (ej: 0.5 si va por la mitad)
                    float staminaPercent = currentStamina / maxStamina;

                    // invierte el valor para el volumen (mucha energia = poco volumen)
                    audioSource.volume = maxBreathingVolume * (1f - staminaPercent);
                }

                // cuando llega al 100%, apaga el audio del todo
                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;

                    if (audioSource != null && audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }

                // te deja volver a correr al llegar al 20%, pero sigue jadeando bajito
                if (isExhausted && currentStamina >= (maxStamina * 0.2f))
                {
                    isExhausted = false;
                }
            }
        }
    }

    public bool CanSprint => !isExhausted && currentStamina > 0f;
}