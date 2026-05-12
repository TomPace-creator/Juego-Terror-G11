using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour

{
    [Header("Configuración de Persecución")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float chaseSpeed = 5.5f;
    [SerializeField] private float memoryTime = 5f;

    [Header("Modo Caza Absoluta")]
    [SerializeField] private float timeToAbsoluteHunt = 60f;

    [Header("Daño de Cordura")]
    [SerializeField] private float killDistance = 1.8f;
    [SerializeField] private float sanityDrainRange = 4f;
    [SerializeField] private float sanityDrainPerSecond = 5f;
    [SerializeField] private float eyeHeight = 1.6f;

    [Header("Mecánicas Defensivas del Jugador")]
    [SerializeField] private float doorReach = 1.5f;
    [SerializeField] private float doorHesitationTime = 1.5f;

    [Header("Mecánica de Aturdimiento (Linterna)")]
    [Tooltip("Segundos que debes mirarlo fijamente para que te reste cordura de golpe")]
    [SerializeField] private float timeToDrainSanityWhenStunned = 5f;
    [Tooltip("Cantidad de cordura que te quita tras mirarlo esos 5 segundos")]
    [SerializeField] private float stunSanityDamage = 15f;

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
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip absoluteHuntSound;
    [SerializeField] private float footstepInterval = 0.6f;
    [Header("Audio de Pasos")]
    [Tooltip("Segundos entre cada paso cuando patrulla (Lento)")]
    [SerializeField] private float patrolFootstepInterval = 1.2f;
    [Tooltip("Segundos entre cada paso cuando persigue (Más rápido)")]
    [SerializeField] private float chaseFootstepInterval = 0.8f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerSanity playerSanity;

    private float waitTimer = 0f;
    private float timeSinceLastSeen = 10f;

    // Control de tiempos de Aturdimiento
    private float timeSinceLastIlluminated = 999f;
    private float continuousIlluminationTime = 0f;

    private bool isVanished = false;
    private bool isInteractingWithDoor = false;
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

        if (GameManager.Instance != null) GameManager.Instance.OnPillsConsumed += HandlePillsConsumed;

        SetRandomPatrolDestination();
        PlaySpawnSound();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnPillsConsumed -= HandlePillsConsumed;
    }

    void Update()
    {
        if (playerTransform == null || !agent.isOnNavMesh || isVanished || isInteractingWithDoor) return;

        // --- SISTEMA DE ATURDIMIENTO EN TIEMPO REAL ---
        timeSinceLastIlluminated += Time.deltaTime;

        // Si fue iluminado hace menos de 0.1 segundos, se considera aturdido
        bool isStunned = timeSinceLastIlluminated < 0.1f;

        if (isStunned)
        {
            agent.isStopped = true;
            continuousIlluminationTime += Time.deltaTime;

            // Si llegamos a los 5 segundos mirándolo fijamente...
            if (continuousIlluminationTime >= timeToDrainSanityWhenStunned)
            {
                if (playerSanity != null) playerSanity.LoseSanity(stunSanityDamage);

                // Aquí en el futuro puedes disparar el trigger del Animator:
                // animator.SetTrigger("TwitchHead");

                continuousIlluminationTime = 0f; // Reiniciamos el reloj para el próximo golpe de cordura
            }
            return; // Cortamos el Update para que no persiga ni patrulle
        }
        else
        {
            // Si dejamos de mirarlo, reseteamos el tiempo continuo y le devolvemos el movimiento
            continuousIlluminationTime = 0f;
            agent.isStopped = false;
        }

        // --- FIN SISTEMA DE ATURDIMIENTO ---

        CheckAndOpenDoorsAhead();
        if (isInteractingWithDoor) return;

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

        if (timeSinceLastSeen <= memoryTime || isAbsoluteHunting) ChasePlayer(canSee, distanceToPlayerXZ);
        else Patrol();
    }

    // La linterna llama a esta función cada frame que le apunta
    public void StunByFlashlight()
    {
        timeSinceLastIlluminated = 0f;
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
                StartCoroutine(DoorHesitationRoutine(normalDoor, null));
                return;
            }

            SlideDoor slideDoor = hit.collider.GetComponent<SlideDoor>();
            if (slideDoor == null) slideDoor = hit.collider.GetComponentInParent<SlideDoor>();

            if (slideDoor != null && !slideDoor.isOpen)
            {
                StartCoroutine(DoorHesitationRoutine(null, slideDoor));
            }
        }
    }

    private IEnumerator DoorHesitationRoutine(Door normalDoor, SlideDoor slideDoor)
    {
        isInteractingWithDoor = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(doorHesitationTime);

        if (normalDoor != null) normalDoor.ToggleDoor(transform, false);
        if (slideDoor != null) slideDoor.Interact();

        agent.isStopped = false;
        isInteractingWithDoor = false;
    }

    private void HandleFootsteps()
    {
        // Solo cuenta pasos si el monstruo se está moviendo físicamente
        if (agent.velocity.magnitude > 0.1f)
        {
            footstepTimer += Time.deltaTime;

            // Determina qué intervalo usar dependiendo de la velocidad actual del NavMeshAgent
            float currentInterval = (agent.speed == chaseSpeed) ? chaseFootstepInterval : patrolFootstepInterval;

            if (footstepTimer >= currentInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f; // Reinicia el temporizador
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
            if (playerSanity != null) playerSanity.LoseSanity(9999f);
            return;
        }

        if (isCurrentlySeeing && distanceToPlayerXZ <= sanityDrainRange)
        {
            if (playerSanity != null) playerSanity.LoseSanity(sanityDrainPerSecond * Time.deltaTime);
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
            if (hit.transform.CompareTag("Player")) return true;
        }
        return false;
    }

    private void HandlePillsConsumed()
    {
        if (!gameObject.activeInHierarchy) return;
        if (!HasLineOfSight() && !isVanished) StartCoroutine(VanishAndRespawnRoutine());
    }

    private IEnumerator VanishAndRespawnRoutine()
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