using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private Transform mainCamera;

    [SerializeField]
    private CharacterController playerController;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private GameObject shovel;

    [SerializeField]
    private GameObject pogoStick;

    private bool hasShovel = false;

    private bool hasPogoStick = false;

    [SerializeField]
    private Material dugMaterial;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float gravity = -9.81f;

    [SerializeField]
    private float projectileReflectionSpeed = 6.0f;

    private Vector3 input;

    private Vector3 velocity;

    private float verticalVelocity;

    private bool startJump = false;
    private bool isJumping = false;
    private bool successfulJump = false;

    [SerializeField]
    private float heightOffset = 0.49f;

    private InteractableObject currentInteractable;

    private bool reset;

    // Start is called before the first frame update
    private void Start()
    {
        // If there is no game manager stored, find the game manager
        if (!gameManager)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // If there is no player controller stored, find the player controller component on the player
        if (!playerController)
        {
            playerController = this.gameObject.GetComponent<CharacterController>();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Reset the player if the reset input is triggered
        if (reset)
        {
            transform.position = new Vector3(1.25f, 1.47000003f, -51.2200012f);
            reset = false;
            return;
        }

        UpdateMovement();
    }

    /// <summary>
    /// Updates player movement relative to the camera
    /// </summary>
    private void UpdateMovement()
    {
        // Calculates velocity based on the camera's current rotation and input vector
        velocity = mainCamera.rotation * input * speed;
        velocity.y = 0.0f;

        // Rotates player to face the velocity/move direction
        if (input.magnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }

        // Set default vertical velocity to -0.5f so playerController.IsGrounded check works properly
        velocity.y = -0.5f;

        // If player is having a successful jump, move forward
        if (isJumping && successfulJump)
        {
            velocity += transform.forward;
        }

        // Start jump logic is meant to ensure the player's jump logic runs once before a isGrounded check
        // overwrites the isJumping bool
        if (startJump)
        {
            startJump = false;
            isJumping = true;
            verticalVelocity += gravity * Time.deltaTime;
            velocity.y += verticalVelocity;
        }
        // Once the startJump logic has run once, do normal ground check
        else
        {
            // If player is grounded, set isJumping to false and add no vertical velocity to y axis
            if (playerController.isGrounded)
            {
                isJumping = false;
                //Debug.Log("Grounded");
            }
            // Otherwise the player is not on the ground and should have gravity and the vertical velocity applied
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
                velocity.y += verticalVelocity;
                //Debug.Log("Not Grounded");
            }
        }

        // Moves the player
        playerController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Calculates movement using player input, using new input system
    /// </summary>
    /// <param name="value"></param>
    private void OnMovement(InputValue value)
    {
        // Set input vector
        input = new Vector3(value.Get<Vector2>().x, 0f, value.Get<Vector2>().y);
    }

    /// <summary>
    /// Digs a hole if the player can dig the current block they are standing on
    /// </summary>
    /// <param name="value"></param>
    private void OnDig(InputValue value)
    {
        // If the player does not have the shovel or is jumping, do not dig
        if (!hasShovel || isJumping)
        {
            return;
        }

        // Shoots a raycast out one unit in front of the player to check for ground to dig
        float heightToFloorOffset = 1.06f;
        Vector3 targetDigPosition = new Vector3 (transform.position.x + transform.forward.x, transform.position.y - heightToFloorOffset, transform.position.z + transform.forward.z);
        RaycastHit hit;
        if (Physics.Linecast(transform.position, targetDigPosition, out hit))
        {
            // Draws a ray for debugging
            Debug.DrawLine(transform.position, targetDigPosition, Color.red);

            if (hit.collider.tag == "Ground" || hit.collider.tag == "GroundTreasure")
            {
                // Stores position of groud object to dig
                Vector3 digPosition = hit.transform.position;

                // Checks if current object has already been dug by checking the current material
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend.sharedMaterial.name != dugMaterial.name)
                {
                    // If the ground block has treasure, dig up the treasure
                    if (hit.collider.tag == "GroundTreasure")
                    {
                        // Checks for GroundTreasure script
                        GroundTreasure groundTreasure;
                        if (groundTreasure = hit.collider.GetComponent<GroundTreasure>())
                        {
                            // TODO: Change this to Physics.Linecast to shoot a ray towards a specific positon if the current implementation is not robust enough
                            // Calculates the target position to move the treasure chest to
                            // If there is an object in front of the player, move the treasure chest to the right of the player
                            // If there is an object to the right of the player, move the treasure chest to behind the player
                            Vector3 treasurePosition = hit.collider.transform.position;
                            treasurePosition.y += 1;
                            Vector3 targetTreasurePosition = CalculateGridPositionInFrontOfPlayer(treasurePosition, 2);
                            if (Physics.Raycast(treasurePosition, transform.forward, out hit, 2.0f))
                            {
                                Debug.Log("Calculating treasure position to right of player");
                                targetTreasurePosition = CalculateGridPositionToRightOfPlayer(treasurePosition, 2);
                                if (Physics.Raycast(treasurePosition, transform.right, out hit, 2.0f))
                                {
                                    Debug.Log("Calculating treasure position behind player");
                                    targetTreasurePosition = CalculateGridPositionBehindPlayer(treasurePosition, 2);
                                }
                            }
                            // Calls the DigUpTreasure method on the ground treasure object
                            groundTreasure.DigUpTreasure(targetTreasurePosition);
                        }
                        else
                        {
                            Debug.LogError("There is no GroundTreasure script on ground trasure object");
                        }
                    }

                    // Start dig action coroutine, passes in position and renderer component of ground object to dig
                    StartCoroutine(DigAction(digPosition, rend));
                }
            }
        }
    }

    /// <summary>
    /// Restricts player input for the duration, centers player on current ground cell and plays the player's dig animation
    /// </summary>
    /// <param name="digPosition">Reference to position of ground object player is digging on</param>
    /// <param name="rend">Reference to Renderer component of ground object player is digging on</param>
    /// <returns></returns>
    private IEnumerator DigAction(Vector3 digPosition, Renderer rend)
    {
        // Disable player input
        playerInput.actions.Disable();

        // Shoot a raycast under the player to find the ground block they are standing on
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.tag == "Ground" || hit.collider.tag == "GroundTreasure")
            {
                // Move the player towards the target ground location (center of the current block they are standing on)
                Vector3 targetGroundLocation = hit.transform.position;
                targetGroundLocation.y = transform.position.y;
                Vector3 lookVector;
                while (Vector3.Distance(transform.position, targetGroundLocation) > 0.05f)
                {
                    // Move player towards center of ground block they are standing on
                    Vector3 tempVelocity = Vector3.MoveTowards(transform.position, targetGroundLocation, 2f * Time.deltaTime);

                    // Set rotation
                    lookVector = tempVelocity - transform.position;
                    transform.rotation = Quaternion.LookRotation(lookVector);
                    transform.position = tempVelocity;
                    yield return null;
                }

                // Once the player is close enough to the position, snap it to the position and set rotation
                transform.position = targetGroundLocation;
                lookVector = digPosition - transform.position;
                lookVector.y = 0;
                transform.rotation = Quaternion.LookRotation(lookVector);

                // Set current material to the dug material;
                rend.sharedMaterial = dugMaterial;

                // TODO: Start dig animation when animation is imported and implemented

                // Wait for dig animation to finish, currently has a placeholder for time
                yield return new WaitForSeconds(2f);

                // Enable player input
                playerInput.actions.Enable();
            }
            else
            {
                Debug.LogWarning("No ground under player");
            }
        }
        else
        {
            Debug.LogWarning("No ground under player");
        }
    }

    /// <summary>
    /// Shove/Push an object one unit in the direction the player is facing
    /// There is some extra logic and math involved due to relative movement
    /// </summary>
    /// <param name="value"></param>
    private void OnShove(InputValue value)
    {
        // If the player does not have the shovel or if the player is jumping, do not shove
        if (!hasShovel || isJumping)
        {
            return;
        }

        // Perform a small/short raycast in front of the player first
        RaycastHit hit;
        Vector3 transformPositionHeightOffset = new Vector3(transform.position.x, transform.position.y - heightOffset, transform.position.z);
        if (Physics.Raycast(transformPositionHeightOffset, transform.forward, out hit, 0.99f))
        {
            // Draws a ray for debugging
            Debug.DrawRay(transformPositionHeightOffset, transform.forward * hit.distance, Color.yellow);

            // If the collider is shovable, initialize variables used for shoving
            if (hit.collider.gameObject.tag == "Shovable")
            {
                ShovableObject shovableObject;
                if (shovableObject = hit.collider.GetComponent<ShovableObject>())
                {
                    if (shovableObject.beingShoved)
                    {
                        return;
                    }

                    // Calculate target position on grid
                    Vector3 targetPosition = CalculateGridPositionInFrontOfPlayer(hit.collider.transform.position, 1);

                    // Call the shove method on the shovable object and start the ShoveAction coroutine
                    shovableObject.Shove(transform.forward, targetPosition);
                    StartCoroutine("ShoveAction");
                }
            }
            // If the collider is a projectile, reflect the projectile
            else if (hit.collider.gameObject.tag == "Projectile")
            {
                ReflectProjectile(hit.collider.gameObject);
            }
            // If the collider is an enemy, stun the enemy
            else if (hit.collider.gameObject.tag == "Enemy")
            {
                EnemyBehavior enemyBehavior = hit.collider.GetComponent<EnemyBehavior>();
                if (!enemyBehavior.isStunned)
                {
                    enemyBehavior.StartCoroutine("StunEnemy");
                }
            }
        }
        // Perform a second larger raycast if the first one did not hit anything, this is mainly meant to detect projectiles
        else if (Physics.SphereCast(transformPositionHeightOffset, 0.5f, transform.forward, out hit, 2.5f))
        {
            // If the collider hit was a projectile, reflect the projectile
            if (hit.collider.gameObject.tag == "Projectile")
            {
                ReflectProjectile(hit.collider.gameObject);
            }
        }
    }

    /// <summary>
    /// Reflects the projectile in the direction the player is facing
    /// </summary>
    /// <param name="projectile"></param>
    private void ReflectProjectile(GameObject projectile)
    {
        EnemyCannonball enemyCannonball;
        if (enemyCannonball = projectile.GetComponent<EnemyCannonball>())
        {
            // Calculate the direction/velocity on the grid to reflect the projectile
            Vector3 targetVelocity = CalculateGridPositionInFrontOfPlayer(Vector3.zero, 1);
            enemyCannonball.SetVelocity(targetVelocity);
            enemyCannonball.SetProjetileSpeed(projectileReflectionSpeed);
            StartCoroutine("ShoveAction");
        }
    }

    /// <summary>
    /// Restricts player input while the player's shove animation is playing
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShoveAction()
    {
        // Disable player input
        playerInput.actions.Disable();

        // TODO: Start shove animation when animation is imported and implemented

        // Wait for shove animation to finish, currently has a placeholder for time
        yield return new WaitForSeconds(1f);

        // Enable player input
        playerInput.actions.Enable();
    }

    /// <summary>
    /// Interacts with an interactable object if possible
    /// </summary>
    /// <param name="value"></param>
    private void OnInteract(InputValue value)
    {
        // If there is a current interactable object stored
        if (currentInteractable && !isJumping)
        {
            // And the interactable object has not been interacted with already
            if (!currentInteractable.GetHasBeenInteracted())
            {
                // Call the interactable object's interaction method
                currentInteractable.DoInteraction();
            }
            // Otherwise the interactable object has already been interacted with
            else
            {
                Debug.Log("Interactable object has already been interacted with");
            }
        }
        // Otherwise there is no interactable object in range
        else
        {
            Debug.Log("No interactable object in range");
        }
    }

    /// <summary>
    /// Executes the player's active special ability if possible
    /// </summary>
    /// <param name="value"></param>
    private void OnSpecialAbility(InputValue value)
    {
        // TODO: Add some way to check what the current active special ability is then execute that specific special ability logic

        // If the player does not have the pogo stick, do not execute the special ability logic
        if (!hasPogoStick)
        {
            return;
        }

        // Jump special ability logic
        // Will only run if the player is grounded and not currently jumping
        if (playerController.isGrounded && !isJumping)
        {
            // Reset successful jump to false
            successfulJump = false;

            // Shoot a raycast one unit in front of the player
            RaycastHit hit;
            Vector3 transformPositionHeightOffset = new Vector3(transform.position.x, transform.position.y - heightOffset, transform.position.z);
            if (Physics.Raycast(transformPositionHeightOffset, transform.forward, out hit, 1f))
            {
                // If the hit object is a valid block to jump on, set successful jump to true
                if (hit.collider.gameObject.tag == "Ground" || hit.collider.gameObject.tag == "Shovable")
                {
                    successfulJump = true;
                }
            }
            // Start jump action
            Debug.Log("Starting Jump");
            StartCoroutine("JumpAction");
        }
    }

    /// <summary>
    /// Simulates a jump
    /// </summary>
    /// <returns></returns>
    private IEnumerator JumpAction()
    {
        // Hide shovel, show pogo stick
        shovel.SetActive(false);
        pogoStick.SetActive(true);

        // Calculate the vertical velocity needed to reach target jump height
        float jumpHeight = 1.5f;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        
        // Set jump booleans
        startJump = true;
        isJumping = true;

        // Loop until the player is done jumping
        while (isJumping)
        {
            yield return null;
        }
        Debug.Log("Done Jump");

        // Hide pogo stick, show shovel
        shovel.SetActive(true);
        pogoStick.SetActive(false);
    }

    /// <summary>
    /// Resets the player back to start
    /// </summary>
    /// <param name="value"></param>
    private void OnReset(InputValue value)
    {
        reset = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the object collided is an interactable object, set it as the current interactable object since we are in range
        if (other.gameObject.tag == "Interactable")
        {
            // Checks if there is an InteractableObject script
            InteractableObject interactable = other.GetComponent<InteractableObject>();
            if (interactable)
            {
                currentInteractable = interactable;
                Debug.Log("Interactable object stored");
            }
            else
            {
                Debug.LogError("Interactable object is missing an InteractableObject script");
            }
        }
        // If the player collides with a shovel, pick up the shovel
        else if (other.gameObject.tag == "Shovel")
        {
            other.gameObject.SetActive(false);
            shovel.SetActive(true);
            hasShovel = true;
        }
        // If the player collides with a pogo stick, pick up the pogo stick
        else if (other.gameObject.tag == "PogoStick")
        {
            other.gameObject.SetActive(false);
            hasPogoStick = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the object collided is an interactable object, clear the current interactable object since we are out of range
        if (other.gameObject.tag == "Interactable")
        {
            // Checks if there is an InteractableObject script
            InteractableObject interactable = other.GetComponent<InteractableObject>();
            if (interactable)
            {
                if (currentInteractable == interactable)
                {
                    currentInteractable = null;
                    Debug.Log("Interactable object removed");
                }
            }
            else
            {
                Debug.LogError("Interactable object is missing an InteractableObject script");
            }
        }
    }

    /// <summary>
    /// Calculates the position on the grid in front of the player
    /// This method is used to move objects in front of the player
    /// </summary>
    /// <param name="objectPosition">The current position of the object to move</param>
    /// <param name="unitsToMove">The number of units to offset the position by</param>
    /// <returns></returns>
    private Vector3 CalculateGridPositionInFrontOfPlayer(Vector3 objectPosition, int unitsToMove)
    {
        Vector3 playerForward = transform.forward;

        // Checks if the player is moving more in the x or z direction
        if (Mathf.Abs(playerForward.x) > Mathf.Abs(playerForward.z))
        {
            // If the player is moving more in the x direction, check if positive or negative
            // and update target position accordingly
            if (playerForward.x > 0)
            {
                objectPosition.x += unitsToMove;
            }
            else
            {
                objectPosition.x -= unitsToMove;
            }
        }
        // If the player is moving more in the z direction, check if positive or negative
        // and update target position accordingly
        else
        {
            if (playerForward.z > 0)
            {
                objectPosition.z += unitsToMove;
            }
            else
            {
                objectPosition.z -= unitsToMove;
            }
        }
        return objectPosition;
    }

    /// <summary>
    /// Calculates the position on the grid to the right of the player
    /// This method is used to move objects to the right of the player
    /// </summary>
    /// <param name="objectPosition">The current position of the object to move</param>
    /// <param name="unitsToMove">The number of units to offset the position by</param>
    /// <returns></returns>
    private Vector3 CalculateGridPositionToRightOfPlayer(Vector3 objectPosition, int unitsToMove)
    {
        Vector3 playerRight = transform.right;

        // Checks if the player is moving more in the x or z direction
        if (Mathf.Abs(playerRight.x) > Mathf.Abs(playerRight.z))
        {
            // If the player is moving more in the x direction, check if positive or negative
            // and update target position accordingly
            if (playerRight.x > 0)
            {
                objectPosition.x += unitsToMove;
            }
            else
            {
                objectPosition.x -= unitsToMove;
            }
        }
        // If the player is moving more in the z direction, check if positive or negative
        // and update target position accordingly
        else
        {
            if (playerRight.z > 0)
            {
                objectPosition.z += unitsToMove;
            }
            else
            {
                objectPosition.z -= unitsToMove;
            }
        }
        return objectPosition;
    }

    /// <summary>
    /// Calculates the position on the grid behind the player
    /// This method is used to move objects behind the player
    /// </summary>
    /// <param name="objectPosition">The current position of the object to move</param>
    /// <param name="unitsToMove">The number of units to offset the position by</param>
    /// <returns></returns>
    private Vector3 CalculateGridPositionBehindPlayer(Vector3 objectPosition, int unitsToMove)
    {
        Vector3 playerforward = transform.forward;

        // Checks if the player is moving more in the x or z direction
        if (Mathf.Abs(playerforward.x) > Mathf.Abs(playerforward.z))
        {
            // If the player is moving more in the x direction, check if positive or negative
            // and update target position accordingly
            if (playerforward.x > 0)
            {
                objectPosition.x -= unitsToMove;
            }
            else
            {
                objectPosition.x += unitsToMove;
            }
        }
        // If the player is moving more in the z direction, check if positive or negative
        // and update target position accordingly
        else
        {
            if (playerforward.z > 0)
            {
                objectPosition.z -= unitsToMove;
            }
            else
            {
                objectPosition.z += unitsToMove;
            }
        }
        return objectPosition;
    }
}
