using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovableObject : MonoBehaviour
{
    public bool beingShoved = false;

    public bool beingChargedShoved = false;

    [SerializeField]
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
    private float speed;

    [SerializeField]
    private float distanceToTravel;

    [SerializeField]
    private Transform playerTransform;

    public bool isBroken = false;

    [SerializeField]
    private GameManager gameManager;

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

        if (!gameManager)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // If this object is in a broken state or the game is paused, skip the update logic
        if (isBroken || gameManager.isGamePaused)
        {
            return;
        }

        // If this object is being shoved, move the object towards the target shove location
        if (beingShoved)
        {
            //Debug.Log("Shoving");
            transform.position = Vector3.SmoothDamp(transform.position, shoveTargetLocation, ref shoveDirection, speed);

            // If this object has reached its location (or very close to it), it should no longer be in the shoved state
            if (Vector3.Distance(transform.position, shoveTargetLocation) < 0.05f)
            {
                // Snap the object to the target location to keep it on the grid
                transform.position = shoveTargetLocation;

                // Set beingShoved to false
                beingShoved = false;
                beingChargedShoved = false;

                //Debug.Log("Got to shove location");

                //Debug.Log("Checking for ground");
                // If there is no ground under the object start falling
                if (!CheckGround())
                {
                    isFalling = true;
                }
            }
        }

        // If the object is falling, apply gravity
        if (isFalling)
        {

            //Debug.Log("Falling");

            //Debug.Log("Checking for ground while falling");
            // Check if there is ground under the object
            if (CheckGround())
            {
                //Debug.Log("Not falling anymore");
                isFalling = false;
            }
            else
            {
                // Start applying gravity
                transform.position += Vector3.down * gravitySpeed * Time.deltaTime;
                beingShoved = false;
                beingChargedShoved = false;
            }

            // Reset this object to its spawn location if it fell off the level
            if (transform.position.y < -20.0f)
            {
                ResetObject();

                // The object is no longer in a falling state
                isFalling = false;
            }
        }

        if (beingChargedShoved)
        {
            //Debug.Log("Checking for ground charged shove");
            if (!CheckGround())
            {
                //Debug.Log("No ground for charged shove");
                StartCoroutine(DelayFall());
                beingChargedShoved = false;
            }
        }
    }

    /// <summary>
    /// Checks if this object can be shoved in the desired direction
    /// If it can be shoved, set the shove variables
    /// </summary>
    /// <param name="shoveDirection"></param>
    /// <param name="shoveTargetLocation"></param>
    /*public void Shove(Vector3 shoveDirection, Vector3 shoveTargetLocation)
    {
        Debug.Log("Trying to shove block");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, shoveDirection, out hit, 1.0f))
        {
            if (hit.collider.tag != "InvisibleBoundsWall" && hit.collider.tag != "Checkpoint")
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
    }*/

    /// <summary>
    /// Checks if this object can be shoved in the desired direction
    /// If it can be shoved, set the shove variables
    /// </summary>
    /// <param name="shoveDirection"></param>
    /// <param name="shoveTargetLocation"></param>
    /// /// <param name="distance"></param>
    /// <param name="speed"></param>
    /// <param name="minDistanceSnap"></param>
    /// <param name="beingChargedShoved"></param>
    public void Shove(Vector3 shoveDirection, Vector3 shoveTargetLocation, float distance, float speed, float minDistanceSnap, bool beingChargedShoved)
    {
        Debug.Log("Trying to shove block at speed of: " + speed);
        RaycastHit hit;
        if (Physics.Linecast(new Vector3(transform.position.x, transform.position.y - 0.40f, transform.position.z), shoveTargetLocation, out hit))
        {
            if (hit.collider.tag != "InvisibleBoundsWall" && hit.collider.tag != "Checkpoint" && hit.collider.tag != "BreakableWall")
            {
                Debug.Log("Cannot be shoved, there is an object in the way");
                return;
            }

            if (hit.collider.tag == "BreakableWall")
            {
                hit.collider.gameObject.SetActive(false);
            }
        }
        // Do a longer second raycast that checks for ramps
        else if (Physics.Linecast(new Vector3(transform.position.x, transform.position.y - 0.40f, transform.position.z), shoveTargetLocation, out hit))
        {
            if (hit.collider.gameObject.name == "GroundRampPlaceholder")
            {
                Debug.Log("Cannot be shoved, there is a ramp in the way");
                return;
            }
        }

        SoundManager.PlaySound(SoundManager.Sound.stoneSliding);
        beingShoved = true;
        distanceToTravel = distance;
        this.beingChargedShoved = beingChargedShoved;
        this.shoveDirection = shoveDirection;
        this.shoveTargetLocation = shoveTargetLocation;
        this.speed = speed;
    }

    /// <summary>
    /// Checks if there is any ground under this object
    /// </summary>
    /// <returns></returns>
    private bool CheckGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z), Vector3.down, out hit, 0.6f))
        {
            if (isFalling)
            {
                // Make sure cube lands on grid one level above the ground hit
                transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 1, hit.transform.position.z);
            }
            return true;
        }

        //Debug.Log("No ground detected");
        return false;
    }

    /// <summary>
    /// Checks if there is any ground under specific location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    private bool CheckGround(Vector3 location)
    {
        RaycastHit hit;
        if (Physics.Raycast(location, Vector3.down, out hit, 0.6f))
        {
            if (isFalling)
            {
                // Make sure cube lands on grid one level above the ground hit
                transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y + 1, hit.transform.position.z);
            }
            return true;
        }

        //Debug.Log("No ground detected");
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
            /*transform.position = new Vector3(spawnLocation.x + 2, spawnLocation.y, spawnLocation.z);
            if (!CheckGround())
            {
                transform.position = new Vector3(spawnLocation.x, spawnLocation.y, spawnLocation.z + 2);
            }*/

            // Move unit to first location 2 units from the player, with ground, and no objects in the way
            // Checks 4 positions around player (right, left, front, back)
            Vector3 newTargetSpawnLocation1 = new Vector3(spawnLocation.x + 2, spawnLocation.y, spawnLocation.z);
            Vector3 newTargetSpawnLocation2 = new Vector3(spawnLocation.x - 2, spawnLocation.y, spawnLocation.z);
            Vector3 newTargetSpawnLocation3 = new Vector3(spawnLocation.x, spawnLocation.y, spawnLocation.z + 2);
            Vector3 newTargetSpawnLocation4 = new Vector3(spawnLocation.x, spawnLocation.y, spawnLocation.z - 2);
            RaycastHit hit;
            if (!Physics.Linecast(spawnLocation, newTargetSpawnLocation1, out hit) && CheckGround(newTargetSpawnLocation1))
            {
                transform.position = newTargetSpawnLocation1;
            }
            else if (!Physics.Linecast(spawnLocation, newTargetSpawnLocation2, out hit) && CheckGround(newTargetSpawnLocation2))
            {
                transform.position = newTargetSpawnLocation2;
            }
            else if (!Physics.Linecast(spawnLocation, newTargetSpawnLocation3, out hit) && CheckGround(newTargetSpawnLocation3))
            {
                transform.position = newTargetSpawnLocation3;
            }
            else if (!Physics.Linecast(spawnLocation, newTargetSpawnLocation4, out hit) && CheckGround(newTargetSpawnLocation4))
            {
                transform.position = newTargetSpawnLocation4;
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

    /// <summary>
    /// Resets this shovable object to spawn point
    /// </summary>
    public void ResetShovableObject()
    {
        StopAllCoroutines();
        isBroken = isFalling = beingShoved = false;
        transform.position = spawnLocation;
    }

    private IEnumerator DelayFall()
    {
        float distanceToTargetRatio =  (distanceToTravel - Vector3.Distance(transform.position, shoveTargetLocation)) / distanceToTravel;
        if (distanceToTargetRatio >= .8)
        {
            yield return new WaitForSeconds(0.6f);
        }
        else
        {
            yield return new WaitForSeconds(Mathf.Clamp(0.2f * (distanceToTargetRatio), 0.07f, 0.3f));
        }
        beingShoved = false;
        beingChargedShoved = false;
        isFalling = true;
    }
}
