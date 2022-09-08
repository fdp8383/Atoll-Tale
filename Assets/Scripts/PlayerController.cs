using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private CharacterController playerController;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float gravity = 5f;

    private Vector3 velocity;

    // Start is called before the first frame update
    private void Start()
    {
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
    private void OnMovement(InputValue value)
    {
        velocity = new Vector3(value.Get<Vector2>().x, 0f, value.Get<Vector2>().y);
    }
}
