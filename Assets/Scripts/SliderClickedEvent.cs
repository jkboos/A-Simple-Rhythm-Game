using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;

public class SliderClickedEvent : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject[] score4K;
    public Animator[] hitLights4K;
    public GameObject[] score7K;
    public Animator[] hitLights7K;
    public KeyEvent keyEvent;
    public ScoreManager scoreManager;
    private GameState gameState;

    private IniManager settings = new IniManager(".\\settings.ini");
    private IniManager info = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\info.ini");

    private int key = 4;
    
    
    void Start()
    {
        gameState = GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>();
        key = Int32.Parse(info.ReadIniFile("info", "Key", "4"));
        for(int i = 0; i< key; i++) {
            StartCoroutine(KeyEvent(i));
        }
    }

    IEnumerator KeyEvent(int i) {
        yield return new WaitForSeconds(0.02f);
        while(true) {
            GameObject[] note = GameObject.FindGameObjectsWithTag("canpress"+i);
            

            if(note.Length > 0 && !GameState.pause) {
                if(keyEvent.KeyUpEvents[i]) {
                    note[0].GetComponent<SliderTimer>().pressed = false;
                }
                if(keyEvent.KeyDownEvents[i]) {
                    note[0].GetComponent<SliderTimer>().start_timing = Math.Abs(Time.time*1000-gameState.start_time-note[0].GetComponent<SliderTimer>().clicked_timing);
                    note[0].GetComponent<SliderTimer>().canRealse = true;
                    note[0].GetComponent<SliderTimer>().pressed = true;
                }
                
                if(note[0].GetComponent<SliderTimer>().pressed && keyEvent.KeyEvents[i]) {
                    note[0].transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(note[0].transform.GetChild(0).GetComponent<RectTransform>().offsetMin.x, -1300);

                    note[0].transform.GetChild(2).transform.position = new Vector2(note[0].transform.GetChild(2).transform.position.x, 170);
                    if(key == 4) {
                        hitLights4K[i].Play("hit");
                    }
                    else if(key == 7) {
                        hitLights7K[i].Play("hit");
                    }
                }
                else if (note[0].GetComponent<SliderTimer>().pressed && !keyEvent.KeyEvents[i])
                {
                    note[0].GetComponent<SliderTimer>().pressed = false;
                }
                
                if(note[0].GetComponent<SliderTimer>().canRealse && keyEvent.KeyUpEvents[i]) {
                    note[0].GetComponent<SliderTimer>().finish_timing = Math.Abs(Time.time*1000-gameState.start_time-note[0].GetComponent<SliderTimer>().end_timing);
                    note[0].GetComponent<SliderTimer>().average = (note[0].GetComponent<SliderTimer>().start_timing+note[0].GetComponent<SliderTimer>().finish_timing)/2;

                    if(note[0].GetComponent<SliderTimer>().average <= 30) {
                        scoreManager.score += scoreManager.MAX_SCORE/gameState.note_amount;
                        scoreManager.perfect_plus++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(0);
                    }
                    else if(note[0].GetComponent<SliderTimer>().average <= 50) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(300f/305f));
                        scoreManager.perfect++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(1);
                    }
                    else if(note[0].GetComponent<SliderTimer>().average <= 60) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(200f/305f));
                        scoreManager.great++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(2);
                    }
                    else if(note[0].GetComponent<SliderTimer>().average <= 80) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(100f/305f));
                        scoreManager.good++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(3);
                    }
                    else if(note[0].GetComponent<SliderTimer>().average <= 100) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(50f/305f));
                        scoreManager.bad++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(4);
                    }
                    else {
                        scoreManager.miss++;
                        scoreManager.combo = 0;
                        Destroy(note[0]);
                        PlayScoreAnimation(5);
                    }
                }
            }
            yield return new WaitForSeconds(0);
        }
    }

    void PlayScoreAnimation(int i) {
        if(key == 4) {
            score4K[i].GetComponent<Animator>().Play("score");
            for(int j = 0; j< 5; j++) {
                if(j == i) {
                    continue;
                }
                score4K[j].GetComponent<Animator>().Play("Idle");
                score4K[j].transform.localScale = new Vector3(0, 0, 0);
            }
        }
        else if(key == 7) {
            score7K[i].GetComponent<Animator>().Play("score");
            for(int j = 0; j< 5; j++) {
                if(j == i) {
                    continue;
                }
                score7K[j].GetComponent<Animator>().Play("Idle");
                score7K[j].transform.localScale = new Vector3(0, 0, 0);
            }
        }
    }
}
