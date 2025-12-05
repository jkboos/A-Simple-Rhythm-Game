
using UnityEngine;
using ini_read_write;
using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class KeyEvent : MonoBehaviour
{
    private IniManager settings = new IniManager(".\\settings.ini");
    private IniManager info = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\info.ini");
    public bool[] KeyDownEvents;
    public bool[] KeyUpEvents;
    public bool[] KeyEvents;
    private int key = 4;
    public Animator[] lights4K;
    public Animator[] lights7K;
    
    // public AudioSource audioSource;
    public static bool can_pause =  false;
    public GameObject pause_canvas;
    public UnityEvent<GameObject, Action> fadeout;
    public UnityEvent<GameObject, Action> fadein;
    public Animator pause_animator;
    public Animator warning;

    private bool is_fadeOut = false;
    
    public GameState gameController;

    // Start is called before the first frame update
    void Start()
    {
        can_pause = false;
        key = Int32.Parse(info.ReadIniFile("info", "Key", "4"));
        KeyDownEvents = new bool[7];
        KeyUpEvents = new bool[7];
        KeyEvents = new bool[7];
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i< key; i++) {
            KeyDownEvents[i] = Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), settings.ReadIniFile(key+"k", "key"+i, "D")));
            KeyUpEvents[i] = Input.GetKeyUp((KeyCode)System.Enum.Parse(typeof(KeyCode), settings.ReadIniFile(key+"k", "key"+i, "D")));
            KeyEvents[i] = Input.GetKey((KeyCode)System.Enum.Parse(typeof(KeyCode), settings.ReadIniFile(key+"k", "key"+i, "D")));

            if(KeyDownEvents[i]) {
                if(key == 4) {
                    lights4K[i].Play("lighton");
                }
                else if(key == 7) {
                    lights7K[i].Play("lighton");
                }
            }
            if(KeyUpEvents[i]) {
                if(key == 4) {
                    lights4K[i].Play("lightoff");
                }
                else if(key == 7) {
                    lights7K[i].Play("lightoff");
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(!is_fadeOut && GameState.pause && !GameState.gameover)
            {
                is_fadeOut = true;
                StartCoroutine(FadeOut());
            }
            else if(can_pause && !GameState.pause && !GameState.gameover)
            {
            
                can_pause = false;
                Time.timeScale = 0;
                GameState.pause = true;
                // audioSource.Pause();
                AudioManager.Instance.pause_BGM();
                pause_canvas.SetActive(true);
                pause_animator.SetTrigger("fadeIn");
            }
            else if (!can_pause && !GameState.pause && !GameState.gameover && gameController.isStart)
            {
                warning.SetTrigger("warn");
            }
            else if (GameState.gameover)
            {
                Time.timeScale = 1;
                SceneManager.LoadScene("Scenes/SelectSong");
            }
        }
    }

    public void _fadeout()
    {
        if(!is_fadeOut && GameState.pause && !GameState.gameover)
        {
            is_fadeOut = true;
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        
        pause_animator.SetTrigger("fadeOut");
        yield return new WaitForSecondsRealtime(0.5f);
        pause_canvas.SetActive(false);
        GameState.pause = false;
        // audioSource.Play();
        
        AudioManager.Instance.resume_BGM();
        is_fadeOut = false;
        Time.timeScale = 1;
        yield return new WaitForSecondsRealtime(3f);
        can_pause = true;
    }
}
