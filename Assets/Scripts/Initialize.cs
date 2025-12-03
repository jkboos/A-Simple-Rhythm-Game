using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ini_read_write;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Initialize : MonoBehaviour
{
    private IniManager iniManager = new IniManager(".\\settings.ini");
    public Image background;

    void Start()
    {
        StateController.list_box_init = true;
        if(iniManager.ReadIniFile("settings", "fullscreen", "1") == "1") {
            Screen.SetResolution(1920, 1080, true);
        }
        else {
            Screen.SetResolution(1920, 1080, false);
        }
        
        StateController.songs_path = Directory.GetDirectories(".\\Songs");
        Array.Sort(StateController.songs_path, (a, b) => {
            string name1 = a.Split(" - ")[a.Split(" - ").Length-1];
            string name2 = b.Split(" - ")[b.Split(" - ").Length-1];
            return name1.CompareTo(name2);
        });

        if (StateController.songs_path.Length > 0)
        {
            if (!AudioManager.Instance.BGM_is_playing())
            {
                StateController.cur_song_index = Random.Range(0, StateController.songs_path.Length);
                StateController.cur_song_path = StateController.songs_path[StateController.cur_song_index];
                AudioManager.Instance.play_BGM(StateController.cur_song_index);
            }
            else
            {
                StateController.cur_song_index = Array.IndexOf(StateController.songs_path, StateController.cur_song_path);
            }
            BGManager.SetBackgroundImage(StateController.songs_path[StateController.cur_song_index], background);
        }
        else
        {
            BGManager.SetBackgroundImage(".//", background);    
        }
        
    }
}
