using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class EnemyMovement : MonoBehaviour
{
    public int rutine;
    public float chronometer;
    public Quaternion angle;
    public float degree;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void EnemyBehaviour()
        {
        chronometer += 1 * Time.deltaTime;
        if (chronometer >=4)
        {
            rutine = Random.Range(0, 2);
            chronometer = 0;
        }
        switch (rutine)
        {
            case 1: 
                degree = Random.Range(0, 360);
                angle = Quaternion.Euler(0, degree, 0);
                rutine++;
                break;
            
            case 2:
                transform.rotation = Quaternion.RotateTowards(transform.rotation, angle, 0.5f);
                transform.Translate(Vector3.forward * 1 * Time.deltaTime);
                break;
        }
    }

    void Update()
    {
        EnemyBehaviour();
    }
}
    
