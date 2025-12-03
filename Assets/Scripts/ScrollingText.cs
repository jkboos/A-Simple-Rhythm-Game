using System;
using TMPro;
using UnityEngine;
using System.IO;
using ini_read_write;

public class ScrollingText : MonoBehaviour
{
    [Header("面板")]
    public GameObject panel;
    [Header("捲動速度")]
    public float scroll_speed = 100;

    private IniManager info;

    void Start()
    {
        GetComponent<TMP_Text>().autoSizeTextContainer = true;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = new Vector3((this.transform.position.x-scroll_speed*Time.deltaTime), this.transform.position.y, this.transform.position.z);
        if(this.transform.position.x < -this.GetComponent<RectTransform>().sizeDelta.x/2) {
            this.transform.position = new Vector3(Screen.width+this.GetComponent<RectTransform>().sizeDelta.x/2, this.transform.position.y, this.transform.position.z);
        }

        try
        {
            string song_path = StateController.songs_path[StateController.cur_song_index];
            FileInfo[] file = new DirectoryInfo(song_path).GetFiles("info.ini");
            info = new IniManager(file[0].FullName);
            this.GetComponent<TMP_Text>().text = info.ReadIniFile("Info", "Name", "Error");
        }
        catch (Exception e)
        {
            
        }

    }
}
