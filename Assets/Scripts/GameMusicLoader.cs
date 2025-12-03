using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using ini_read_write;
using System.Globalization;
using System;


public class GameMusicLoader : MonoBehaviour
{
    IniManager iniManager = new IniManager(".\\settings.ini");

    public AudioSource audioSource;
    

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StateController.songs_path = Directory.GetDirectories(".\\Songs");
        Array.Sort(StateController.songs_path, (a, b) => {
            string name1 = a.Split(" - ")[a.Split(" - ").Length-1];
            string name2 = b.Split(" - ")[b.Split(" - ").Length-1];
            return name1.CompareTo(name2);
        });
    }

    void Update() {
        audioSource.volume = float.Parse(iniManager.ReadIniFile("settings", "volume", "0"), CultureInfo.InvariantCulture.NumberFormat);
    }

    public void LoadMusic() {
        StartCoroutine(GetAudioClip(GetAudiosByPath(StateController.songs_path[StateController.cur_song_index])));
    }

    public void PlayMusic() {
        audioSource.Play();
    }

    IEnumerator GetAudioClip(string file_path) {
        Debug.Log("file://"+file_path);
        UnityWebRequest _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip("file://"+file_path, AudioType.MPEG);
        yield return _unityWebRequest.SendWebRequest();
        AudioClip _audioClip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
        audioSource.clip = _audioClip;
        audioSource.volume = 1;
    }

    string GetAudiosByPath(string path) {
 
        string [] audioClipspath = null;
        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);
            FileInfo[] files = direction.GetFiles("*.mp3");
            audioClipspath = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                audioClipspath[i] = files[i].FullName;
            }
 
        }
        
        return audioClipspath[0];
    }
}
