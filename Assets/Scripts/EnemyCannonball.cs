using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCannonball : MonoBehaviour
{
    private Vector3 velocity;

    [SerializeField]
    private float speed = 3.0f;

    [SerializeField]
    private float baseSpeed = 3.0f;

    // The max amount of time the projectile will stay active for
    [SerializeField]
    private float maxLifespan = 8.0f;

    private float lifespanTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If this projectile has reached it's max life span, deactivate it
        lifespanTimer -= Time.deltaTime;
        if (lifespanTimer <= 0)
        {
            DeactivateProjectile();
        }

        // Otherwise move the projectile
        transform.position += velocity * speed * Time.deltaTime;
    }

    /// <summary>
    /// Sets the projectiles velocity and resets it's lifespan timer
    /// </summary>
    /// <param name="velocity"></param>
    public void SetVelocity(Vector3 velocity)
    {
        this.velocity = velocity;
        lifespanTimer = maxLifespan;
    }

    /// <summary>
    /// Deactivates the projectile
    /// </summary>
    private void DeactivateProjectile()
    {
        velocity = Vector3.zero;
        speed = baseSpeed;
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the collision object is an enemy, stun the enemy
        string objectTag = other.gameObject.tag;
        if (objectTag == "Enemy")
        {
            EnemyBehavior enemyBehavior = other.GetComponent<EnemyBehavior>();
            if (!enemyBehavior.isStunned)
            {
                enemyBehavior.StartCoroutine("StunEnemy");
            }
        }
        // If the collision object is a shovable object, break the shovable object
        else if (objectTag == "Shovable")
        {
            other.GetComponent<ShovableObject>().StartCoroutine("BreakObject");
        }
        // If the collision object is a breakable object, deactivate the breakable object
        else if (objectTag == "Breakable")
        {
            other.gameObject.SetActive(false);
        }

        // Deactivate the projectile
        DeactivateProjectile();
    }

    public void SetProjetileSpeed(float speed)
    {
        this.speed = speed;
    }
}
