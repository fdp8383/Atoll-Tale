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

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to renderer component
        if (!rend)
        {
            rend = GetComponent<Renderer>();
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
        // Placeholder, sets material to interacted material placeholder
        rend.sharedMaterial = interactedMaterial;
        hasBeenInteracted = true;
    }
}
