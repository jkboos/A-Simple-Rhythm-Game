using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;
using TMPro;
using System.Globalization;
using UnityEngine.UI;

public class SliderEvent : MonoBehaviour
{
    /*
        0: VolumeValue
        1: SpeedValue
        2: VolumeSlider
        3: SpeedSlider
    */
    public GameObject[] objects;


    IniManager iniManager = new IniManager(".\\settings.ini");

    public void VolumeChanged() {
        float value = float.Parse(iniManager.ReadIniFile("settings", "volume", "0"), CultureInfo.InvariantCulture.NumberFormat);
        value = objects[2].GetComponent<Slider>().value;
        iniManager.WriteIniFile("settings", "volume", value);

        objects[0].GetComponent<TMP_Text>().text = (int)Math.Round(value*100) + "%";
    }

    public void SpeedChanged() {
        int value = Int32.Parse(iniManager.ReadIniFile("settings", "speed", "0"));
        value = (int)objects[3].GetComponent<Slider>().value;
        iniManager.WriteIniFile("settings", "speed", value);

        objects[1].GetComponent<TMP_Text>().text = value.ToString();
    }
}
