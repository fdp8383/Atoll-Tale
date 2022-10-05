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
    private float stunDuration = 3.0f;

    [SerializeField]
    private Material enemyMaterial;

    [SerializeField]
    private Material enemyStunnedMatertial;

    [SerializeField]
    private Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to player transform component
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
        // Only update this enemy if it is not stunned
        if (!isStunned)
        {
            // Update the rotation to face the player
            FacePlayerDirection();

            // If the cooldown timer reaches 0, and the player is in range, 
            // fire a projectile and reset the cooldown timer
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
    /// Fires a projectile from the enemy cannonball pool
    /// </summary>
    private void FireProjectile()
    {
        // TODO: Add logic to select a new cannonball if the current cannonball selected is already active
        // If all the cannonballs in the pool are active, spawn a new one and add it to the pool

        // Gets reference to cannonball index and the selected cannonball's game object
        int cannonballIndex = enemyCannonballManager.cannonballIndex;
        GameObject cannonBall = enemyCannonballManager.cannonballs[cannonballIndex];

        // Activates the cannonball by moving it to the firepoint and setting the velocity
        cannonBall.SetActive(true);
        cannonBall.transform.position = firePoint.position;
        cannonBall.GetComponent<EnemyCannonball>().SetVelocity(transform.forward);

        // Increment cannonball index, if it reaches the end of the cannonball list, reset it to 0
        if (cannonballIndex == enemyCannonballManager.cannonballs.Count - 1)
        {
            enemyCannonballManager.cannonballIndex = 0;
        }
        else 
        {
            enemyCannonballManager.cannonballIndex++;
        }
    }

    /// <summary>
    /// Checks if player is within the range of this enemy
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
    /// Rotates the enemy to look in the direction of the player
    /// </summary>
    private void FacePlayerDirection()
    {
        // TODO: Implement smooth rotation
        // Need to set up a way to know when the enemy is rotating to prevent them
        // from firing at a weird angle during the rotation

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        Quaternion rotation;
        //float smoothValue = 5.0f;

        // Check if the player is more in the x or z direction from the enemy
        // Then check if the player is more in the positive or negative direction
        // and set the rotation accordingly
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
    /// Stuns the enemy, effectively stopping its update function for x amount of seconds
    /// </summary>
    /// <returns></returns>
    public IEnumerator StunEnemy()
    {
        // Sets isStunned to true and changes the material to the stunned material
        isStunned = true;
        rend.sharedMaterial = enemyStunnedMatertial;

        // Waits for the stun duration
        yield return new WaitForSeconds(stunDuration);

        // Sets isStunned to false and changes material back to enemy material
        // Also resets the cooldown timer
        isStunned = false;
        rend.sharedMaterial = enemyMaterial;
        cooldownTimer = fireRate;
    }
}
