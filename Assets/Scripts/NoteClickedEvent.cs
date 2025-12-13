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
    public Animator acc_score_animation;
    public Animator[] hitLights4K;
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
                        acc_score_animation.SetTrigger("perfect+");
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 50) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(300f/305f));
                        scoreManager.perfect++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        acc_score_animation.SetTrigger("perfect");
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 60) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(200f/305f));
                        scoreManager.great++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        acc_score_animation.SetTrigger("great");
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 80) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(100f/305f));
                        scoreManager.good++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        acc_score_animation.SetTrigger("good");
                    }
                    else if(note[0].GetComponent<NoteTimer>().timing <= 100) {
                        scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(50f/305f));
                        scoreManager.bad++;
                        scoreManager.combo++;
                        Destroy(note[0]);
                        acc_score_animation.SetTrigger("ok");
                    }
                    else {
                        scoreManager.miss++;
                        scoreManager.combo = 0;
                        Destroy(note[0]);
                        acc_score_animation.SetTrigger("miss");
                    }
                }
            }
            yield return new WaitForSeconds(0);
        }
    }
}
