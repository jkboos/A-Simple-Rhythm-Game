using System;
using System.Collections;
using System.Globalization;
using System.IO;
using ini_read_write;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    IniManager iniManager = new IniManager(".\\settings.ini");
    public static AudioManager Instance;

    private AudioSource button_click;
    private AudioSource BGM;
    private bool fadein = false;
    
    [Range(0, 1)]
    public float fadeIn_speed = 0.02f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            button_click = gameObject.AddComponent<AudioSource>();
            BGM = gameObject.AddComponent<AudioSource>();
            
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update() {
        if (!fadein)
        {
            BGM.volume = float.Parse(iniManager.ReadIniFile("settings", "volume", "0"), CultureInfo.InvariantCulture.NumberFormat);
        }
        
    }

    public void play_button_click(AudioClip clip)
    {
        button_click.PlayOneShot(clip);
    }

    public void play_BGM(int index)
    {
        StartCoroutine(GetAudioClip(GetAudiosByPath(StateController.songs_path[StateController.cur_song_index]), () => BGM.Play()));
    }

    public void load_BGM(int index)
    {
        StartCoroutine(GetAudioClip(GetAudiosByPath(StateController.songs_path[StateController.cur_song_index])));
    }

    public void stop_BGM()
    {
        BGM.Stop();
    }

    public void pause_BGM()
    {
        BGM.Pause();
    }

    public void resume_BGM()
    {
        BGM.Play();
    }

    IEnumerator fadeIn()
    {
        while (BGM.volume < float.Parse(iniManager.ReadIniFile("settings", "volume", "0"),
                   CultureInfo.InvariantCulture.NumberFormat))
        {
            if (BGM.volume + fadeIn_speed >= float.Parse(iniManager.ReadIniFile("settings", "volume", "0"),
                    CultureInfo.InvariantCulture.NumberFormat))
            {
                BGM.volume =  float.Parse(iniManager.ReadIniFile("settings", "volume", "0"), CultureInfo.InvariantCulture.NumberFormat);
                fadein = false;
            }
            else
            {
                BGM.volume += fadeIn_speed;
            }

            yield return new WaitForSecondsRealtime(0.001f);
        }
    }
    public void fadein_resume_BGM()
    {
        fadein = true;
        BGM.volume = 0;
        BGM.Play();
        StartCoroutine(fadeIn());
    }

    public bool BGM_is_playing()
    {
        return BGM.isPlaying;
    }
    
    public IEnumerator GetAudioClip(string file_path, Action callback = null) {
        Debug.Log("file://"+file_path);
        UnityWebRequest _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip("file://"+file_path, AudioType.MPEG);
        yield return _unityWebRequest.SendWebRequest();
        AudioClip _audioClip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
        BGM.clip = _audioClip;
        callback?.Invoke();
    }

    public string GetAudiosByPath(string path) {
 
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