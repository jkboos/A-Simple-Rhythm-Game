using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ini_read_write;
using System.Globalization;
using System.IO;

public class Score : MonoBehaviour
{
    public GameObject info_panel;
    public TMP_Text time;
    public TMP_Text key;
    public TMP_Text score;
    public Image rank_icon;
    public TMP_Text combo;
    public TMP_Text accuracy;
    public TMP_Text perfect_plus;
    public TMP_Text perfect;
    public TMP_Text great;
    public TMP_Text good;
    public TMP_Text bad;
    public TMP_Text miss;
    public void LoadScore() {
        if(File.Exists(StateController.songs_path[StateController.cur_song_index]+"\\score.ini")) {
            IniManager iniManager = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\score.ini");

            score.text = iniManager.ReadIniFile("Record", "score", "Error");
            combo.text = iniManager.ReadIniFile("Record", "max_combo", "Error");
            accuracy.text = (float.Parse(iniManager.ReadIniFile("Record", "accuracy", "Error"), CultureInfo.InvariantCulture.NumberFormat)*100).ToString("0.00") + "%";
            perfect_plus.text = iniManager.ReadIniFile("Record", "perfect_plus", "Error");
            perfect.text = iniManager.ReadIniFile("Record", "perfect", "Error");
            great.text = iniManager.ReadIniFile("Record", "great", "Error");
            good.text = iniManager.ReadIniFile("Record", "good", "Error");
            bad.text = iniManager.ReadIniFile("Record", "bad", "Error");
            miss.text = iniManager.ReadIniFile("Record", "miss", "Error");

            string rank = iniManager.ReadIniFile("Record", "rank", "D");
            switch(rank) {
                case "SS":
                    rank_icon.sprite = Resources.Load<Sprite>("rank/SS");
                    break;
                case "S":
                    rank_icon.sprite = Resources.Load<Sprite>("rank/S");
                    break;
                case "A":
                    rank_icon.sprite = Resources.Load<Sprite>("rank/A");
                    break;
                case "B":
                    rank_icon.sprite = Resources.Load<Sprite>("rank/B");
                    break;
                case "C":
                    rank_icon.sprite = Resources.Load<Sprite>("rank/C");
                    break;
                case "D":
                    rank_icon.sprite = Resources.Load<Sprite>("rank/D");
                    break;
            }

            info_panel.transform.GetChild(2).gameObject.SetActive(true);
            info_panel.transform.GetChild(3).gameObject.SetActive(false);
        }
        else {
            info_panel.transform.GetChild(2).gameObject.SetActive(false);
            info_panel.transform.GetChild(3).gameObject.SetActive(true);
        }
    }

    public void LoadTimeAndKey() {
        IniManager iniManager = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\info.ini");

        time.text = iniManager.ReadIniFile("info", "time", "00:00");
        key.text = iniManager.ReadIniFile("info", "Key", "0") + "k";
    }
}
