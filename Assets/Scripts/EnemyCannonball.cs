using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCannonball : MonoBehaviour
{
    private Vector3 velocity;

    [SerializeField]
    private float speed = 3.0f;

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
        lifespanTimer -= Time.deltaTime;
        if (lifespanTimer <= 0)
        {
            DeactivateProjectile();
        }

        transform.position += velocity * speed * Time.deltaTime;
    }

    public void SetVelocity(Vector3 velocity)
    {
        this.velocity = velocity;
        lifespanTimer = maxLifespan;
    }

    private void DeactivateProjectile()
    {
        velocity = Vector3.zero;
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject hitObject = other.gameObject;

        if (hitObject.tag == "Enemy")
        {
            EnemyBehavior enemyBehavior = other.GetComponent<EnemyBehavior>();
            if (!enemyBehavior.isStunned)
            {
                enemyBehavior.StartCoroutine("StunEnemy");
            }
        }
        else if (hitObject.tag == "Shovable")
        {
            hitObject.GetComponent<ShovableObject>().StartCoroutine("BreakObject");
        }

        DeactivateProjectile();
    }
}
