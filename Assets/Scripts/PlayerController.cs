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
    private Material dugMaterial;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float gravity = -9.81f;

    private Vector3 input;

    private Vector3 velocity;

    private float verticalVelocity;

    private bool startJump = false;
    private bool isJumping = false;
    private bool successfulJump = false;

    [SerializeField]
    private float heightOffset = 0.49f;

    private InteractableObject currentInteractable;

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
        // Shoots a raycast out directly under the player to check for ground to dig
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            // Draws a ray for debugging
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.red);

            if (hit.collider.tag == "Ground" || hit.collider.tag == "GroundTreasure")
            {
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
                            // Calculates the target position to move the treasure chest to and calls the DigUpTreasure method
                            // on the ground treasure object
                            Vector3 treasurePosition = hit.collider.transform.position;
                            treasurePosition.y += 1;
                            treasurePosition = CalculateGridPositionInFrontOfPlayer(treasurePosition, 2);
                            groundTreasure.DigUpTreasure(treasurePosition);

                            // TODO: Add gold when interacting with treasure chest
                            // Adding gold here for now, make seperate interactable treasure chest object and add gold that way in future
                            gameManager.AddToPlayerGold(1);
                        }
                        else
                        {
                            Debug.LogError("There is no GroundTreasure script on ground trasure object");
                        }
                    }

                    // Set current material to the dug material and start the dig action coroutine
                    rend.sharedMaterial = dugMaterial;
                    StartCoroutine("DigAction");
                }
            }
        }
    }

    /// <summary>
    /// Restricts player input while the player's dig animation is playing
    /// </summary>
    /// <returns></returns>
    private IEnumerator DigAction()
    {
        // Disable player input
        playerInput.actions.Disable();

        // TODO: Start dig animation when animation is imported and implemented

        // Wait for dig animation to finish, currently has a placeholder for time
        yield return new WaitForSeconds(2f);

        // Enable player input
        playerInput.actions.Enable();
    }

    /// <summary>
    /// Shove/Push an object one unit in the direction the player is facing
    /// There is some extra logic and math involved due to relative movement
    /// </summary>
    /// <param name="value"></param>
    private void OnShove(InputValue value)
    {
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
                //Debug.Log(playerForward);

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
                    Debug.Log(targetPosition);
                    StartCoroutine("ShoveAction");
                }
            }
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
        if (currentInteractable)
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

        // Jump special ability logic
        // Will only run if the player is grounded
        if (playerController.isGrounded)
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
        // Disable player input
        playerInput.actions.Disable();

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

        // Enable player input
        playerInput.actions.Enable();
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
}
