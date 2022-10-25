using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int playerGold;

    public int playerHealth;

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

    /// <summary>
    /// Updates player health
    /// </summary>
    /// <param name="healthToAdd"></param>
    public void UpdatePlayerHealth(int healthToAdd)
    {
        playerHealth += healthToAdd;

        if (playerHealth <= 0)
        {
            RestartLevelAtCheckpoint();
        }
    }

    private void RestartLevelAtCheckpoint()
    {

    }
}
