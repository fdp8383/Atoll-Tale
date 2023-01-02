using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private bool hasBeenInteracted = false;

    [SerializeField]
    private Material interactedMaterial;

    [SerializeField]
    private Renderer rend;

    [SerializeField]
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to renderer component
        if (!rend)
        {
            rend = GetComponent<Renderer>();
        }

        // Get reference to game manager script
        if (!gameManager)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Checks if this object has been interacted with already
    /// </summary>
    /// <returns></returns>
    public bool GetHasBeenInteracted()
    {
        return hasBeenInteracted;
    }

    /// <summary>
    /// Executes the interaction logic
    /// </summary>
    public void DoInteraction()
    {
        // If this interactable object is a treasure chest, add to the player's gold and hide the object
        if (gameObject.name == "InteractableTreasureChest")
        {
            gameManager.AddToPlayerGold(1);
            gameManager.AddToPlayerChestsFound();
            gameObject.SetActive(false);
        }
        else if (gameObject.name == "InteractableTreasureChestFinal")
        {
            gameManager.AddToPlayerGold(1);
            gameManager.AddToPlayerChestsFound();
            gameManager.WinLevel();
            gameObject.SetActive(false);
        }
        // Otherwise change the material to the interacted material placeholder
        else
        {
            // Placeholder, sets material to interacted material placeholder
            rend.sharedMaterial = interactedMaterial;
        }

        // Set hasBeenInteracted to true
        hasBeenInteracted = true;
    }
}
