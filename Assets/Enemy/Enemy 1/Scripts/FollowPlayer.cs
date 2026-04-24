using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : MonoBehaviour
{
    private GameObject player;
    private NavMeshAgent agent;

    //nuevo
    private PlayerSanity playerSanity;
    //

    [Header("Config Speed y Detection enemySof")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float detectionRange = 15f;


    //nuev
    [Header("Daño y Cordura")]
    [SerializeField] private float killDistance = 1.8f;
    [SerializeField] private float sanityDrainRange = 4f;
    [SerializeField] private float sanityDrainPerSecond = 5f;
    [SerializeField] private float eyeHeight = 1.6f;
    //

    //nuev
    [Header("Interacción con Puertas")]
    [SerializeField] private float doorReach = 1.5f;
    //
    
    //sof
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
    //


        //nuev

        agent.speed = speed;

        if (player != null)
        {
            playerSanity = player.GetComponentInChildren<PlayerSanity>();
        }
    }
    //nuev update
    void Update()
    {
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        CheckAndOpenDoorsAhead();

        Vector3 enemyPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerPosXZ = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        float distance = Vector3.Distance(enemyPosXZ, playerPosXZ);


        //sof
        if (distance < detectionRange)
        {
            agent.SetDestination(player.transform.position);

            if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                agent.ResetPath();
            }

            HandleDamage(distance);
        }
        else
        {
            agent.ResetPath();
        }
    }
    //

    //nuev

    private void HandleDamage(float currentDistance)
    {
        if (playerSanity == null) return;

        if (currentDistance <= killDistance)
        {
            playerSanity.LoseSanity(9999f);
            return;
        }

        if (currentDistance <= sanityDrainRange && HasLineOfSight())
        {
            playerSanity.LoseSanity(sanityDrainPerSecond * Time.deltaTime);
        }
    }
    //
    //nuev raycast
    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.transform.position + Vector3.up * 1f;
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
    //
    //nuev interactables
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
    //
}