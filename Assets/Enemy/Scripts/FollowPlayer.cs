using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    GameObject player;
    [SerializeField] float speed;
    [SerializeField] float detectionRange; 
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);

        if(distance < detectionRange) 
        {
	        Vector3 direction = (player.transform.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);

            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

}


