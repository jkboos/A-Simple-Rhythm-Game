using System;
using ini_read_write;
using UnityEngine;

public class AutoSlider : MonoBehaviour
{
    public ScoreManager scoreManager;
    public Animator acc_score_animation;
    
    public Animator[] hitLights4K =  new Animator[4];
    public Animator[] hitLights7K =   new Animator[7];
    
    private IniManager info = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\info.ini");
    public GameState gameState;

    private int key = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        key = Int32.Parse(info.ReadIniFile("info", "Key", "4"));
        
        if(key == 4){
            GameObject[] lights = GameObject.FindGameObjectsWithTag("4k-hit-light");
            Array.Sort(lights, (a, b) => a.name.CompareTo(b.name));
            for (int i = 0; i < hitLights4K.Length; i++)
            {
                hitLights4K[i] = lights[i].GetComponent<Animator>();
            }
        }
        else if (key == 7)
        {
            GameObject[] lights = GameObject.FindGameObjectsWithTag("7k-hit-light");
            Array.Sort(lights, (a, b) => a.name.CompareTo(b.name));
            for (int i = 0; i < hitLights7K.Length; i++)
            {
                hitLights7K[i] = lights[i].GetComponent<Animator>();
            }
        }

        gameState = GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>();
        acc_score_animation = GameObject.FindGameObjectWithTag("acc-score").GetComponent<Animator>();
        scoreManager = GameObject.FindGameObjectWithTag("scoremanager").GetComponent<ScoreManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(StateController.mods["AT"])
        {
            if (GetComponent<SliderTimer>().clicked_timing - (Time.time * 1000 - gameState.start_time) < gameState.perfect_plus_offset)
            {
                GetComponent<SliderTimer>().start_timing = Math.Abs(Time.time * 1000 - gameState.start_time - GetComponent<SliderTimer>().clicked_timing);
                GetComponent<SliderTimer>().canRealse = true;
                GetComponent<SliderTimer>().pressed = true;
            }
            if(GetComponent<SliderTimer>().pressed) {
                transform.GetChild(0).GetComponent<RectTransform>().offsetMin = new Vector2(transform.GetChild(0).GetComponent<RectTransform>().offsetMin.x, -1300);

                transform.GetChild(2).transform.position = new Vector2(transform.GetChild(2).transform.position.x, 170);
                if(key == 4) {
                    hitLights4K[Int32.Parse(transform.parent.name)].Play("hit");
                }
                else if(key == 7) {
                    hitLights7K[Int32.Parse(transform.parent.name)].Play("hit");
                }
            }

            if (GetComponent<SliderTimer>().end_timing - (Time.time * 1000 - gameState.start_time) < gameState.perfect_plus_offset)
            {
                GetComponent<SliderTimer>().finish_timing = Math.Abs(Time.time*1000-gameState.start_time-GetComponent<SliderTimer>().end_timing);
                GetComponent<SliderTimer>().average = (GetComponent<SliderTimer>().start_timing+GetComponent<SliderTimer>().finish_timing)/2;

                score();

            }
        }
        void score()
    {
        if(GetComponent<SliderTimer>().average <= gameState.perfect_plus_offset) {
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
        else if(GetComponent<SliderTimer>().average <= gameState.perfect_offset) {
            scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(300f/305f));
            scoreManager.perfect++;
            scoreManager.combo++;
            Destroy(gameObject);
            acc_score_animation.SetTrigger("perfect");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery;
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else if(GetComponent<SliderTimer>().average <= gameState.great_offset) {
            scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(200f/305f));
            scoreManager.great++;
            scoreManager.combo++;
            Destroy(gameObject);
            acc_score_animation.SetTrigger("great");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery*(200f/305f);
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else if(GetComponent<SliderTimer>().average <= gameState.good_offset) {
            scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(100f/305f));
            scoreManager.good++;
            scoreManager.combo++;
            Destroy(gameObject);
            acc_score_animation.SetTrigger("good");
            
            if (gameState.health < gameState.MAX_HEALTH)
            {
                gameState.health += gameState.base_recovery*(100f/305f);
                if(gameState.health > gameState.MAX_HEALTH)  gameState.health = gameState.MAX_HEALTH;
            }
        }
        else if(GetComponent<SliderTimer>().average <= gameState.ok_offset) {
            scoreManager.score += (int)(scoreManager.MAX_SCORE/gameState.note_amount*(50f/305f));
            scoreManager.bad++;
            scoreManager.combo++;
            Destroy(gameObject);
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
            Destroy(gameObject);
            acc_score_animation.SetTrigger("miss");
            
            gameState.health -= gameState.base_damage;
            if(gameState.health < 0)  gameState.health = 0;
            
        }
    }
    }
}
