using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Timeline;

public class ProjectilBehaviours : MonoBehaviour
{
    public enum Type
    {
        Strait,
        Smart,
        Aimed
    }

    [Header("General")]
    [SerializeField]
    private GameObject target;                  //Projectile's target
    public Type type;                           //Projectile type
    public float initSpeed = 1.0f;              //Set the initial Speed
    public float ttl = 5f;                      //Time to Live

    [Header("Acceleration")]
    public bool doAccelerate = false;           //Boolean to enable the Acceleration
    public float accelerationSpeed = 1.0f;      //Acceleration variable (linéar)
    public float maxSpeed = 10.0f;              //Maximum speed

    [Header("Aimed")]
    public float stopAimDistance = 5f;          //Stop follow the target at close range

    [Header("Smart")]
    public float spred = 2;                     //Precision of the smart calculus

    private Vector3 direction;                  //Direction of the projectile
    private float currentSpeed;                 //Current speed of the projectile

    void Start()
    {
        //Set the initial Speed
        currentSpeed = initSpeed;
        //Set the direction toward the target
        direction = (transform.position - target.transform.position)*-1;

        //If Smart type, anticipate the target trajectory
        if (type.Equals(Type.Smart)){
            float timeToImpact = direction.magnitude / currentSpeed;
            Vector2 predictedPos = (Vector2)target.transform.position + target.GetComponent<Rigidbody2D>().velocity*(timeToImpact/2);
            direction = (predictedPos - (Vector2)transform.position).normalized;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        //Destroy the object if his time is over
        ttl -= dt;
        if (ttl < 0)
        {
            Destroy(this.gameObject);
        }

        //Follow the targe if Aimed type
        if(type.Equals(Type.Aimed) && Vector3.Distance(transform.position, target.transform.position) > stopAimDistance) {
            direction = (transform.position - target.transform.position) * -1;
        }

        //Move toward the target direction
        //if (doAccelerate)currentSpeed = Mathf.Clamp(currentSpeed, currentSpeed*accelerationSpeed*Time.deltaTime, maxSpeed);
        transform.position += direction.normalized * currentSpeed * dt;
    }

    //Destroy if touch a entity
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) Debug.Log("Player Hit !");
        else Debug.Log("Collide with : " + other.name);
        Destroy(this.gameObject);
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
    }
}
