using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [Header("clips de audio")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip footstepSound;

    [Header("configuración de pasos")]
    [Tooltip("tiempo base entre pasos")]
    [SerializeField] private float baseFootstepInterval = 0.6f;

    private AudioSource audioSource;
    private NavMeshAgent agent;
    private float footstepTimer = 0f;

    void Start()
    {
       
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

       
        PlaySpawnSound();
    }

    void Update()
    {
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (agent == null) return;

       
        if (agent.velocity.magnitude > 0.1f)
        {
            footstepTimer += Time.deltaTime;


            float speedMultiplier = agent.speed / 4f;
            float currentInterval = baseFootstepInterval / Mathf.Max(speedMultiplier, 0.5f);

            if (footstepTimer >= currentInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
         
            footstepTimer = 0f;
        }
    }

   
    public void PlaySpawnSound()
    {
        if (audioSource != null && spawnSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.PlayOneShot(spawnSound);
        }
    }

    private void PlayFootstepSound()
    {
        if (audioSource != null && footstepSound != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f); 
            audioSource.PlayOneShot(footstepSound, 0.5f); 
        }
    }
}