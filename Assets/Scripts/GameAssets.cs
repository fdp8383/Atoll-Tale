using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameAssets : MonoBehaviour
{
    public static GameAssets instance;
    public List<SoundAudioClip> audioClips;
    [SerializeField]
    private Slider soundSlider;
    [SerializeField]
    private Slider musicSlider;
    public float soundVolume = 0.5f;
    public float musicVolume = 0.5f;
    [SerializeField]
    private AudioSource menuMusic;
    [SerializeField]
    private AudioSource gameMusic;
    [SerializeField]
    private AudioSource oceanMusic;
    [SerializeField]
    private AudioSource treeMusic;

    private void Awake()
    {
        //set up singleton instance
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    [System.Serializable]
    public class SoundAudioClip
    {
        public SoundManager.Sound sound;
        public AudioClip audioClip;
    }

    public void Start()
    {
        if (!soundSlider)
        {
            soundSlider = GameObject.Find("SoundsSlider").GetComponent<Slider>();
        }
        soundSlider.value = soundVolume;

        if (!musicSlider)
        {
            musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        }
        musicSlider.value = musicVolume;

        soundSlider.onValueChanged.AddListener(delegate { SoundSliderChangeCheck(); });
        musicSlider.onValueChanged.AddListener(delegate { MusicSliderChangeCheck(); });

        GameObject.Find("SettingsMenu").SetActive(false);
    }

    public void SoundSliderChangeCheck()
    {
        soundVolume = soundSlider.value;
    }

    public void MusicSliderChangeCheck()
    {
        musicVolume = musicSlider.value;
        UpdateMusicSoundValues();
    }

    public void UpdateMusicSoundValues()
    {
        menuMusic.volume = 0.4f * musicVolume;
        gameMusic.volume = 0.4f * musicVolume;
        treeMusic.volume = 1.0f * musicVolume;
        oceanMusic.volume = 0.15f * musicVolume;
    }

    private void Update()
    {
        if (!soundSlider)
        {
            soundSlider = GameObject.Find("SoundsSlider").GetComponent<Slider>();
            soundSlider.value = soundVolume;
            soundSlider.onValueChanged.AddListener(delegate { SoundSliderChangeCheck(); });
        }

        if (!musicSlider)
        {
            musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(delegate { MusicSliderChangeCheck(); });
            GameObject.Find("SettingsMenu").SetActive(false);
            GameObject.Find("PauseMenu").SetActive(false);
        }
    }
}
