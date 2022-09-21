using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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
    private float gravity = 5f;

    private Vector3 input;

    private Vector3 velocity;

    // Start is called before the first frame update
    private void Start()
    {
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
        velocity = mainCamera.rotation * input;
        velocity.y = 0f;

        // Rotates player to face the velocity/move direction
        if (input.magnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }

        // Moves the player
        playerController.Move(velocity * Time.deltaTime * speed);
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

            // Checks if current object has already been dug by checking the current material
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend.sharedMaterial.name != dugMaterial.name)
            {
                // Set current material to the dug material and start the dig action coroutine
                rend.sharedMaterial = dugMaterial;
                StartCoroutine("DigAction");
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
        Vector3 transformPositionHeightOffset = new Vector3(transform.position.x, transform.position.y - 0.49f, transform.position.z);
        if (Physics.Raycast(transformPositionHeightOffset, transform.forward, out hit, 0.99f))
        {
            // Draws a ray for debugging
            Debug.DrawRay(transformPositionHeightOffset, transform.forward * hit.distance, Color.yellow);

            // If the collider is shovable, initialize variables used for shoving
            if (hit.collider.gameObject.tag == "Shovable")
            {
                ShovableObject shovableObject;
                Vector3 targetPosition = hit.collider.transform.position;
                Vector3 playerForward = transform.forward;
                Debug.Log(playerForward);

                if (shovableObject = hit.collider.GetComponent<ShovableObject>())
                {
                    // Checks if the player is moving more in the x or z direction
                    if (Mathf.Abs(playerForward.x) > Mathf.Abs(playerForward.z))
                    {
                        // If the player is moving more in the x direction, check if positive or negative
                        // and update target position accordingly
                        if (playerForward.x > 0)
                        {
                            targetPosition.x += 1;
                        }
                        else
                        {
                            targetPosition.x -= 1;  
                        }
                    }
                    // If the player is moving more in the z direction, check if positive or negative
                    // and update target position accordingly
                    else
                    {
                        if (playerForward.z > 0)
                        {
                            targetPosition.z+= 1;
                        }
                        else
                        {
                            targetPosition.z -= 1;
                        }
                    }

                    // Call the shove method on the shovable object and start the ShoveAction coroutine
                    shovableObject.Shove(transform.forward, targetPosition);
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
}
