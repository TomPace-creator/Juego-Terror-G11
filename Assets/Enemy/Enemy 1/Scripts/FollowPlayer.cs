using UnityEngine;
using UnityEngine.AI;
public class FollowPlayer : MonoBehaviour    
{
    private GameObject player;
    private NavMeshAgent agent;
    [SerializeField] float speed;
    [SerializeField] float detectionRange; 
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null) return;
        if (!agent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if(distance < detectionRange) 
        {
	        agent.SetDestination(player.transform.position);
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                agent.ResetPath();
            }
        }
        else
        {
            agent.ResetPath();
        }    
        
    }
}



