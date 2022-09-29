using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int playerGold;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    /// <summary>
    /// Adds to the player's gold count
    /// </summary>
    /// <param name="gold"></param>
    public void AddToPlayerGold(int goldToAdd)
    {
        playerGold += goldToAdd;
    }
}
