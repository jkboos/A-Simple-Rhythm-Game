using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;
using UnityEngine.UI;

public class SliderClickedEvent : MonoBehaviour
{
    public AudioSource audioSource;
    public Animator acc_score_animation;
    public Animator[] hitLights4K;
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
        for (int i = 0; i < key; i++)
        {
            StartCoroutine(KeyEvent(i));
        }
        
    }

    IEnumerator KeyEvent(int i) {
        yield return new WaitForSeconds(0.02f);
        while(true) {
            GameObject[] note = GameObject.FindGameObjectsWithTag("canpress"+i);
            
            if(note.Length > 0 && !GameState.pause) {
                if(StateController.mods["AT"])
                {
                    if (Math.Abs(Time.time * 1000 - gameState.start_time - note[0].GetComponent<SliderTimer>().clicked_timing) < gameState.perfect_plus_offset)
                    {
                        note[0].GetComponent<SliderTimer>().start_timing = Math.Abs(Time.time * 1000 - gameState.start_time - note[0].GetComponent<SliderTimer>().clicked_timing);
                        note[0].GetComponent<SliderTimer>().canRealse = true;
                        note[0].GetComponent<SliderTimer>().pressed = true;
                    }
                    if(note[0].GetComponent<SliderTimer>().pressed) {
                        note[0].transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(note[0].transform.GetChild(0).GetComponent<RectTransform>().offsetMin.x, -1300);

                        note[0].transform.GetChild(2).transform.position = new Vector2(note[0].transform.GetChild(2).transform.position.x, 170);
                        if(key == 4) {
                            hitLights4K[i].Play("hit");
                        }
                        else if(key == 7) {
                            hitLights7K[i].Play("hit");
                        }
                    }

                    if (Math.Abs(Time.time * 1000 - gameState.start_time - note[0].GetComponent<SliderTimer>().end_timing) < gameState.perfect_plus_offset || note[0].transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y  < 0)
                    {
                        note[0].GetComponent<SliderTimer>().finish_timing = Math.Abs(Time.time*1000-gameState.start_time-note[0].GetComponent<SliderTimer>().end_timing);
                        note[0].GetComponent<SliderTimer>().average = (note[0].GetComponent<SliderTimer>().start_timing+note[0].GetComponent<SliderTimer>().finish_timing)/2;

                        score(note);

                    }
                    yield return null;
                    continue;
                }
                
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

                    score(note);
                }
            }
            yield return new WaitForSeconds(0);
        }
    }

    void score(GameObject[] note)
    {
        if(StateController.mods["AT"] || note[0].GetComponent<SliderTimer>().average <= gameState.perfect_plus_offset) {
            scoreManager.perfect_plus++;
            scoreManager.combo++;
            Destroy(note[0]);
            acc_score_animation.SetTrigger("perfect+");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery;
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else if(note[0].GetComponent<SliderTimer>().average <= gameState.perfect_offset) {
            scoreManager.perfect++;
            scoreManager.combo++;
            Destroy(note[0]);
            acc_score_animation.SetTrigger("perfect");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery;
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else if(note[0].GetComponent<SliderTimer>().average <= gameState.great_offset) {
            scoreManager.great++;
            scoreManager.combo++;
            Destroy(note[0]);
            acc_score_animation.SetTrigger("great");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery*(200f/305f);
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else if(note[0].GetComponent<SliderTimer>().average <= gameState.good_offset) {
            scoreManager.good++;
            scoreManager.combo++;
            Destroy(note[0]);
            acc_score_animation.SetTrigger("good");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery*(100f/305f);
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else if(note[0].GetComponent<SliderTimer>().average <= gameState.ok_offset) {
            scoreManager.bad++;
            scoreManager.combo++;
            Destroy(note[0]);
            acc_score_animation.SetTrigger("ok");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery*(50f/305f);
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else {
            scoreManager.miss++;
            scoreManager.combo = 0;
            Destroy(note[0]);
            acc_score_animation.SetTrigger("miss");
            
            gameState.health -= gameState.base_damage;
            if(gameState.health < 0)  gameState.health = 0;
            
        }
    }
}
