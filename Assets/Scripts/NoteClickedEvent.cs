using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;

public class NoteClickedEvent : MonoBehaviour
{
    public ScoreManager scoreManager;
    public AudioSource audioSource;
    public KeyEvent keyEvent;
    public GameObject[] score4K;
    public Animator[] hitLights4K;
    public GameObject[] score7K;
    public Animator[] hitLights7K;

    private IniManager settings = new IniManager(".\\settings.ini");
    private IniManager info = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\info.ini");
    public GameState gameState;

    private int key = 4;
    // Start is called before the first frame update
    void Start()
    {
        key = Int32.Parse(info.ReadIniFile("info", "Key", "4"));
        for(int i = 0; i< key; i++) {
            StartCoroutine(KeyEvent(i));
        }
    }

    IEnumerator KeyEvent(int i) {
        yield return new WaitForSeconds(0.02f);
        while(true) {
            GameObject[] note = GameObject.FindGameObjectsWithTag("canclick"+i);

            if(note.Length > 0) {
                if(keyEvent.KeyDownEvents[i]) {
                    if(key == 4) {
                        hitLights4K[i].Play("hit");
                    }
                    else if(key == 7) {
                        hitLights7K[i].Play("hit");
                    }
                    if(note[0].GetComponent<NoteTimer>().timing <= 30) {
                        scoreManager.score += scoreManager.MAX_SCORE/gameState.note_amount;
                        scoreManager.perfect_plus++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(0);
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 50) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(300f/305f));
                        scoreManager.perfect++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(1);
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 60) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(200f/305f));
                        scoreManager.great++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(2);
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 80) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(100f/305f));
                        scoreManager.good++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        PlayScoreAnimation(3);
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 100) {
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
            for(int j = 0; j< 6; j++) {
                if(j == i) {
                    continue;
                }
                score4K[j].GetComponent<Animator>().Play("Idle");
                score4K[j].transform.localScale = new Vector3(0, 0, 0);
            }
        }
        else if(key == 7) {
            score7K[i].GetComponent<Animator>().Play("score");
            for(int j = 0; j< 6; j++) {
                if(j == i) {
                    continue;
                }
                score7K[j].GetComponent<Animator>().Play("Idle");
                score7K[j].transform.localScale = new Vector3(0, 0, 0);
            }
        }
    }
}
