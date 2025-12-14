using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using ini_read_write;
using TMPro;
using UnityEngine.Video;
using DG.Tweening;

public class GameState : MonoBehaviour
{

    public float start_time = 0;
    public bool isStart = false;
    public Image background;
    public GameObject blackMask;
    public VideoPlayer video;
    public GameObject modeScene;
    public ScoreManager scoreManager;
    public GameObject progressbar;
    public GameObject time;

    private IniManager iniManager = new IniManager(".\\settings.ini");
    
    private float speed;
    private int offset;
    public int note_amount = 0;
    private bool isMusicStart = false;

    public static bool pause = false;
    public static bool gameover = false;
    StreamReader streamReader;
    private float end_time;
    private bool is_settle = false;
    public Storyboard storyboard;
    public Animator loading;
    public GameObject particle;
    public GameObject combo_text;
    public GameObject acc_score;
    public GameObject health_bar;
    private Image health_bar_image;

    public float health_dotween = 0;
    public float MAX_HEALTH = 100;
    public float health = 100;
    public float base_damage = 5;
    public float base_recovery = 0.5f;

    public float perfect_plus_offset = 20;
    public float perfect_offset = 40;
    public float great_offset = 60;
    public float good_offset = 70;
    public float ok_offset = 80;

    public float miss_offset = 100;

    public bool is_death = false;

    public GameObject death_transition;

