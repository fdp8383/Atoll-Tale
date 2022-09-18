using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;
    public List<AudioClip> audioClips;
    public Dictionary<string, AudioClip> soundLibrary;

    public static GameAssets Instance 
    {
        get 
        {
            if (_i == null) _i = (Instantiate(Resources.Load("GameAssets")) as GameObject).GetComponent<GameAssets>();
            return _i;
        }
    }

    private void Start()
    {
        //add sound clips to library
        soundLibrary = new Dictionary<string, AudioClip>();
        for(int i = 0; i < audioClips.Count; i++)
        {
            soundLibrary.Add(audioClips[i].name, audioClips[i]);
        }
    }
}
