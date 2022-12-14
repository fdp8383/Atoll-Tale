using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private Transform mainCamera;

    [SerializeField]
    private GameObject dollyCamera;

    [SerializeField]
    private CharacterController playerController;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private Animator playerAnimator;

    public Vector3 spawnPoint;

    [SerializeField]
    private GameObject shovel;

    [SerializeField]
    private GameObject pogoStick;

    private bool hasShovel = false;

    private bool hasPogoStick = false;

    [SerializeField]
    private Material dugMaterial;

    [SerializeField]
    private Slider swingChargeBar;
    [SerializeField]
    private Transform swingChargeBarCanvasTransform;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float gravity = -9.81f;

    [SerializeField]
    private float projectileReflectionSpeed = 6.0f;

    private Vector3 input;

    private Vector3 velocity;

    private float verticalVelocity;

    [SerializeField]
    private bool isInDigPreview = false;
    [SerializeField]
    private GameObject digTilePreview;
    private GameObject currentDigTile;
    private Vector3 digPosition;

    private bool startJump = false;
    private bool isJumping = false;
    private bool successfulJump = false;

    [SerializeField]
    private float heightOffset = 0.49f;

    private InteractableObject currentInteractable;
    public EnemyCannonball currentProjectile;

    [SerializeField]
    private Renderer rend;

    [SerializeField]
    private Material playerMaterial;

    [SerializeField]
    private Material playerDamagedMaterial;

    [SerializeField]
    private Material playerInvincibleMaterial;

    //private bool doneCenteringOnGround = false;

    [SerializeField]
    private float invulnerabilityDuration;
    private float invulnerabilityDurationTimer;
    public bool isInvulnerable = false;

    public bool godMode = false;

    [SerializeField]
    private float swingChargeTime = 0f;
    [SerializeField]
    private bool isChargingSwing = false;
    [SerializeField]
    private Vector3 swingChargeBarOffset = Vector3.zero;

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

        if (!digTilePreview)
        {
            digTilePreview = gameManager.transform.GetChild(0).gameObject;
            if (digTilePreview.name != "DigTilePreview")
            {
                Debug.LogWarning("DigTilePreview may not be set correctly", this.gameObject);
            }
        }

        // Set the player spawn point
        spawnPoint = transform.position;

        swingChargeBar.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (gameManager.isGamePaused)
        {
            return;
        }

        UpdateMovement();

        invulnerabilityDurationTimer -= Time.deltaTime;
        if (invulnerabilityDurationTimer <= 0)
        {
            isInvulnerable = false;
            rend.sharedMaterial = playerMaterial;
        }

        if (isChargingSwing)
        {
            swingChargeTime += Time.deltaTime;
            swingChargeBar.value = swingChargeTime;

            if (swingChargeTime >= 2.5f)
            {
                playerAnimator.SetBool("isChargingSwingFull", true);
                playerAnimator.SetBool("isChargingSwing", false);
            }
        }

        if (isInDigPreview)
        {
            UpdateDigPreview();
        }
    }

    private void LateUpdate()
    {
        swingChargeBarCanvasTransform.LookAt(swingChargeBarCanvasTransform.position + Camera.main.transform.forward);
        //Vector3 lookAtVector = Camera.main.transform.position; //swingChargeBarCanvasTransform.position + Camera.main.transform.forward;
        //lookAtVector.y = 0f;
        //swingChargeBarCanvasTransform.LookAt(lookAtVector);
    }

    /// <summary>
    /// Updates player movement relative to the camera
    /// </summary>
    private void UpdateMovement()
    {
        // Calculates velocity based on the camera's current rotation and input vector
        velocity = mainCamera.rotation * input * speed;
        //velocity = input * speed;
        velocity.y = 0.0f;

        // Rotates player to face the velocity/move direction
        if (input.magnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
        else
        {
            playerAnimator.SetBool("isWalking", false);
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

        if (!isChargingSwing)
        {
            // Moves the player
            playerController.Move(velocity * Time.deltaTime);
        }
    }

    /// <summary>
    /// Calculates movement using player input, using new input system
    /// </summary>
    /// <param name="value"></param>
    private void OnMovement(InputValue value)
    {
        // Set input vector
        input = new Vector3(value.Get<Vector2>().x, 0f, value.Get<Vector2>().y);
        if (isChargingSwing)
        {
            playerAnimator.SetBool("isWalking", false);
        }
        else
        {
            playerAnimator.SetBool("isWalking", true);
        }
    }

    /// <summary>
    /// Shove/Push an object one unit in the direction the player is facing
    /// There is some extra logic and math involved due to relative movement
    /// </summary>
    /// <param name="value"></param>
    private void OnSwing(InputValue value)
    {
        // If the player does not have the shovel or if the player is jumping, do not shove
        if (!hasShovel || isJumping || isInDigPreview)
        {
            return;
        }

        if (value.isPressed)
        {
            Debug.Log("Starting swing charge");
            isChargingSwing = true;
            playerAnimator.SetBool("isChargingSwing", true);
            swingChargeBar.gameObject.SetActive(true);
            swingChargeBar.value = 0.0f;
        }
        else
        {
            Debug.Log("Stopping swing charge. Total charge time was: " + swingChargeTime);
            if (swingChargeTime > 0.5f)
            {
                swingChargeTime = Mathf.Clamp(swingChargeTime, 0.51f, 2.49f);
                swingChargeTime *= 2;
                Debug.Log("Clamped swing charge time: " + swingChargeTime);
                // Perform a small/short raycast in front of the player first
                RaycastHit hit;
                Vector3 transformPositionHeightOffset = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                if (Physics.Raycast(transform.position, transform.forward, out hit, 1.49f))
                {
                    // Draws a ray for debugging
                    Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.yellow);

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

                            // Update gameManager's list of shovable objects moved since last checkpoint
                            gameManager.UpdateShovableObjectsMovedList(shovableObject);

                            // Calculate target position on grid
                            Vector3 targetPosition = CalculateGridPositionInFrontOfPlayer(hit.collider.transform.position, 5);
                            float distance = 5;
                            if (Physics.Linecast(new Vector3(hit.transform.position.x, hit.transform.position.y + 0f, hit.transform.position.z), targetPosition, out hit, 3))
                            {
                                distance = Mathf.Floor(Vector3.Distance(shovableObject.transform.position, hit.collider.transform.position));
                                distance -= 1;
                                //Debug.Log("Found charged shove target location: " + targetPosition);
                            }
                            distance = Mathf.Clamp(distance, 1.0f, Mathf.Ceil(swingChargeTime));
                            Debug.Log("Distance to travel: " + distance);
                            targetPosition = CalculateGridPositionInFrontOfPlayer(shovableObject.transform.position, distance);

                            // Call the shove method on the shovable object and start the ShoveAction coroutine
                            shovableObject.Shove(transform.forward, targetPosition, distance, Mathf.Clamp(distance / 10.0f, 0.2f, 0.5f), 0.05f * distance, true);
                            StartCoroutine("ShoveAction");
                        }
                    }
                    // If the collider is an enemy, stun the enemy
                    else if (hit.collider.gameObject.tag == "Enemy")
                    {
                        EnemyBehavior enemyBehavior = hit.collider.GetComponent<EnemyBehavior>();
                        if (!enemyBehavior.isStunned)
                        {
                            enemyBehavior.StartCoroutine("StunEnemy");
                            SoundManager.PlaySound(SoundManager.Sound.clang);
                        }
                    }
                }
                // If nothing was hit by the raycast, check if a projectile is in the player's reflection hitbox. If there is a projectile, reflect
                else if (currentProjectile != null)
                {
                    ReflectProjectile(currentProjectile, projectileReflectionSpeed + (swingChargeTime * 2));
                }
            }
            else
            {
                // Perform a small/short raycast in front of the player first
                RaycastHit hit;
                Vector3 transformPositionHeightOffset = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                if (Physics.Raycast(transform.position, transform.forward, out hit, 0.99f))
                {
                    Debug.Log("Hit something on swing", hit.collider.gameObject);
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

                            Debug.Log("Sending shovable object to add to list: " + shovableObject);
                            // Update gameManager's list of shovable objects moved since last checkpoint
                            gameManager.UpdateShovableObjectsMovedList(shovableObject);

                            // Calculate target position on grid
                            Vector3 targetPosition = CalculateGridPositionInFrontOfPlayer(hit.collider.transform.position, 1);

                            // Call the shove method on the shovable object and start the ShoveAction coroutine
                            shovableObject.Shove(transform.forward, targetPosition, 1f, 0.2f, 0.05f, false);
                            StartCoroutine("ShoveAction");
                        }
                    }
                    // If the collider is an enemy, stun the enemy
                    else if (hit.collider.gameObject.tag == "Enemy")
                    {
                        EnemyBehavior enemyBehavior = hit.collider.GetComponent<EnemyBehavior>();
                        if (!enemyBehavior.isStunned)
                        {
                            enemyBehavior.StartCoroutine("StunEnemy");
                            SoundManager.PlaySound(SoundManager.Sound.clang);
                        }
                    }
                }
                // If nothing was hit by the raycast, check if a projectile is in the player's reflection hitbox. If there is a projectile, reflect
                else if (currentProjectile != null)
                {
                    ReflectProjectile(currentProjectile, projectileReflectionSpeed);
                }
            }

            StartCoroutine("SwingAction");
            isChargingSwing = false;
            swingChargeTime = 0f;
            swingChargeBar.value = 0f;
            swingChargeBar.gameObject.SetActive(false);
        }
    }

    private IEnumerator SwingAction()
    {
        playerInput.actions.Disable();
        playerAnimator.SetBool("isSwinging", true);
        playerAnimator.SetBool("isChargingSwing", false);
        playerAnimator.SetBool("isChargingSwingFull", false);

        yield return new WaitForSeconds(0.5f);

        playerAnimator.SetBool("isSwinging", false);
        playerInput.actions.Enable();
    }

    /// <summary>
    /// Reflects the projectile in the direction the player is facing
    /// </summary>
    /// <param name="projectile"></param>
    private void ReflectProjectile(EnemyCannonball projectile, float speed)
    {
        // Calculate the direction/velocity on the grid to reflect the projectile
        Vector3 targetVelocity = CalculateGridPositionInFrontOfPlayer(Vector3.zero, 1);
        projectile.SetVelocity(targetVelocity);
        projectile.SetProjetileSpeed(speed);
        StartCoroutine("ShoveAction");
    }

    /// <summary>
    /// Restricts player input while the player's shove animation is playing
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShoveAction()
    {
        // Disable player input
        playerInput.actions.Disable();

        SoundManager.PlaySound(SoundManager.Sound.clang);

        // Wait for shove animation to finish, currently has a placeholder for time
        yield return new WaitForSeconds(0.5f);

        // Enable player input
        playerInput.actions.Enable();
    }

    /// <summary>
    /// Interacts with an interactable object if possible
    /// </summary>
    /// <param name="value"></param>
    private void OnInteract(InputValue value)
    {
        if (isChargingSwing || isJumping || isInDigPreview)
        {
            return;
        }

        // If there is a current interactable object stored
        if (currentInteractable)
        {
            // And the interactable object has not been interacted with already
            if (!currentInteractable.GetHasBeenInteracted())
            {
                SoundManager.PlaySound(SoundManager.Sound.startGame);
                // Call the interactable object's interaction method
                currentInteractable.DoInteraction();
                currentInteractable = null;
            }
            // Otherwise the interactable object has already been interacted with
            else
            {
                Debug.Log("Interactable object has already been interacted with");
            }
        }
    }

    /// <summary>
    /// Digs on tile if possible
    /// </summary>
    /// <param name="value"></param>
    private void OnDig(InputValue value)
    {
        if (isChargingSwing || isJumping)
        {
            return;
        }

        if (value.isPressed)
        {
            // If the player does not have the shovel, do not dig
            if (hasShovel)
            {
                isInDigPreview = true;
            }
            else
            {
                isInDigPreview = false;
            }
        }
        else
        {
            if (currentDigTile)
            {
                // Checks if current object has already been dug by checking if the dug spot of the hit ground tile is active
                GameObject dugSpot = currentDigTile.transform.GetChild(0).gameObject;
                if (!dugSpot.activeInHierarchy)
                {
                    Debug.Log("Trying to dig on block", currentDigTile);
                    // Start dig action coroutine, passes in position and renderer component of ground object to dig
                    StartCoroutine(DigAction(digPosition, currentDigTile, dugSpot));
                }
            }
            isInDigPreview = false;
            digTilePreview.SetActive(false);
            currentDigTile = null;
            digPosition = Vector3.zero;
        }
    }

    private void UpdateDigPreview()
    {
        // Shoots a raycast out one unit in front of the player to check for ground to dig
        float heightToFloorOffset = 0.32f;
        Vector3 targetDigPosition = new Vector3(transform.position.x + (transform.forward.x * 1.5f), transform.position.y - heightToFloorOffset, transform.position.z + (transform.forward.z * 1.5f));
        RaycastHit hit;
        if (Physics.Linecast(transform.position, targetDigPosition, out hit))
        {
            // Draws a ray for debugging
            Debug.DrawLine(transform.position, targetDigPosition, Color.red);

            RaycastHit groundCheck;
            if (Physics.Raycast(hit.transform.position, Vector3.up, out groundCheck, 1.0f))
            {
                if (groundCheck.collider.tag != "Player")
                {
                    Debug.Log("Cannot dig at tile hit, there is a block on top of it", groundCheck.collider.gameObject);
                    digPosition = Vector3.zero;
                    digTilePreview.SetActive(false);
                    currentDigTile = null;
                    return;
                }
            }

            if (transform.position.y - hit.transform.position.y < 0.64f)
            {
                Debug.Log("Cannot dig at tile hit, it is above the current level the player is on");
                digPosition = Vector3.zero;
                digTilePreview.SetActive(false);
                currentDigTile = null;
                return;
            }

            if (Physics.Raycast(transform.position, Vector3.down, out groundCheck, 1.5f))
            {
                if (groundCheck.collider.tag == "GroundRamp")
                {
                    Debug.Log("Standing on ramp, cannot dig");
                    digPosition = Vector3.zero;
                    digTilePreview.SetActive(false);
                    currentDigTile = null;
                    return;
                }
            }

            if (hit.collider.tag == "Ground" || hit.collider.tag == "GroundTreasure")
            {
                // Stores position and game object of ground object to dig
                digPosition = hit.transform.position;
                currentDigTile = hit.collider.gameObject;
                digTilePreview.SetActive(true);
                digTilePreview.transform.position = new Vector3(digPosition.x, digPosition.y + 0.55f, digPosition.z);
            }
        }
    }

    /// <summary>
    /// Restricts player input for the duration, centers player on current ground cell and plays the player's dig animation
    /// </summary>
    /// <param name="digPosition">Reference to position of ground object player is digging on</param>
    /// <param name="rend">Reference to Renderer component of ground object player is digging on</param>
    /// <returns></returns>
    private IEnumerator DigAction(Vector3 digPosition, GameObject groundTileHit, GameObject dugSpot)
    {
        // Disable player input
        playerInput.actions.Disable();

        // TODO: Remove center on tile checks, no longer needed.

        /*Vector3 targetGroundLocation = CalculateGridPositionToCenterPlayer(digPosition, 1);
        Vector3 transformPositionHeightOffset = new Vector3(transform.position.x, transform.position.y - heightOffset, transform.position.z);
        if (Physics.Raycast(targetGroundLocation, Vector3.up, 1.0f, 6) || Physics.Raycast(transformPositionHeightOffset, transform.forward, 0.99f, 6))
        {
            Debug.LogWarning("Cannot center on target ground location, there is a block on top of it or in front of player");
            // Enable player input
            playerInput.actions.Enable();
            yield return null;
        }
        else
        {
            RaycastHit groundCheck;
            targetGroundLocation.y = transform.position.y;
            Debug.Log("Checking for ground at target location to center at: " + targetGroundLocation);
            if (Physics.Raycast(targetGroundLocation, Vector3.down, out groundCheck, 0.65f))
            {
                Debug.Log("Hit something: ", groundCheck.collider.gameObject);
                if (groundCheck.collider.tag != "Ground")
                {
                    Debug.LogWarning("Cannot center on target ground location, there is no block underneath to center on");
                    // Enable player input
                    playerInput.actions.Enable();
                    yield return null;
                }
                else
                {
                    playerAnimator.SetBool("isDigging", true);
                    playerAnimator.SetBool("isWalking", false);

                    // Wait for dig animation to finish, currently has a placeholder for time
                    yield return new WaitForSeconds(0.5f);

                    // Set dug spot of hit ground tile to active;
                    dugSpot.SetActive(true);

                    // If the ground block has treasure, dig up the treasure
                    if (groundTileHit.tag == "GroundTreasure")
                    {
                        // Checks for GroundTreasure script
                        GroundTreasure groundTreasure;
                        if (groundTreasure = groundTileHit.GetComponent<GroundTreasure>())
                        {
                            // TODO: Change this to Physics.Linecast to shoot a ray towards a specific positon if the current implementation is not robust enough
                            // Calculates the target position to move the treasure chest to
                            // If there is an object in front of the player, move the treasure chest to the right of the player
                            // If there is an object to the right of the player, move the treasure chest to behind the player
                            Vector3 treasurePosition = groundTileHit.transform.position;
                            treasurePosition.y += 1;

                            // Calls the DigUpTreasure method on the ground treasure object
                            groundTreasure.DigUpTreasure(treasurePosition);
                        }
                        else
                        {
                            Debug.LogError("There is no GroundTreasure script on ground trasure object");
                        }
                    }

                    playerAnimator.SetBool("isDigging", false);

                    // Enable player input
                    playerInput.actions.Enable();
                }
            }
            else
            {
                Debug.LogWarning("Cannot center on target ground location, there is no block underneath to center on");
                // Enable player input
                playerInput.actions.Enable();
                yield return null;
            }
        }*/

        playerAnimator.SetBool("isDigging", true);
        playerAnimator.SetBool("isWalking", false);

        SoundManager.PlaySound(SoundManager.Sound.dig);

        // Wait for dig animation to finish, currently has a placeholder for time
        yield return new WaitForSeconds(0.5f);

        // Set dug spot of hit ground tile to active;
        dugSpot.SetActive(true);

        // If the ground block has treasure, dig up the treasure
        if (groundTileHit.tag == "GroundTreasure")
        {
            // Checks for GroundTreasure script
            GroundTreasure groundTreasure;
            if (groundTreasure = groundTileHit.GetComponent<GroundTreasure>())
            {
                // TODO: Change this to Physics.Linecast to shoot a ray towards a specific positon if the current implementation is not robust enough
                // Calculates the target position to move the treasure chest to
                // If there is an object in front of the player, move the treasure chest to the right of the player
                // If there is an object to the right of the player, move the treasure chest to behind the player
                Vector3 treasurePosition = groundTileHit.transform.position;
                treasurePosition.y += 1;

                // Calls the DigUpTreasure method on the ground treasure object
                groundTreasure.DigUpTreasure(treasurePosition);
            }
            else
            {
                Debug.LogError("There is no GroundTreasure script on ground trasure object");
            }
        }

        playerAnimator.SetBool("isDigging", false);

        // Enable player input
        playerInput.actions.Enable();
    }

    /// <summary>
    /// Executes the player's active special ability if possible
    /// </summary>
    /// <param name="value"></param>
    private void OnSpecialAbility(InputValue value)
    {
        // TODO: Add some way to check what the current active special ability is then execute that specific special ability logic

        // If the player does not have the pogo stick or is currently charging a swing, do not execute the special ability logic
        if (!hasPogoStick || isChargingSwing || isInDigPreview)
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
            //Vector3 transformPositionHeightOffset = new Vector3(transform.position.x, transform.position.y - heightOffset, transform.position.z);
            if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
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

        SoundManager.PlaySound(SoundManager.Sound.jump);

        // Calculate the vertical velocity needed to reach target jump height
        float jumpHeight = 1.3f;
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
    /// Toggles god mode. When in god mode the player will not take damage from enemies
    /// </summary>
    public void ToggleGodMode()
    {
        godMode = !godMode;
    }

    /// <summary>
    /// Gives player pogostick
    /// </summary>
    public void GivePogostick()
    {
        hasPogoStick = true;
        gameManager.SetActiveSepcialAbility("Pogostick");
    }

    /// <summary>
    /// Updates and teleports player to next checkpoint
    /// </summary>
    public void TeleportToNextCheckpoint()
    {
        gameManager.TeleportPlayerToNextCheckpoint();
    }

    /// <summary>
    /// Resets player to last checkpoint reached
    /// </summary>
    public void ResetCheckpoint()
    {
        gameManager.StartCoroutine(gameManager.ResetPlayer());
    }

    /// <summary>
    /// Reloads current scene
    /// </summary>
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Stuns the player for a set duration
    /// </summary>
    /// <param name="stunDuration"></param>
    /// <returns></returns>
    public IEnumerator StunPlayer(float stunDuration)
    {
        // Disable player input
        playerInput.actions.Disable();

        rend.sharedMaterial = playerDamagedMaterial;

        // Stun player for passed in stun duration
        yield return new WaitForSeconds(stunDuration);

        rend.sharedMaterial = playerInvincibleMaterial;

        ResetPlayer();
    }

    /// <summary>
    /// Activates player invulnerability duration from taking damage
    /// </summary>
    public void ResetInvulnerabilityDuration()
    {
        invulnerabilityDurationTimer = invulnerabilityDuration;
        isInvulnerable = true;
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
            GivePogostick();
        }
        // If the player collides with a checkpoint, update the player's current checkpoint
        /*else if (other.gameObject.tag == "Checkpoint")
        {
            spawnPoint = other.transform.position;
            spawnPoint.y = transform.position.y;
            gameManager.UpdatePlayerCheckpoint(other.gameObject);
        }*/
        else if (other.gameObject.tag == "Ocean")
        {
            Debug.Log("Hit ocean");
            gameManager.UpdatePlayerHealth(-3);
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
    private Vector3 CalculateGridPositionInFrontOfPlayer(Vector3 objectPosition, float unitsToMove)
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
    private Vector3 CalculateGridPositionToRightOfPlayer(Vector3 objectPosition, float unitsToMove)
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
    private Vector3 CalculateGridPositionBehindPlayer(Vector3 objectPosition, float unitsToMove)
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

    private Vector3 CalculateGridPositionToCenterPlayer(Vector3 objectPosition, float unitsToMove)
    {
        Vector3 directionToPlayer = objectPosition - transform.position;

        // Checks if the player is moving more in the x or z direction
        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.z))
        {
            // If the player is moving more in the x direction, check if positive or negative
            // and update target position accordingly
            if (directionToPlayer.x > 0)
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
            if (directionToPlayer.z > 0)
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

    /// <summary>
    /// Resets player to spawn point (last checkpoint)
    /// </summary>
    public void ResetPlayer()
    {
        StopAllCoroutines();
        rend.sharedMaterial = playerMaterial;
        startJump = isJumping = successfulJump = isInvulnerable = isChargingSwing = isInDigPreview = false;
        velocity = digPosition = Vector3.zero;
        verticalVelocity = invulnerabilityDurationTimer = swingChargeTime = swingChargeBar.value = 0.0f;
        currentInteractable = null;
        swingChargeBar.gameObject.SetActive(false);
        digTilePreview.SetActive(false);
        playerAnimator.SetBool("isSwinging", false);
        playerAnimator.SetBool("isChargingSwing", false);
        playerAnimator.SetBool("isChargingSwingFull", false);
        playerAnimator.SetBool("isDigging", false);
        playerAnimator.SetBool("isWalking", false);
        playerInput.actions.Enable();
    }

    /// <summary>
    /// Disables player input
    /// </summary>
    public void DisablePlayerInput()
    {
        // Disable player input
        playerInput.actions.Disable();
    }

    /// <summary>
    /// Enables player input
    /// </summary>
    public void EnablePlayerInput()
    {
        // Enable player input
        playerInput.actions.Enable();
    }
}
