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
    private MeshRenderer objectMeshRenderer;

    [SerializeField]
    private BoxCollider objectCollider;

    [SerializeField]
    private Vector3 spawnLocation;

    [SerializeField]
    private float gravitySpeed;

    [SerializeField]
    private Transform playerTransform;

    public bool isBroken = false;

    // Start is called before the first frame update
    private void Start()
    {
        // Set spawn location
        spawnLocation = transform.position;

        // Get reference to player transform component
        if (!playerTransform)
        {
            playerTransform = GameObject.Find("Player").transform;
        }

        // Get reference to mesh renderer component
        if (!objectMeshRenderer)
        {
            objectMeshRenderer = GetComponent<MeshRenderer>();
        }

        // Get reference to object collider component
        if (!objectCollider)
        {
            objectCollider = GetComponent<BoxCollider>();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // If this object is in a broken state, skip the update logic
        if (isBroken)
        {
            return;
        }

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

            // Check if there is ground under the object
            if(CheckGround())
            {
                isFalling = false;
            }

            // Reset this object to its spawn location if it fell off the level
            if (transform.position.y < -20.0f)
            {
                ResetObject();

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
        // Do a longer second raycast that checks for ramps
        else if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.49f, transform.position.z), shoveDirection, out hit, 1.0f))
        {
            if (hit.collider.gameObject.name == "GroundRampPlaceholder")
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
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z), Vector3.down, out hit, 0.5f))
        {
            // Make sure cube lands on grid one level above the ground hit
            transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 1, hit.transform.position.z);
            return true;
        }

        Debug.Log("No ground detected");
        return false;
    }

    /// <summary>
    /// Moves this object back to it's spawn location
    /// </summary>
    private void ResetObject()
    {
        // Check if player is at this object's spawn point location
        // If it is, move it to a new location
        // Otherwise move the object back to the original spawn location
        if (IsPlayerAtSpawnPoint())
        {
            // Move object two units to the right of the spawn point
            // If the object spawns off of the level, move it two units to the front of the spawn point
            transform.position = new Vector3(spawnLocation.x + 2, spawnLocation.y, spawnLocation.z);
            if (!CheckGround())
            {
                transform.position = new Vector3(spawnLocation.x, spawnLocation.y, spawnLocation.z + 2);
            }
        }
        else
        {
            transform.position = spawnLocation;
        }
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

    /// <summary>
    /// Puts this object in a broken state for x amount of seconds then resets the object
    /// </summary>
    /// <returns></returns>
    public IEnumerator BreakObject()
    {
        // Disables this object
        isBroken = true;
        objectMeshRenderer.enabled = false;
        objectCollider.enabled = false;
        beingShoved = false;
        isFalling = false;

        // Wait for 4 seconds
        yield return new WaitForSeconds(4f);

        // Reset this object
        ResetObject();
        objectMeshRenderer.enabled = true;
        objectCollider.enabled = true;
        isBroken = false;
    }
}
