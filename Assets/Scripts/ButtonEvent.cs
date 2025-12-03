using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ini_read_write;
using TMPro;
using AirFishLab.ScrollingList;
using UnityEngine.Events;
using UnityEngine.UI;


public class ButtonEvent : MonoBehaviour
{
    public GameObject[] objects;
    IniManager iniManager = new IniManager(".\\settings.ini");
    public UnityEvent<GameObject, Action> fadeout;
    public UnityEvent<GameObject, Action> fadein;
    public GameObject menu_canvas;
    public GameObject settings_canvas;
    public GameObject songlist_canvas;
    
    
    public void GoSettings()
    {
        AudioManager.Instance.play_button_click(StateController.button_click_sound);
        
        fadeout.Invoke(menu_canvas, () =>
        {
            menu_canvas.SetActive(false);
            settings_canvas.SetActive(true);
            fadein.Invoke(settings_canvas, null);
        });
    }
    
    public void BackToMenu() {
        AudioManager.Instance.play_button_click(StateController.button_click_sound);
        
        fadeout.Invoke(settings_canvas, () =>
        {
            settings_canvas.SetActive(false);
            menu_canvas.SetActive(true);
            fadein.Invoke(menu_canvas, () => Destroy(GameObject.FindGameObjectWithTag("fadeout-mask")));
        });
    }
    
    public void OffsetDecrease() {
        int value = Int32.Parse(iniManager.ReadIniFile("settings", "offset", "0"));
        if (Input.GetKey("left shift"))
        {
            value -= 10;
        }
        else
        {
            value -= 1;            
        }
        iniManager.WriteIniFile("settings", "offset", value);

        objects[0].GetComponent<TMP_Text>().text = value + "ms";
    }
    public void OffsetIncrease() {
        int value = Int32.Parse(iniManager.ReadIniFile("settings", "offset", "0"));
        if (Input.GetKey("left shift"))
        {
            value += 10;
        }
        else
        {
            value += 1;            
        }
        iniManager.WriteIniFile("settings", "offset", value);

        objects[0].GetComponent<TMP_Text>().text = value + "ms";
    }
    
    public void GoToSelectSong()
    {
        AudioManager.Instance.play_button_click(StateController.button_click_sound);
        fadeout.Invoke(menu_canvas, () => SceneManager.LoadScene("SelectSong"));
    }
    public void BackToSelectSong()
    {
        StateController.list_box_init = true;
        AudioManager.Instance.fadein_resume_BGM();
        fadeout.Invoke(menu_canvas, () =>
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("SelectSong");
        });
    }
    
    public void GoToMenu() 
    {
        AudioManager.Instance.play_button_click(StateController.button_click_sound);
        fadeout.Invoke(songlist_canvas, () => SceneManager.LoadScene("Menu"));
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void retry()
    {
        Time.timeScale = 1;
        AudioManager.Instance.stop_BGM();
        AudioManager.Instance.play_button_click(StateController.button_click_sound);
        SceneManager.LoadScene("PlayScene");
    }
}
