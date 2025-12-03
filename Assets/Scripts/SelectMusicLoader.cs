using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using ini_read_write;
using System.Globalization;


public class SelectMusicLoader : MonoBehaviour
{
    IniManager iniManager = new IniManager(".\\settings.ini");

    public static AudioSource audioSource;
    

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StateController.songs_path = Directory.GetDirectories(".\\Songs");
        //StateController.cur_song_index = Random.Range(0, StateController.songs_path.Length);

    }

    public void PlayMusic(int index) {
        StartCoroutine(GetAudioClip(GetAudiosByPath(StateController.songs_path[index])));
    }
    public void StopMusic() {
        if(audioSource.isPlaying) {
            audioSource.Stop();
        }
    }

    void Update() {
        audioSource.volume = float.Parse(iniManager.ReadIniFile("settings", "volume", "0"), CultureInfo.InvariantCulture.NumberFormat);
    }

    IEnumerator GetAudioClip(string file_path) {
        Debug.Log("file://"+file_path);
        UnityWebRequest _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip("file://"+file_path, AudioType.MPEG);
        yield return _unityWebRequest.SendWebRequest();
        AudioClip _audioClip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
        audioSource.clip = _audioClip;
        audioSource.volume = 1;
        audioSource.Play();
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
