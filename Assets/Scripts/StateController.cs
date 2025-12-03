using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    public static string[] songs_path = {};
    public static int cur_song_index = 4;
    public static string cur_song_path;
    public static AudioClip button_click_sound =  Resources.Load<AudioClip>("button/button_click");
    public static bool list_box_init = true;
}
