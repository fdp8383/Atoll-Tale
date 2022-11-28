using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReflectionHitbox : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        if (!playerController)
        {
            playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Projectile")
        {
            playerController.currentProjectile = other.GetComponent<EnemyCannonball>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Projectile")
        {
            // This check is for a situation where more than one projectile may have entered the player's hitbox
            if (other.GetComponent<EnemyCannonball>() == playerController.currentProjectile)
            {
                playerController.currentProjectile = null;
            }
        }
    }
}
