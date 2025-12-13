using UnityEngine;

public class StateController
{
    public static string[] songs_path = {};
    public static int cur_song_index = 4;
    public static string cur_song_path;
    public static AudioClip button_click_sound =  Resources.Load<AudioClip>("button/button_click");
    public static AudioClip death_sound =  Resources.Load<AudioClip>("SFX/death");
    public static AudioClip fail_music =  Resources.Load<AudioClip>("fail/fail_music");
    public static bool list_box_init = true;
}
