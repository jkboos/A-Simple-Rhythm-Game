using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using ini_read_write;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine.Video;

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
    
    private int speed;
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

    void Start() {
        pause = false;
        gameover = false;
        speed = Int32.Parse(iniManager.ReadIniFile("settings", "speed", "3"));
        offset = Int32.Parse(iniManager.ReadIniFile("settings", "offset", "0"));
        time.GetComponent<TMP_Text>().autoSizeTextContainer = true;
        SetBackgroundImage(StateController.songs_path[StateController.cur_song_index]);
        StartCoroutine(delay());
        AudioManager.Instance.load_BGM(StateController.cur_song_index);

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
        if(isMusicStart && !is_settle && end_time < Time.time*1000 - start_time) { 
            is_settle = true;
            gameover = true;
            StartCoroutine(Settle());
        }
        else if (isMusicStart && !is_settle)
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

    IEnumerator delay() {
        StreamReader streamReader = new StreamReader(StateController.songs_path[StateController.cur_song_index]+"\\note.txt");
        float t = -1130f/((speed+4)*400f)*1000f+250f+offset;
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
            isStart = true;

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
