using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float chaseSpeed = 5.5f;
    [Tooltip("te sigue buscando después de perderte de vista")]
    [SerializeField] private float memoryTime = 5f;

    [Header("Modo Caza Absoluta (Sexto Sentido)")]
    [Tooltip("si no te ve por 60 seg te encuentra igual")]
    [SerializeField] private float timeToAbsoluteHunt = 60f;

    [Header("Daño de Cordura")]
    [Tooltip("Distancia máxima a la que debe estar para quitarte cordura")]
    [SerializeField] private float sanityDrainRange = 3f;
    [Tooltip("Cuánta cordura quita por SEGUNDO")]
    [SerializeField] private float sanityDrainPerSecond = 5f;
    [Tooltip("Altura de los ojos del monstruo para calcular si te ve")]
    [SerializeField] private float eyeHeight = 1.6f;

    [Header("Interacción con Puertas")]
    [Tooltip("Distancia a la que el enemigo detecta y empuja una puerta")]
    [SerializeField] private float doorReach = 1.5f;

    [Header("Configuración de Patrullaje")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float patrolRadius = 20f;
    [SerializeField] private float waitTimeAtDestination = 0.5f;

    [Header("Desaparición Táctica")]
    [Tooltip("El Tag que le pondremos a los objetos vacíos del mapa")]
    [SerializeField] private string spawnPointTag = "EnemySpawn";
    [SerializeField] private float vanishDuration = 20f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnSound;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerSanity playerSanity;

    private float waitTimer = 0f;
    private float timeSinceLastSeen = 10f;
    private bool isVanished = false;

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

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distanceToPlayer <= detectionRange;
        bool canSee = inRange && HasLineOfSight();

        if (canSee)
        {
            timeSinceLastSeen = 0f; 
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
        }

     
        bool isAbsoluteHunting = timeSinceLastSeen >= timeToAbsoluteHunt;

       
        if (timeSinceLastSeen <= memoryTime || isAbsoluteHunting)
        {
            ChasePlayer(canSee, distanceToPlayer);
        }
        else
        {
            Patrol();
        }
    }

    private void ChasePlayer(bool isCurrentlySeeing, float distanceToPlayer)
    {
        agent.speed = chaseSpeed;

     
        agent.SetDestination(playerTransform.position);

        if (isCurrentlySeeing && distanceToPlayer <= sanityDrainRange && playerSanity != null)
        {
            playerSanity.LoseSanity(sanityDrainPerSecond * Time.deltaTime);
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
            Door door = hit.collider.GetComponent<Door>();
            if (door == null) door = hit.collider.GetComponentInParent<Door>();

            if (door != null && !door.isOpen)
            {
                door.ToggleDoor(transform, false);
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
}