    void Start() {
        health =  MAX_HEALTH;
        pause = false;
        gameover = false;
        speed = float.Parse(iniManager.ReadIniFile("settings", "speed", "3"));
        offset = Int32.Parse(iniManager.ReadIniFile("settings", "offset", "0"));
        time.GetComponent<TMP_Text>().autoSizeTextContainer = true;
        SetBackgroundImage(StateController.songs_path[StateController.cur_song_index]);
        StartCoroutine(delay());
        AudioManager.Instance.load_BGM(StateController.cur_song_index);

        GameObject stage_center = GameObject.FindGameObjectWithTag("stage-center");
        combo_text.transform.position = new Vector3(stage_center.transform.position.x, combo_text.transform.position.y, combo_text.transform.position.z);
        acc_score.transform.position = new Vector3(stage_center.transform.position.x, acc_score.transform.position.y, acc_score.transform.position.z);
        health_bar.transform.position = new Vector3(stage_center.transform.position.x + stage_center.GetComponent<RectTransform>().sizeDelta.x / 2 + health_bar.GetComponent<RectTransform>().sizeDelta.y / 2 + 50, Screen.height / 2, health_bar.transform.position.z);
        health_bar_image = health_bar.transform.GetChild(0).GetComponent<Image>();
        
        updateHealth();

        float max = 0;
        StreamReader reader = new StreamReader(StateController.cur_song_path + "\\note.txt");
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] s = line.Split(',');
            if (s.Length == 3 && float.Parse(s[1]) > max)
            {
                max = float.Parse(s[1]);
            }
            else if(s.Length == 4 && float.Parse(s[3]) > max)
            {
                max = float.Parse(s[3]);
            }
        }

        end_time = max;
        
        Debug.Log("end_time: "+end_time);
    }

    void Update() {
        health_bar_image.fillAmount = health_dotween / 100f;

        if (!StateController.mods["NF"] && health <= 0 && !is_death)
        {
            is_death = true;
            gameover = true;
            Time.timeScale = 0;
            death_transition.SetActive(true);
            death_transition.GetComponent<Animator>().SetTrigger("death");
            AudioManager.Instance.stop_BGM();
            AudioManager.Instance.playSFX(StateController.death_sound);
        }
        
        if(isStart && !is_settle && end_time < Time.time*1000 - start_time) { 
            is_settle = true;
            gameover = true;
            StartCoroutine(Settle());
        }
        else if (isStart && !is_settle)
        {
            progressbar.GetComponent<RectTransform>().offsetMax = new Vector2(
                -Screen.width + Screen.width * ((Time.time * 1000 - start_time) / end_time),
                progressbar.GetComponent<RectTransform>().offsetMax.y);
            time.transform.position = new Vector3(Screen.width+progressbar.GetComponent<RectTransform>().offsetMax.x-time.GetComponent<RectTransform>().rect.width/2, time.transform.position.y, 0);
            int remain_time = (int)((end_time - (Time.time*1000 - start_time)) / 1000);
            string second = $"{remain_time % 60}";
            string minute = $"{remain_time / 60}";
            minute = minute.PadLeft(2, '0');
            second = second.PadLeft(2, '0');
            time.GetComponent<TMP_Text>().text = $"{minute}:{second}";
        }
    }

    IEnumerator Settle() {
        yield return new WaitForSeconds(1.5f);
        modeScene.transform.GetChild(0).gameObject.SetActive(false);
        modeScene.transform.GetChild(1).gameObject.SetActive(false);
        modeScene.transform.GetChild(2).gameObject.SetActive(false);
        modeScene.transform.GetChild(3).gameObject.SetActive(true);

        scoreManager.SettleScore();
    }
    
    void updateHealth() {
        DOVirtual.Float(health_dotween, health, 0.2f, (x) => {
            health_dotween = x;
        }).OnComplete(() => {
            updateHealth();
        });
    }

    IEnumerator delay() {
        StreamReader streamReader = new StreamReader(StateController.songs_path[StateController.cur_song_index]+"\\note.txt");
        float t = -1130f/((speed)*400f)*1000f+250f+offset;
        t /= 1000f;
        // Debug.Log(t);
        while (!storyboard.is_loaded)
        {
            Debug.Log("Waiting for storyboard");
            yield return new WaitForSeconds(1f);
        }
        loading.SetTrigger("fadeout");
        
        if(t < 0) {  
            yield return new WaitForSeconds(3+t);
            start_time = (float)Math.Round(Time.time*1000);
            isStart = true;
            yield return new WaitForSeconds(-t);
            // GetComponent<GameMusicLoader>().PlayMusic();
            if (!background.gameObject.activeSelf)
            {
                video.gameObject.SetActive(true);
                particle.SetActive(false);
                blackMask.GetComponent<Animator>().SetTrigger("fadeOut");
            }
            AudioManager.Instance.resume_BGM();
            isMusicStart = true;
            KeyEvent.can_pause = true;
            

        }
        else {
            yield return new WaitForSeconds(3);
            // GetComponent<GameMusicLoader>().PlayMusic();
            if (!background.gameObject.activeSelf)
            {
                video.gameObject.SetActive(true);
                particle.SetActive(false);
            }
            AudioManager.Instance.resume_BGM();
            isMusicStart = true;
            KeyEvent.can_pause = true;
            yield return new WaitForSeconds(t);
            start_time = (float)Math.Round(Time.time*1000);
            isStart = true;
        }
    }

    Sprite ImageToSprite(string path) {
        string filePath = path;
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        Rect rec = new Rect(0, 0, tex.width, tex.height);
        Sprite spriteToUse = Sprite.Create(tex,rec,new Vector2(0.5f,0.5f),100);

        return spriteToUse;
    }

    void SetBackgroundImage(string image_path) {
        DirectoryInfo directory = new DirectoryInfo(image_path);
        FileInfo[] files = directory.GetFiles("*.mp4");
        
        if(files.Length == 0) {
            files = directory.GetFiles("*.png");
        }

        if (files.Length == 0)
        {
            files = directory.GetFiles("*.jpg");
        }
        Debug.Log(files[0].FullName);

        if (files[0].FullName.EndsWith(".png") || files[0].FullName.EndsWith(".jpg"))
        {
            video.gameObject.SetActive(false);
            background.gameObject.SetActive(true);
            blackMask.SetActive(false);
            background.sprite = ImageToSprite(files[0].FullName);
        }
        else
        {
            video.gameObject.SetActive(false);
            background.gameObject.SetActive(false);
            blackMask.SetActive(true);
            video.url = files[0].FullName;
        }
    }

}
