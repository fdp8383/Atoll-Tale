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
        if (gameObject.name == "InteractableTreasureChest")
        {
            gameManager.AddToPlayerGold(1);
            hasBeenInteracted = true;
            gameObject.SetActive(false);
        }
        else
        {
            // Placeholder, sets material to interacted material placeholder
            rend.sharedMaterial = interactedMaterial;
            hasBeenInteracted = true;
        }
    }
}
