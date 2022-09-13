using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
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
    /// Updates player movement
    /// </summary>
    private void UpdateMovement()
    {
        playerController.Move(velocity * Time.deltaTime * speed);
    }

    /// <summary>
    /// Calculates movement using player input, using new input system
    /// </summary>
    /// <param name="value"></param>
    private void OnMovement(InputValue value)
    {
        velocity = new Vector3(value.Get<Vector2>().x, 0f, value.Get<Vector2>().y);
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
}
