using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;
using System.Globalization;
using UnityEngine.UI;
using TMPro;

public class SettingsInitialize : MonoBehaviour
{
    IniManager iniManager = new IniManager(".\\settings.ini");

    public GameObject VolumeSlider;
    public GameObject VolumeValue;
    public GameObject SpeedSlider;
    public GameObject SpeedValue;
    public GameObject OffsetValue;
    public GameObject FullScreenToggle;
    public GameObject[] set4k;
    public GameObject[] set7k;

    void Start()
    {
        //Volume
        float volume = float.Parse(iniManager.ReadIniFile("settings", "volume", "0"), CultureInfo.InvariantCulture.NumberFormat);
        VolumeSlider.GetComponent<Slider>().value = volume;
        VolumeValue.GetComponent<TMP_Text>().text = (int)Math.Round(volume*100) + "%";

        //Speed
        int speed = Int32.Parse(iniManager.ReadIniFile("settings", "speed", "0"));
        SpeedSlider.GetComponent<Slider>().value = speed;
        SpeedValue.GetComponent<TMP_Text>().text = speed.ToString();

        //Offset
        OffsetValue.GetComponent<TMP_Text>().text = iniManager.ReadIniFile("settings", "offset", "0") + "ms";

        //FullScreen
        if(iniManager.ReadIniFile("settings", "fullscreen", "1") == "1") {
            FullScreenToggle.GetComponent<Toggle>().isOn = true;
        }
        else {
            FullScreenToggle.GetComponent<Toggle>().isOn = false;
        }

        //keybind
        for(int i = 0; i< 4; i++) {
            set4k[i].transform.GetChild(0).GetComponent<TMP_Text>().text = iniManager.ReadIniFile("4k", "key"+i, "D");
        }
        for(int i = 0; i< 7; i++) {
            set7k[i].transform.GetChild(0).GetComponent<TMP_Text>().text = iniManager.ReadIniFile("7k", "key"+i, "D");
        }
    }
}
