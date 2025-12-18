using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;

public class movement : MonoBehaviour
{  
    private float speed = 3;
    private IniManager iniManager = new IniManager(".\\settings.ini");
    public Animator acc_score_animation;
    private ScoreManager scoreManager;
    private GameState gameState;

    void Start() {
        speed = float.Parse(iniManager.ReadIniFile("settings", "speed", "3"));
        scoreManager = GameObject.FindGameObjectWithTag("scoremanager").GetComponent<ScoreManager>();
        gameState = GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>();
        acc_score_animation = GameObject.FindGameObjectWithTag("acc-score").GetComponent<Animator>();
        speed /= gameState.speed_multiplier;
    }
    
    void Update()
    {
        if(gameObject.tag.Contains("canclick") || (gameObject.tag != "slider" && !gameObject.tag.Contains("canpress")))
        {
            transform.position = new Vector3(transform.parent.position.x, transform.position.y-400*(speed)*Time.deltaTime, transform.position.z);
        }
        else if(gameObject.tag.Contains("canpress") || gameObject.tag == "slider") {
            for(int i = 0; i< 3; i++) {
                GameObject part = gameObject.transform.GetChild(i).gameObject;
                if(gameObject.transform.GetChild(1).gameObject.transform.position.y <= gameObject.transform.GetChild(2).gameObject.transform.position.y && i == 1) {
                    continue;
                }
                part.transform.position = new Vector3(part.transform.parent.position.x, part.transform.position.y-400*(speed)*Time.deltaTime, part.transform.position.z);
            }
            
        }
        if((gameObject.tag.Contains("canclick") || (gameObject.tag != "slider" && !gameObject.tag.Contains("canpress"))) && Time.time*1000-gameState.start_time-gameObject.GetComponent<NoteTimer>().clicked_timing > gameState.miss_offset) {

            if (StateController.mods["AT"])
            {
                scoreManager.perfect_plus++;
                scoreManager.combo++;
                Destroy(gameObject);
                acc_score_animation.SetTrigger("perfect+");

                if (gameState.health < gameState.MAX_HEALTH)
                {
                    gameState.health += gameState.base_recovery;
                    if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
                }
            }
            else
            {
                acc_score_animation.SetTrigger("miss");
                scoreManager.miss++;
                scoreManager.combo = 0;
                Destroy(gameObject);

                gameState.health -= gameState.base_damage;
                if (gameState.health < 0) gameState.health = 0;
            }
        }
        else if((gameObject.tag.Contains("canpress") || gameObject.tag == "slider") && !gameObject.GetComponent<SliderTimer>().pressed && Time.time*1000-gameState.start_time-gameObject.GetComponent<SliderTimer>().clicked_timing > gameState.miss_offset) {
            if (StateController.mods["AT"])
            {
                scoreManager.score += scoreManager.MAX_SCORE/gameState.note_amount;
                scoreManager.perfect_plus++;
                scoreManager.combo++;
                Destroy(gameObject);
                acc_score_animation.SetTrigger("perfect+");

                if (gameState.health < gameState.MAX_HEALTH)
                {
                    gameState.health += gameState.base_recovery;
                    if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
                }
            }
            else
            {
                acc_score_animation.SetTrigger("miss");
                scoreManager.miss++;
                scoreManager.combo = 0;
                Destroy(gameObject);

                gameState.health -= gameState.base_damage;
                if (gameState.health < 0) gameState.health = 0;
            }
        }
    }
}
