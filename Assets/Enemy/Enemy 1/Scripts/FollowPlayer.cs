using UnityEngine;
using UnityEngine.AI;

public class FollowPlayer : MonoBehaviour    
{
    private GameObject player;
    private NavMeshAgent agent;
    private PlayerSanity playerSanity;

    [SerializeField] float speed = 3.5f;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] float sanityLossPerSecond = 10f;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            Debug.LogError("No se encontró ningún GameObject con el tag 'Player'");
            return;
        }

        playerSanity = player.GetComponent<PlayerSanity>();

        if (playerSanity == null)
        {
            Debug.LogError("El jugador no tiene el componente PlayerSanity");
        }
    }

    void Update()
    {
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < detectionRange) 
        {
            agent.SetDestination(player.transform.position);

            if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                agent.ResetPath();
            }

            if (playerSanity != null)
            {
                playerSanity.LoseSanity(sanityLossPerSecond * Time.deltaTime);
                Debug.Log("Quitando cordura. Cordura actual: " + playerSanity.currentSanity);
            }
        }
        else
        {
            agent.ResetPath();
        }    
    }
}


