using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("configuracion de spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("El Tag que tienen los puntos de spawn vacios en la escena")]
    [SerializeField] private string spawnPointTag = "EnemySpawn";

    // esta funcion la va a llamar el reloj a las 5 am mediante el UnityEvent
    public void SpawnearEnemigo()
    {
        if (enemyPrefab != null)
        {
            // busca en toda la escena todos los objetos que tengan este Tag
            GameObject[] points = GameObject.FindGameObjectsWithTag(spawnPointTag);

            if (points != null && points.Length > 0)
            {
                // elige un punto al azar de la lista que encontro
                int randomIndex = Random.Range(0, points.Length);
                Transform selectedPoint = points[randomIndex].transform;

                // crea al enemigo en ese punto
                Instantiate(enemyPrefab, selectedPoint.position, selectedPoint.rotation);

                Debug.Log("Nuevo enemigo spawneado a las 5 AM en: " + selectedPoint.name);
            }
            else
            {
                Debug.LogWarning("No se encontraron puntos de spawn con el tag: " + spawnPointTag);
            }
        }
    }
}