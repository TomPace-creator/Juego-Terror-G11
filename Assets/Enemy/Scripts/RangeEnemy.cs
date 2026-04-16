using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

public class RangeEnemy : MonoBehaviour
{
    public NavMeshAgent Enemy;
    public float Velocity;
    public bool Following;
    public float Range;
    float Distance;

    public Transform Target;

    [Header("Configuración de Merodeo")]
    public float RadioMerodeo = 10f;
    public float TiempoEspera = 3f;
    public float VelocidadMerodeo = 2f;
    private bool estaEsperando = false;

    private void Start()
    {
        if (Target == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) Target = player.transform;
        }

        if (Enemy == null) Enemy = GetComponent<NavMeshAgent>();
        
        Enemy.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    private void Update()
    {
        Distance = Vector3.Distance(transform.position, Target.position);

        // Lógica de detección
        if (Distance < Range)
        {
            Following = true;
        }
        else if (Distance > Range + 5) 
        {
            Following = false;
        }

        if (Following)
        {
            Perseguir();
        }
        else
        {
            Merodear();
        }
    }

    void Perseguir()
    {
        StopAllCoroutines(); 
        estaEsperando = false;

        Enemy.speed = Velocity;
        Enemy.SetDestination(Target.position);
    }

    void Merodear()
    {
        Enemy.speed = VelocidadMerodeo;

        if (Enemy.hasPath && Enemy.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Enemy.ResetPath(); // Limpiamos el camino fallido
        }

        if (!Enemy.pathPending && Enemy.remainingDistance <= Enemy.stoppingDistance && !estaEsperando)
        {
            StartCoroutine(EsperarYBuscarNuevoPunto());
        }

    }

    IEnumerator EsperarYBuscarNuevoPunto()
    {
        estaEsperando = true;
        
        
        yield return new WaitForSeconds(TiempoEspera);

        
        Vector3 direccionAleatoria = Random.insideUnitSphere * RadioMerodeo;
        direccionAleatoria += transform.position;

        NavMeshHit hit;
        
        if (NavMesh.SamplePosition(direccionAleatoria, out hit, RadioMerodeo, 1))
        {
            Enemy.SetDestination(hit.position);
        }

        estaEsperando = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Enemy.transform.position, Range);

        if (Enemy != null && Enemy.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, Enemy.destination);
        }
    }
}

