using System;
using ini_read_write;
using UnityEngine;

public class AutoNote : MonoBehaviour
{
    public ScoreManager scoreManager;
    public Animator acc_score_animation;
    
    public Animator[] hitLights4K =  new Animator[4];
    public Animator[] hitLights7K =   new Animator[7];
    
    private IniManager info = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\info.ini");
    public GameState gameState;

    private int key = 4;
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
        if(GetComponent<NoteTimer>().timing <= 0)
        {
            if(key == 4) {
                hitLights4K[Int32.Parse(transform.parent.name)].Play("hit");
            }
            else if(key == 7) {
                hitLights7K[Int32.Parse(transform.parent.name)].Play("hit");
            }
            score();
                    
        }
        
    }
    
    void score()
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
}
