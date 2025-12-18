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
        if (!StateController.mods["AT"])
        {
            for (int i = 0; i < key; i++)
            {
                StartCoroutine(KeyEvent(i));
            }
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
                    
                    score(note);
                }
            }
            yield return null;
        }
    }

    void score(GameObject[] note)
    {
        if(StateController.mods["AT"] || note[0].GetComponent<NoteTimer>().timing <= gameState.perfect_plus_offset) {
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
        else if(note[0].GetComponent<NoteTimer>().timing <= gameState.perfect_offset) {
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
        else if(note[0].GetComponent<NoteTimer>().timing <= gameState.great_offset) {
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
        else if(note[0].GetComponent<NoteTimer>().timing <= gameState.good_offset) {
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
        else if(note[0].GetComponent<NoteTimer>().timing <= gameState.ok_offset) {
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
