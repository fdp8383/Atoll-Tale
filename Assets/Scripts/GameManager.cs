using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public int playerGold;

    public int chestsFound = 0;

    public int playerDeaths = 0;

    public int playerHealth;

    public bool isGamePaused = false;

    private bool isLevelWon = false;

    [SerializeField]
    private PlayerInput gameManagerInput;

    [SerializeField]
    private CinemachineInputProvider cameraInput;

    [SerializeField]
    private GameObject devToolsMenu;

    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private GameObject winMenu;

    [SerializeField]
    private TextMeshProUGUI winMenuText; 

    [SerializeField]
    private TextMeshProUGUI cointText;

    [SerializeField]
    private List<GameObject> fullHearts;

    [SerializeField]
    private GameObject pogostickIcon;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private List<GameObject> foundCheckpointPositions;

    [SerializeField]
    private List<GameObject> allCheckpointPositions;

    [SerializeField]
    private Vector3 currentCheckpointPosition;

    [SerializeField]
    private List<ShovableObject> shovableObjectsMovedSinceLastCheckpoint;

    // Start is called before the first frame update
    private void Start()
    {
        Time.timeScale = 1.0f;
        isLevelWon = false;
        chestsFound = 0;
        playerDeaths = 0;

        // Get reference to player controller script if not set in editor
        if (!playerController)
        {
            playerController = GameObject.Find("Player").GetComponentInChildren<PlayerController>();
        }

        currentCheckpointPosition = playerController.transform.position;

        // Gets reference to dev tools menu
        if (!devToolsMenu)
        {
            devToolsMenu = GameObject.Find("DevToolsMenu");
        }

        // Gets reference to dev tools menu
        if (!pauseMenu)
        {
            pauseMenu = GameObject.Find("PauseMenu");
        }

        if (!winMenu)
        {
            winMenu = GameObject.Find("WinMenu");
        }

        if (!cameraInput)
        {
            cameraInput = GameObject.Find("CMFreeLookCam").GetComponent<CinemachineInputProvider>();
        }

        // Lock mouse cursor to screen and hide the mouse cursor
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
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
        cointText.text = "x " + playerGold;
    }

    /// <summary>
    /// Adds to the player's chest found count
    /// </summary>
    public void AddToPlayerChestsFound()
    {
        chestsFound += 1;
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
            playerHealth = 0;
            for (int i = 0; i < fullHearts.Count; i++)
            {
                fullHearts[i].SetActive(false);
            }
            RestartLevelToLastCheckpoint();
        }
        else
        {
            fullHearts[playerHealth].SetActive(false);
        }
    }

    /// <summary>
    /// Updates the player's checkpoint spawn and clears the list of shovable objects moved since the last checkpoint
    /// </summary>
    /// <param name="spawnPoint"></param>
    public void UpdatePlayerCheckpoint(GameObject checkpoint)
    {
        if (checkpoint != null)
        {
            // Checks to see if spawn point passed in has been reached by the player already
            if (!foundCheckpointPositions.Contains(checkpoint))
            {
                Debug.Log("Updating checkpoint: " + checkpoint.name);
                currentCheckpointPosition = checkpoint.transform.position;
                foundCheckpointPositions.Add(checkpoint);
                shovableObjectsMovedSinceLastCheckpoint.Clear();
            }
        }
    }

    /// <summary>
    /// Resets level to last checkpoint. So far this includes resetting the player to the last checkpoint spawn point
    /// and resetting all shovable blocks moved after the last checkpoint to their spawn points
    /// </summary>
    private void RestartLevelToLastCheckpoint()
    {
        for (int i = 0; i < shovableObjectsMovedSinceLastCheckpoint.Count; i++)
        {
            shovableObjectsMovedSinceLastCheckpoint[i].ResetShovableObject();
        }
        StartCoroutine(ResetPlayer());
        playerHealth = 3;
        if (playerGold > 0)
        {
            playerGold -= 1;
        }
        playerDeaths += 1;
    }

    /// <summary>
    /// Updates the shovable objects moved since last checkpoint list by attempting to add a shovable object script
    /// Will be called by the player when they shove a block
    /// </summary>
    /// <param name="shovableObjectToAdd"></param>
    public void UpdateShovableObjectsMovedList(ShovableObject shovableObjectToAdd)
    {
        Debug.Log("Trying to add shovable object to list: " + shovableObjectToAdd);

        if (shovableObjectToAdd != null)
        {
            if (!shovableObjectsMovedSinceLastCheckpoint.Contains(shovableObjectToAdd))
            {
                shovableObjectsMovedSinceLastCheckpoint.Add(shovableObjectToAdd);
            }
        }
    }
    
    /// <summary>
    /// Updates and teleports player to next checkpoint
    /// </summary>
    public void TeleportPlayerToNextCheckpoint()
    {
        if (foundCheckpointPositions.Count < allCheckpointPositions.Count)
        {
            Debug.Log("Teleporting player to next checkpoint");
            UpdatePlayerCheckpoint(allCheckpointPositions[foundCheckpointPositions.Count]);
            StartCoroutine(ResetPlayer());
        }
    }

    /// <summary>
    /// Resets player, this is done here in the game manager with a time delay to make sure the character controller 
    /// does not override player position
    /// </summary>
    /// <returns></returns>
    public IEnumerator ResetPlayer()
    {
        playerController.ResetPlayer();
        playerController.transform.position = currentCheckpointPosition;
        playerController.enabled = false;
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < fullHearts.Count; i++)
        {
            fullHearts[i].SetActive(true);
        }
        cointText.text = "x " + playerGold;
        playerController.enabled = true;
    }


    public void SetActiveSepcialAbility(string specialAbility)
    {
        if (specialAbility == "Pogostick")
        {
            pogostickIcon.SetActive(true);
        }
    }

    /// <summary>
    /// Toggles the dev tools menu display
    /// </summary>
    /// <param name="value"></param>
    private void OnDevTool(InputValue value)
    {
        // comment out for final build
        /*devToolsMenu.SetActive(!devToolsMenu.activeInHierarchy);
        Cursor.visible = devToolsMenu.activeInHierarchy;
        */
    }

    /// <summary>
    /// Toggles game pause
    /// </summary>
    /// <param name="value"></param>
    private void OnPause(InputValue value)
    {
        if (isLevelWon)
        {
            return;
        }

        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            PauseGame();
        }
        else
        {
            Time.timeScale = 1.0f;
            playerController.ResetPlayer();
            playerController.EnablePlayerInput();
            cameraInput.enabled = true;
            pauseMenu.SetActive(false);
            Cursor.visible = devToolsMenu.activeInHierarchy;
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0.0f;
        playerController.DisablePlayerInput();
        cameraInput.enabled = false;
        pauseMenu.SetActive(true);
        Cursor.visible = true;
    }

    public void WinLevel()
    {
        Time.timeScale = 0.0f;
        playerController.DisablePlayerInput();
        cameraInput.enabled = false;
        Cursor.visible = true;
        isLevelWon = true;
        winMenu.SetActive(true);
        winMenuText.text = "Thanks for playing!\n" +
                           "Treasure chests found: " + chestsFound +  "/8\n" +
                           "Deaths: " + playerDeaths + "\n" +
                           "Gold collected: " + playerGold;
    }

    /// <summary>
    /// Resumes game
    /// </summary>
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1.0f;
        playerController.EnablePlayerInput();
        pauseMenu.SetActive(false);
        Cursor.visible = devToolsMenu.activeInHierarchy;
    }
}
