using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;
using TMPro;

public class Keybind : MonoBehaviour
{
    private bool isSetting = false;
    private IniManager iniManager = new IniManager(".\\settings.ini");
    public void SetKeybind(int btn) {
        if(!isSetting) {        
            int key = btn/10;
            int index = btn%10;
            isSetting = true;
            StartCoroutine(StartSetting(key, index));
        }
    }

    IEnumerator StartSetting(int key, int index) {
        GameObject.FindGameObjectWithTag("set"+key+"k"+index).GetComponent<TMP_Text>().text = "...";
        while(isSetting) {
            yield return new WaitForSeconds(0);

            foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                if(Input.GetKeyDown(keyCode)) {
                    for(int i = 0; i< key; i++) {
                        if(iniManager.ReadIniFile(key+"k", "key"+i, "") == keyCode.ToString() && i != index) {
                            iniManager.WriteIniFile(key+"k", "key"+i, iniManager.ReadIniFile(key+"k", "key"+index, ""));
                            GameObject.FindGameObjectWithTag("set"+key+"k"+i).GetComponent<TMP_Text>().text = iniManager.ReadIniFile(key+"k", "key"+index, "");
                            break;
                        }
                    }
                    iniManager.WriteIniFile(key+"k", "key"+index, keyCode.ToString());
                    GameObject.FindGameObjectWithTag("set"+key+"k"+index).GetComponent<TMP_Text>().text = keyCode.ToString();
                    isSetting = false;
                    break;
                }
            }
        }
    }
}
