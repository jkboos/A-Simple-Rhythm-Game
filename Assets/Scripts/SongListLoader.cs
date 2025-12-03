using System;
using System.Collections;
using System.Collections.Generic;
using AirFishLab.ScrollingList;
using TMPro;
using UnityEngine;
using ini_read_write;
using Unity.Mathematics;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.UI;

public class SongListLoader : MonoBehaviour
{
    [Header("最低歌曲列表數量")]
    public int amount = 8;
    // public SelectMusicLoader musicLoader;
    public Image background;
    public Score score;


    // Start is called before the first frame update
    void Start()
    {

        int x = Directory.GetDirectories(".\\Songs").Length;
        if(x > amount) {
            amount = x;
        }

        GetComponent<RectTransform>().anchoredPosition = new Vector2(GetAnchorPosX(amount), this.GetComponent<RectTransform>().anchoredPosition.y);

        GetComponent<CircularScrollingList>().BoxSetting.SetNumOfBoxes(amount);
        GetComponent<CircularScrollingList>().ListSetting.SetBoxDensity(GetDensity(amount));
        
        // GetComponent<CircularScrollingList>().GenerateBoxesAndArrange();
        GetComponent<CircularScrollingList>().Initialize();
        
        for(int i = 0; i < x; i++) {
            string song_path = StateController.songs_path[i];
            Debug.Log(song_path);
            FileInfo[] file = new DirectoryInfo(song_path).GetFiles("info.ini");

            IniManager info = new IniManager(file[0].FullName);
            GetComponent<CircularScrollingList>().ListBoxes[i].transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = info.ReadIniFile("Info", "Name", "Error")+"\n["+info.ReadIniFile("Info", "Version", "")+"]";
            
        }
        
        if(amount/2+amount%2-1 == StateController.cur_song_index) {
            // musicLoader.PlayMusic(StateController.cur_song_index);
            if (!AudioManager.Instance.BGM_is_playing())
            {
                AudioManager.Instance.play_BGM(StateController.cur_song_index);
            }
            BGManager.SetBackgroundImage(StateController.songs_path[StateController.cur_song_index], background);
            score.LoadScore();
            score.LoadTimeAndKey();
        }
        else {
            MoveTo(amount/2+amount%2-1, StateController.cur_song_index);
        }
    }

    void MoveTo(int cur_index, int target_index) {

        int up_steps = 0;
        int down_steps = 0;

        if(cur_index < target_index) {
            up_steps = cur_index+amount-target_index;
            down_steps = target_index-cur_index;
        }
        else {
            up_steps = cur_index-target_index;
            down_steps = amount-target_index+cur_index;
        }
        if(up_steps < down_steps) {
            for(int i = 0; i< up_steps; i++)
            {
                GetComponent<CircularScrollingList>().MoveOneUnitDown();
            }
        }
        else {
            for(int i = 0; i< down_steps; i++) 
            {
                GetComponent<CircularScrollingList>().MoveOneUnitUp();
            }
        }
    }

    float GetDensity(int x) {
        return 6.75f/(x-1);
    }

    float GetAnchorPosX(int x) {
        return -21.9897959183673f*x + 243.9795918367347f;
    }

    
}

