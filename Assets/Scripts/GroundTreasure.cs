using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTreasure : MonoBehaviour
{
    [SerializeField]
    private GameObject interactableTreasure;

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
        interactableTreasure.SetActive(true);
        interactableTreasure.transform.position = treasurePosition;
        Debug.Log("Dug up treasure!");
    }
}
