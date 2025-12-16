using System.Collections.Generic;
using NUnit.Framework;
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
    
    public static Dictionary<string, bool> mods = new Dictionary<string, bool>
    {
        ["NF"] = false,
        ["DT"] = false,
        ["HT"] = false,
        ["AT"] = false,
        ["EZ"] = false,
        ["FI"] = false,
        ["FL"] = false,
        ["HD"] = false,
        ["HR"] = false,
        ["MR"] = false,
        ["PF"] = false,
        ["SD"] = false,
    };
    
    public static Dictionary<string, List<string>> mods_conflict = new Dictionary<string, List<string>>
    {
        ["NF"] = new List<string>() {"AT", "SD", "PF"},
        ["DT"] = new List<string>() {"HT"},
        ["HT"] = new List<string>() {"DT"},
        ["AT"] = new List<string>() {"NF"},
        ["EZ"] = new List<string>() {"HR"},
        ["FI"] = new List<string>() {"FL", "HD"},
        ["FL"] = new List<string>() {"FI", "HD"},
        ["HD"] = new List<string>() {"FI", "FL"},
        ["HR"] = new List<string>() {"EZ"},
        ["PF"] = new List<string>() {"SD", "NF"},
        ["SD"] = new List<string>() {"PF", "NF"}
    };
}
