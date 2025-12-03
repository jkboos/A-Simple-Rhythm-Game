using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using System.IO;
using System.Text;
using ini_read_write;

public class ScoreManager : MonoBehaviour
{
    [Header("分數上限")]
    public int MAX_SCORE = 1000000;
    [Header("分數")]
    public int score = 0;
    [Header("精準度")]
    public float accuracy = 0;
    [Header("最大連擊數")]
    public int max_combo = 0;
    [Header("連擊數")]
    public int combo = 0;
    [Header("Perfect+")]
    public int perfect_plus = 0;
    [Header("Perfect")]
    public int perfect = 0;
    [Header("Great")]
    public int great = 0;
    [Header("Good")]
    public int good = 0;
    [Header("Bad")]
    public int bad = 0;
    [Header("Miss")]
    public int miss = 0;
    [Header("Rank")]
    public string rank;

    public TMP_Text score_text;
    public TMP_Text accuracy_text;
    public TMP_Text combo_text;

    public TMP_Text settle_score_text;
    public TMP_Text settle_perfect_plus_text;
    public TMP_Text settle_perfect_text;
    public TMP_Text settle_great_text;
    public TMP_Text settle_good_text;
    public TMP_Text settle_bad_text;
    public TMP_Text settle_miss_text;
    public TMP_Text settle_combo_text;
    public TMP_Text settle_accuracy_text;
    public Image rank_icon;

    private int cur_score = 0;

    void Start() {
        updateScore();
    }
    void Update() {
        score_text.text = cur_score.ToString();
        accuracy_text.text = Math.Round(calculateAccuracy()*100, 2).ToString("0.00")+"%";
        combo_text.text = combo.ToString();
        if(combo > max_combo) {
            max_combo = combo;
        }
    }

    void updateScore() {
        DOVirtual.Int(cur_score, score, 0.2f, (x) => {
            cur_score = x;
        }).OnComplete(() => {
            updateScore();
        });
    }

    float calculateAccuracy() {
        if(perfect_plus+perfect+great+good+bad+miss > 0) {
            accuracy = (300f*(perfect_plus+perfect)+200f*great+100f*good+50f*bad)/(300f*(perfect_plus+perfect+great+good+bad+miss));
        }
        return accuracy;
    }
    
    public void SettleScore() {
        settle_score_text.text = score.ToString();
        settle_accuracy_text.text = (accuracy*100).ToString("0.00")+"%";
        settle_combo_text.text = max_combo.ToString();
        settle_perfect_plus_text.text = perfect_plus.ToString();
        settle_perfect_text.text = perfect.ToString();
        settle_great_text.text = great.ToString();
        settle_good_text.text = good.ToString();
        settle_bad_text.text = bad.ToString();
        settle_miss_text.text = miss.ToString();

        if(accuracy == 1f) {
            rank_icon.sprite = Resources.Load<Sprite>("rank/SS");
            rank = "SS";
        }
        else if(accuracy >= 0.95f) {
            rank_icon.sprite = Resources.Load<Sprite>("rank/S");
            rank = "S";
        }
        else if(accuracy >= 0.9f) {
            rank_icon.sprite = Resources.Load<Sprite>("rank/A");
            rank = "A";
        }
        else if(accuracy >= 0.8f) {
            rank_icon.sprite = Resources.Load<Sprite>("rank/B");
            rank = "B";
        }
        else if(accuracy >= 0.7f) {
            rank_icon.sprite = Resources.Load<Sprite>("rank/C");
            rank = "C";
        }
        else {
            rank_icon.sprite = Resources.Load<Sprite>("rank/D");
            rank = "D";
        }

        if(File.Exists(StateController.songs_path[StateController.cur_song_index]+"\\score.ini")) {
            IniManager iniManager = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\score.ini");
            if(Int32.Parse(iniManager.ReadIniFile("Record", "score", "-1")) < score) {
                WriteScore();
            }
        }
        else {
            WriteScore();
        }

        
    }

    void WriteScore() {
        using (FileStream fs = File.Create(StateController.songs_path[StateController.cur_song_index]+"\\score.ini")) {
            Byte[] info = new UTF8Encoding(true).GetBytes(
                "[Record]\n"+
                "score="+score.ToString()+"\n"+
                "accuracy="+accuracy.ToString()+"\n"+
                "max_combo="+max_combo.ToString()+"\n"+
                "perfect_plus="+perfect_plus.ToString()+"\n"+
                "perfect="+perfect.ToString()+"\n"+
                "great="+great.ToString()+"\n"+
                "good="+good.ToString()+"\n"+
                "bad="+bad.ToString()+"\n"+
                "miss="+miss.ToString()+"\n"+
                "rank="+rank
            );
			fs.Write(info, 0, info.Length);
        }
    }
}
