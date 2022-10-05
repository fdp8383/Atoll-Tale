using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    private EnemyCannonballManager enemyCannonballManager;

    [SerializeField]
    private Transform firePoint;

    // How often the enemy shoots a projectile
    [SerializeField]
    private float fireRate = 4.0f;

    private float cooldownTimer;

    [SerializeField]
    private float enemyDetectionDistance = 15.0f;

    [SerializeField]
    private Transform playerTransform;

    private bool isPlayerInRange = false;

    public bool isStunned = false;

    [SerializeField]
    private Material enemyMaterial;

    [SerializeField]
    private Material enemyStunnedMatertial;

    [SerializeField]
    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        if (!playerTransform)
        {
            playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        }

        // Get reference to renderer component
        if (!rend)
        {
            rend = GetComponent<Renderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStunned)
        {
            FacePlayerDirection();

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
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void FireProjectile()
    {
        int cannonballIndex = enemyCannonballManager.cannonballIndex;
        GameObject cannonBall = enemyCannonballManager.cannonballs[cannonballIndex];
        cannonBall.SetActive(true);
        cannonBall.transform.position = firePoint.position;
        cannonBall.GetComponent<EnemyCannonball>().SetVelocity(transform.forward);
        enemyCannonballManager.cannonballIndex++;
        if (cannonballIndex == enemyCannonballManager.cannonballs.Count)
        {
            enemyCannonballManager.cannonballIndex = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckIsPlayerInRange()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) <= enemyDetectionDistance)
        {
            isPlayerInRange = true;
        }
        else
        {
            isPlayerInRange = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void FacePlayerDirection()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        Quaternion rotation;
        float smoothValue = 5.0f;
        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.z))
        {
            if (directionToPlayer.x > 0)
            {
                rotation = Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                rotation = Quaternion.Euler(0f, 270f, 0f);
            }
        }
        else
        {
            if (directionToPlayer.z > 0)
            {
                rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, smoothValue * Time.deltaTime);
        transform.rotation = rotation;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator StunEnemy()
    {
        isStunned = true;
        rend.sharedMaterial = enemyStunnedMatertial;

        yield return new WaitForSeconds(3f);

        isStunned = false;
        rend.sharedMaterial = enemyMaterial;
        cooldownTimer = fireRate;
    }
}
