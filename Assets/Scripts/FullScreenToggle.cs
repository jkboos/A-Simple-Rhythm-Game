using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using UnityEngine.UI;

public class FullScreenToggle : MonoBehaviour
{

    private IniManager iniManager = new IniManager(".\\settings.ini");
    public void Toggle() {
        if(GetComponent<Toggle>().isOn) {
            Screen.SetResolution(1920, 1080, true);
            iniManager.WriteIniFile("settings", "fullscreen", "1");
        }
        else {
            Screen.SetResolution(1920, 1080, false);
            iniManager.WriteIniFile("settings", "fullscreen", "0");
        }
    }
}
