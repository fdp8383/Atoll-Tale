using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovableObject : MonoBehaviour
{
    public bool beingShoved = false;

    private bool isFalling = false;

    private Vector3 shoveDirection;

    private Vector3 shoveTargetLocation;

    [SerializeField]
    private Vector3 spawnLocation;

    [SerializeField]
    private float gravitySpeed;

    [SerializeField]
    private Transform playerTransform;

    // Start is called before the first frame update
    private void Start()
    {
        // Set spawn location
        spawnLocation = transform.position;

        // Get reference to player transform
        if (!playerTransform)
        {
            playerTransform = GameObject.Find("Player").transform;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // If this object is being shoved, move the object towards the target shove location
        if (beingShoved)
        {
            transform.position = Vector3.SmoothDamp(transform.position, shoveTargetLocation, ref shoveDirection, 0.3f);

            // If this object has reached its location (or very close to it), it should no longer be in the shoved state
            if (Vector3.Distance(transform.position, shoveTargetLocation) < 0.05f)
            {
                // Snap the object to the target location to keep it on the grid
                transform.position = shoveTargetLocation;

                // Set beingShoved to false
                beingShoved = false;

                Debug.Log("Got to shove location");

                // If there is no ground under the object start falling
                if (!CheckGround())
                {
                    Debug.Log("Checking for ground");
                    isFalling = true;
                }
            }
        }

        // If the object is falling, apply gravity
        if (isFalling)
        {

            Debug.Log("Falling");

             // Start applying gravity
             transform.position += Vector3.down * gravitySpeed * Time.deltaTime;

             // Reset this object to its spawn location if it fell off the level
             if (transform.position.y < -20.0f)
             {
                 // Check if player is at this object's spawn point location
                 // If it is, move it to a new location 2 units to the right
                 // Otherwise move the object back to the original spawn location
                 if (IsPlayerAtSpawnPoint())
                 {
                     transform.position = new Vector3(spawnLocation.x + 2, spawnLocation.y, spawnLocation.z);
                 }
                 else
                 {
                     transform.position = spawnLocation;
                 }

                 // The object is no longer in a falling state
                 isFalling = false;
             }
        }
    }

    /// <summary>
    /// Checks if this object can be shoved in the desired direction
    /// If it can be shoved, set the shove variables
    /// </summary>
    /// <param name="shoveDirection"></param>
    /// <param name="shoveTargetLocation"></param>
    public void Shove(Vector3 shoveDirection, Vector3 shoveTargetLocation)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, shoveDirection, out hit, 1.0f))
        {
            if (hit.collider.tag != "InvisibleBoundsWall")
            {
                Debug.Log("Cannot be shoved, there is an object in the way");
                return;
            }
        }

        beingShoved = true;
        this.shoveDirection = shoveDirection;
        this.shoveTargetLocation = shoveTargetLocation;
    }

    /// <summary>
    /// Checks if there is any ground under this object
    /// </summary>
    /// <returns></returns>
    private bool CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z), Vector3.down, out hit, 1.0f))
        {
            return true;
        }

        Debug.Log("No ground detected");
        return false;
    }

    /// <summary>
    /// Checks if player is at this object's spawn point location
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerAtSpawnPoint()
    {
        // Use box collision method to check if player is within bounds of this object's spawn location
        Vector3 playerPosition = playerTransform.position;
        if (playerPosition.x + 0.5f >= spawnLocation.x - 0.5f &&
            playerPosition.x  - 0.5f<= spawnLocation.x + 0.5f &&
            playerPosition.z + 0.5f >= spawnLocation.z - 0.5f &&
            playerPosition.z - 0.5f <= spawnLocation.z + 0.5f)
        {
            return true;
        }

        return false;
    }
}
