using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        StartCoroutine("StartGameDelayed");
    }

    public IEnumerator StartGameDelayed()
    {
        SoundManager.PlaySound(SoundManager.Sound.startGame);
        yield return new WaitForSeconds(1.9f);
        GameObject.Find("GameAssets").GetComponent<GameAssets>().PlayGameSong();
        SceneManager.LoadScene("MainScene");
    }

    public void QuitGame()
    {
        StartCoroutine("QuitGameDelayed");
    }    

    public IEnumerator QuitGameDelayed()
    {
        SoundManager.PlaySound(SoundManager.Sound.clickBack);
        yield return new WaitForSeconds(0.8f);
        Application.Quit();
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1.0f;
        StartCoroutine("ToMainMenuDelayed");
    }

    public IEnumerator ToMainMenuDelayed()
    {
        SoundManager.PlaySound(SoundManager.Sound.clickBack);
        yield return new WaitForSeconds(0.8f);
        GameObject.Find("GameAssets").GetComponent<GameAssets>().PlayMenuSong();
        SceneManager.LoadScene("MainMenu");
    }

    public void SelectButtonSound()
    {
        SoundManager.PlaySound(SoundManager.Sound.clickButton);
    }

    public void BackButtonSound()
    {
        SoundManager.PlaySound(SoundManager.Sound.clickBack);
    }
}
