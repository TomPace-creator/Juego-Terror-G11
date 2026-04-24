using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float chaseSpeed = 5.5f;
    [Tooltip("Segundos que te sigue buscando después de perderte de vista")]
    [SerializeField] private float memoryTime = 5f;

    [Header("Modo Caza Absoluta (Sexto Sentido)")]
    [Tooltip("Segundos sin ver a Ruth antes de marchar hacia ella")]
    [SerializeField] private float timeToAbsoluteHunt = 60f;

    [Header("Daño de Cordura")]
    [Tooltip("Distancia a la que te atrapa (1.8 es ideal para que los colisionadores no estorben)")]
    [SerializeField] private float killDistance = 1.8f;
    [Tooltip("Distancia máxima a la que debe estar para quitarte cordura gradual")]
    [SerializeField] private float sanityDrainRange = 4f;
    [Tooltip("Cuánta cordura quita por SEGUNDO mientras te persigue y TE VE")]
    [SerializeField] private float sanityDrainPerSecond = 5f;
    [Tooltip("Altura de los ojos del monstruo para calcular si te ve")]
    [SerializeField] private float eyeHeight = 1.6f;

    [Header("Interacción con Puertas")]
    [SerializeField] private float doorReach = 1.5f;

    [Header("Configuración de Patrullaje")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float patrolRadius = 20f;
    [SerializeField] private float waitTimeAtDestination = 0.5f;

    [Header("Desaparición Táctica")]
    [SerializeField] private string spawnPointTag = "EnemySpawn";
    [SerializeField] private float vanishDuration = 20f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;
    [Tooltip("Sonido de los pasos del monstruo")]
    [SerializeField] private AudioClip footstepSound; 
    [Tooltip("Sonido estridente al entrar en Modo Caza Absoluta")]
    [SerializeField] private AudioClip absoluteHuntSound; 
    [SerializeField] private float footstepInterval = 0.6f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerSanity playerSanity;

    private float waitTimer = 0f;
    private float timeSinceLastSeen = 10f;
    private bool isVanished = false;

    private float footstepTimer = 0f;
    private bool hasPlayedHuntSound = false; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerSanity = playerObj.GetComponentInChildren<PlayerSanity>();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPillsConsumed += HandlePillsConsumed;
        }

        SetRandomPatrolDestination();
        PlaySpawnSound();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPillsConsumed -= HandlePillsConsumed;
        }
    }

    void Update()
    {
        if (playerTransform == null || !agent.isOnNavMesh || isVanished) return;

        CheckAndOpenDoorsAhead();

        
        HandleFootsteps();

        Vector3 enemyPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerPosXZ = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
        float distanceToPlayerXZ = Vector3.Distance(enemyPosXZ, playerPosXZ);

        bool inRange = distanceToPlayerXZ <= detectionRange;
        bool canSee = inRange && HasLineOfSight();

        if (canSee)
        {
            timeSinceLastSeen = 0f;
            hasPlayedHuntSound = false; 
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
        }

        bool isAbsoluteHunting = timeSinceLastSeen >= timeToAbsoluteHunt;

        if (isAbsoluteHunting && !hasPlayedHuntSound)
        {
            PlayAbsoluteHuntSound();
            hasPlayedHuntSound = true;
        }

        if (timeSinceLastSeen <= memoryTime || isAbsoluteHunting)
        {
            ChasePlayer(canSee, distanceToPlayerXZ);
        }
        else
        {
            Patrol();
        }
    }

    private void HandleFootsteps()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            footstepTimer += Time.deltaTime;

           
            float currentInterval = (agent.speed == chaseSpeed) ? footstepInterval * 0.7f : footstepInterval;

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

    private void ChasePlayer(bool isCurrentlySeeing, float distanceToPlayerXZ)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);

        if (distanceToPlayerXZ <= killDistance)
        {
            if (playerSanity != null)
            {
                playerSanity.LoseSanity(9999f);
            }
            return;
        }

        if (isCurrentlySeeing && distanceToPlayerXZ <= sanityDrainRange)
        {
            if (playerSanity != null)
            {
                playerSanity.LoseSanity(sanityDrainPerSecond * Time.deltaTime);
            }
        }
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtDestination)
            {
                SetRandomPatrolDestination();
                waitTimer = 0f;
            }
        }
    }

    private void SetRandomPatrolDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = playerTransform.position + Vector3.up * 1f;
        Vector3 direction = target - origin;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, detectionRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    private void CheckAndOpenDoorsAhead()
    {
        Vector3 origin = transform.position + Vector3.up * 1f;

        if (Physics.SphereCast(origin, 0.5f, transform.forward, out RaycastHit hit, doorReach))
        {
            Door normalDoor = hit.collider.GetComponent<Door>();
            if (normalDoor == null) normalDoor = hit.collider.GetComponentInParent<Door>();

            if (normalDoor != null && !normalDoor.isOpen)
            {
                normalDoor.ToggleDoor(transform, false);
                return;
            }

            SlideDoor slideDoor = hit.collider.GetComponent<SlideDoor>();
            if (slideDoor == null) slideDoor = hit.collider.GetComponentInParent<SlideDoor>();

            if (slideDoor != null && !slideDoor.isOpen)
            {
                slideDoor.Interact();
            }
        }
    }

    private void HandlePillsConsumed()
    {
        if (!gameObject.activeInHierarchy) return;

        if (!HasLineOfSight() && !isVanished)
        {
            StartCoroutine(VanishAndRespawnRoutine());
        }
    }

    private System.Collections.IEnumerator VanishAndRespawnRoutine()
    {
        isVanished = true;
        agent.enabled = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders) c.enabled = false;

        yield return new WaitForSeconds(vanishDuration);

        GameObject[] points = GameObject.FindGameObjectsWithTag(spawnPointTag);

        if (points != null && points.Length > 0)
        {
            Transform randomPoint = points[Random.Range(0, points.Length)].transform;
            transform.position = randomPoint.position;
            transform.rotation = randomPoint.rotation;
        }

        foreach (Renderer r in renderers) r.enabled = true;
        foreach (Collider c in colliders) c.enabled = true;

        agent.enabled = true;
        isVanished = false;

        timeSinceLastSeen = memoryTime + 0.1f;
        hasPlayedHuntSound = false; 
        SetRandomPatrolDestination();
        PlaySpawnSound();
    }

    private void PlaySpawnSound()
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

    private void PlayAbsoluteHuntSound()
    {
        if (audioSource != null && absoluteHuntSound != null)
        {
            audioSource.pitch = 0.5f; 
            audioSource.PlayOneShot(absoluteHuntSound, 1f);
        }
    }
}