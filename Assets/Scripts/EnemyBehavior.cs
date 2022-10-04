using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    List<GameObject> cannonballs;

    private int cannonballIndex;

    [SerializeField]
    private Transform firePoint;

    // How often the enemy shoots a projectile
    [SerializeField]
    private float fireRate = 4.0f;

    private float cooldownTimer;

    [SerializeField]
    private Transform playerTransform;

    private bool isPlayerInRange = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!playerTransform)
        {
            playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            CheckIsPlayerInRange();
            if (isPlayerInRange)
            {
                FireProjectile();
                cooldownTimer = fireRate;
            }
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void FireProjectile()
    {
        GameObject cannonBall = cannonballs[cannonballIndex];
        cannonBall.SetActive(true);
        cannonBall.transform.position = firePoint.position;
        cannonBall.GetComponent<EnemyCannonball>().SetVelocity(transform.forward);
        cannonballIndex++;
        if (cannonballIndex == cannonballs.Count)
        {
            cannonballIndex = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckIsPlayerInRange()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) <= 15)
        {
            isPlayerInRange = true;
        }
        else
        {
            isPlayerInRange = false;
        }
    }
}
