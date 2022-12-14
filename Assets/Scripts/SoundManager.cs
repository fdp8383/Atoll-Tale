using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public enum Sound
    {
        dig,
        clang,
        eat,
        stepOne,
        stepTwo,
        jump,
        stoneSliding,
        treeBreeze,
        oceanWaves,
        menuMusic,
        gameMusic,
        startGame,
        clickButton,
        clickBack,
        quitGame
    }

    public static void PlaySound(Sound sound)
    {
        GameObject soundGO = new GameObject("Sound");
        AudioSource audioSource = soundGO.AddComponent<AudioSource>();
        audioSource.volume = GameObject.Find("GameAssets").GetComponent<GameAssets>().soundVolume;
        audioSource.PlayOneShot(GetAudioClip(sound));
    }

    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach(GameAssets.SoundAudioClip soundAudioClip in GameAssets.instance.audioClips )
        {
            if(soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }
        Debug.LogError("Sound" + sound + "not found!");
        return null;
    }
}
