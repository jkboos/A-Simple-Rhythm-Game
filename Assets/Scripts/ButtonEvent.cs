using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ini_read_write;
using TMPro;
using UnityEngine.Events;
using DG.Tweening;
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
    public GameState gameState;
    public GameObject mods_panel;

    public float mods_default_posX = 100;
    private float mods_posX = 0;
    
    public Material outline_material;
    
    
    public void GoSettings()
    {
        AudioManager.Instance.playSFX(StateController.button_click_sound);
        
        fadeout.Invoke(menu_canvas, () =>
        {
            menu_canvas.SetActive(false);
            settings_canvas.SetActive(true);
            fadein.Invoke(settings_canvas, null);
        });
    }
    
    public void BackToMenu() {
        AudioManager.Instance.playSFX(StateController.button_click_sound);
        
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
        AudioManager.Instance.playSFX(StateController.button_click_sound);
        fadeout.Invoke(menu_canvas, () => SceneManager.LoadScene("SelectSong"));
    }
    public void BackToSelectSong()
    {
        StateController.list_box_init = true;
        AudioManager.Instance.stop_BGM();
        AudioManager.Instance.fadein_resume_BGM();
        if (gameState.is_death)
        {
            menu_canvas = GameObject.FindGameObjectWithTag("fail-canvas");
        }
        fadeout.Invoke(menu_canvas, () =>
        {
            Time.timeScale = 1;
            AudioManager.Instance.set_BGM_speed(1);
            SceneManager.LoadScene("SelectSong");
        });
    }
    
    public void GoToMenu() 
    {
        AudioManager.Instance.playSFX(StateController.button_click_sound);
        fadeout.Invoke(songlist_canvas, () => SceneManager.LoadScene("Menu"));
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void retry()
    {
        Time.timeScale = gameState.speed_multiplier;
        AudioManager.Instance.stop_BGM();
        AudioManager.Instance.playSFX(StateController.button_click_sound);
        SceneManager.LoadScene("PlayScene");
    }

    public void ModsTab()
    {
        if (mods_posX == 0)
        {
            DOVirtual.Float(mods_posX, mods_default_posX, 0.2f, (x) => {
                mods_posX = x;
                mods_panel.GetComponent<RectTransform>().anchoredPosition = new Vector3(mods_posX, mods_panel.GetComponent<RectTransform>().anchoredPosition.y, 0);
            });
        }
        else
        {
            DOVirtual.Float(mods_posX, 0, 0.2f, (x) => {
                mods_posX = x;
                mods_panel.GetComponent<RectTransform>().anchoredPosition = new Vector3(mods_posX, mods_panel.GetComponent<RectTransform>().anchoredPosition.y, 0);
            });
        }
    }
    
    public void ActivateMods(GameObject button)
    {
        float angle(float a)
        {
            if(a <= 180f)
            {
                return a;
            }
            else
            {
                return a - 360f;
            }
        }

        void on_animation(GameObject button)
        {
            float scale = button.transform.localScale.x;
            float rotation = angle(button.transform.localRotation.eulerAngles.z);
            DOVirtual.Float(scale, 1.05f, 0.2f, (x) => {
                scale = x;
                button.transform.localScale = new Vector3(scale, scale, scale);
            });
            DOVirtual.Float(rotation, -8, 0.2f, (x) => {
                rotation = x;
                button.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            });
            button.GetComponent<Image>().material = outline_material;
        }

        void off_animation(GameObject button)
        {
            float scale = button.transform.localScale.x;
            float rotation = angle(button.transform.localRotation.eulerAngles.z);
            DOVirtual.Float(scale, 1, 0.2f, (x) => {
                scale = x;
                button.transform.localScale = new Vector3(scale, scale, scale);
            });
            DOVirtual.Float(rotation, 0, 0.2f, (x) => {
                rotation = x;
                button.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            });
            button.GetComponent<Image>().material = null;
        }
        
        
        bool active;
        if (button.tag == "mod-active")
        {
            active = false;
            button.tag = "Untagged";
            
            off_animation(button);
        }
        else
        {
            if (StateController.mods_conflict.ContainsKey(button.name))
            {
                GameObject[] mods = GameObject.FindGameObjectsWithTag("mod-active");
                foreach (GameObject mod in mods)
                {
                    if (StateController.mods_conflict[button.name].Contains(mod.name))
                    {   
                        mod.tag = "Untagged";
                        StateController.mods[mod.name] = false;
                        off_animation(mod);
                    }
                }
            }
            
            active = true;
            button.tag = "mod-active";
            
            on_animation(button);
        }
        StateController.mods[button.name] = active;
    }
}
