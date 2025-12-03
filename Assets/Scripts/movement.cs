using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using System;

public class movement : MonoBehaviour
{  
    private int speed = 3;
    private IniManager iniManager = new IniManager(".\\settings.ini");
    public Animator miss;
    private ScoreManager scoreManager;
    private GameState gameState;

    void Start() {
        speed = Int32.Parse(iniManager.ReadIniFile("settings", "speed", "3"));
        miss = GameObject.FindGameObjectWithTag("miss").GetComponent<Animator>();
        scoreManager = GameObject.FindGameObjectWithTag("scoremanager").GetComponent<ScoreManager>();
        gameState = GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.tag.Contains("canclick") || (gameObject.tag != "slider" && !gameObject.tag.Contains("canpress"))) {
            transform.position = new Vector3(transform.parent.position.x, transform.position.y-400*(speed+4)*Time.deltaTime, transform.position.z);
        }
        else if(gameObject.tag.Contains("canpress") || gameObject.tag == "slider") {
            for(int i = 0; i< 3; i++) {
                GameObject part = gameObject.transform.GetChild(i).gameObject;
                if(gameObject.transform.GetChild(1).gameObject.transform.position.y <= gameObject.transform.GetChild(2).gameObject.transform.position.y && i == 1) {
                    continue;
                }
                part.transform.position = new Vector3(part.transform.parent.position.x, part.transform.position.y-400*(speed+4)*Time.deltaTime, part.transform.position.z);
            }
            
        }
        if((gameObject.tag.Contains("canclick") || (gameObject.tag != "slider" && !gameObject.tag.Contains("canpress"))) && Time.time*1000-gameState.start_time-gameObject.GetComponent<NoteTimer>().clicked_timing > 100) {
            miss.Play("score");
            scoreManager.miss++;
            scoreManager.combo = 0;
            Destroy(gameObject);
        }
        else if((gameObject.tag.Contains("canpress") || gameObject.tag == "slider") && !gameObject.GetComponent<SliderTimer>().pressed && Time.time*1000-gameState.start_time-gameObject.GetComponent<SliderTimer>().clicked_timing > 100) {
            miss.Play("score");
            scoreManager.miss++;
            scoreManager.combo = 0;
            Destroy(gameObject);
        }
    }
}
