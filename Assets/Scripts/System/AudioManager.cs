using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioLists;
    AudioSource audioPlayer;

    private static AudioManager prAudioManager;
    public static Dictionary<CharacterType, UnitConfig> unitDictionary;
    public static AudioManager instance
    {
        get
        {
            if (!prAudioManager)
            {
                prAudioManager = FindObjectOfType<AudioManager>();
                if (!prAudioManager)
                {
                }
                else
                {
                }
            }

            return prAudioManager;
        }
    }
    private void Awake()
    {
        AudioManager[] obj = FindObjectsOfType<AudioManager>();
        if (obj.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            audioPlayer = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        EventManager.StartListening(MyEvents.EVENT_SCENE_CHANGED, OnSceneChanged);

    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_SCENE_CHANGED, OnSceneChanged);
    }

    private void OnSceneChanged(EventObject arg0)
    {
        int sceneIdx = arg0.intObj;

        Debug.Log("Scene changed" + sceneIdx);
        audioPlayer.clip = audioLists[sceneIdx];
        audioPlayer.Play();
    }
}
