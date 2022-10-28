using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTreasure : MonoBehaviour
{
    [SerializeField]
    private GameObject interactableTreasure;

    [SerializeField]
    private GameObject treasureSpot;

    [SerializeField]
    private Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (interactableTreasure)
        {
            interactableTreasure.SetActive(false);
        }
        else
        {
            Debug.LogError("There is no reference to interactable treasure on" + this.gameObject);
        }

        if (!treasureSpot)
        {
            treasureSpot = transform.GetChild(1).gameObject;
        }

        if (!playerTransform)
        {
            playerTransform = GameObject.Find("Player").GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Digs up the treasure in this ground block and moves it to the passed in target position
    /// </summary>
    /// <param name="treasurePosition"></param>
    public void DigUpTreasure(Vector3 treasurePosition)
    {
        treasureSpot.SetActive(false);
        interactableTreasure.SetActive(true);
        interactableTreasure.transform.position = treasurePosition;
        FacePlayerDirection();
        Debug.Log("Dug up treasure!");
    }

    /// <summary>
    /// Rotates the treasure chest to look in the direction of the player
    /// </summary>
    private void FacePlayerDirection()
    {
        Vector3 directionToPlayer = playerTransform.position - interactableTreasure.transform.position;
        Quaternion rotation;

        // Check if the player is more in the x or z direction from the treasure chest
        // Then check if the player is more in the positive or negative direction
        // and set the rotation accordingly
        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.z))
        {
            if (directionToPlayer.x > 0)
            {
                rotation = Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                rotation = Quaternion.Euler(0f, 270f, 0f);
            }
        }
        else
        {
            if (directionToPlayer.z > 0)
            {
                rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        interactableTreasure.transform.rotation = rotation;
    }
}
