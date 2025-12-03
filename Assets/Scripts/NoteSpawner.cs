using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using Unity.Collections;
using ini_read_write;

public class NoteSpawner : MonoBehaviour
{
    [Header("Note")]
    public GameObject note;
    [Header("Slider")]
    public GameObject slider;
    public GameObject modeScene;

    private IniManager info = new IniManager(StateController.songs_path[StateController.cur_song_index]+"\\info.ini");
    private int key = 4;
    StreamReader streamReader;
    string str;
    string[] line;
    List<GameObject> notes = new List<GameObject>();
    public List<float> times = new List<float>();
    private int speed = 3;
    private float offset = 0;
    private IniManager iniManager = new IniManager(".\\settings.ini");
    public GameState gameState;
    public GameObject combo_text;

    public void Start() {
        key = Int32.Parse(info.ReadIniFile("info", "Key", "4"));
        if(key == 4) {
            modeScene.transform.GetChild(0).gameObject.SetActive(true);
            modeScene.transform.GetChild(1).gameObject.SetActive(false);
            combo_text.GetComponent<RectTransform>().position = new Vector3(893, 430, 0);
        }
        else if(key == 7) {
            modeScene.transform.GetChild(0).gameObject.SetActive(false);
            modeScene.transform.GetChild(1).gameObject.SetActive(true);
            combo_text.GetComponent<RectTransform>().position = new Vector3(705, 443.8f, 0);
        }

        speed = Int32.Parse(iniManager.ReadIniFile("settings", "speed", "3"));
        offset = float.Parse(iniManager.ReadIniFile("settings", "offset", "0"), CultureInfo.InvariantCulture.NumberFormat);
        

        float final_offset = -1130f/((speed+4)*400f)*1000f;
        Debug.Log(final_offset);

        streamReader = new StreamReader(StateController.songs_path[StateController.cur_song_index]+"\\note.txt");
        str = streamReader.ReadLine();
        int i = 0;
        while(str != null) {
            line = str.Split(",");
            GameObject n;
            GameObject parent = GameObject.FindGameObjectWithTag(line[0].ToString());
            
            if(line[2] == "0") {  
                n = Instantiate(note, parent.transform, parent);
                n.GetComponent<NoteTimer>().clicked_timing = float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat)-final_offset;
            }
            else {
                n = Instantiate(slider, parent.transform, parent);
                float t = float.Parse(line[3], CultureInfo.InvariantCulture.NumberFormat)-float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                float h = t/(Time.deltaTime*1000)*((speed+4)*400*Time.deltaTime);
                GameObject center = n.transform.GetChild(0).gameObject;
                GameObject top = n.transform.GetChild(1).gameObject;
                GameObject bottom = n.transform.GetChild(2).gameObject;
                center.GetComponent<RectTransform>().sizeDelta = new Vector2(center.GetComponent<RectTransform>().sizeDelta.x, h);
                center.transform.position = new Vector2(center.transform.position.x, bottom.transform.position.y+center.GetComponent<RectTransform>().sizeDelta.y/2+33);
                center.GetComponent<RectTransform>().offsetMax = new Vector2(center.GetComponent<RectTransform>().offsetMax.x, center.GetComponent<RectTransform>().offsetMax.y-bottom.GetComponent<RectTransform>().sizeDelta.y+24);
                top.transform.position = new Vector2(top.transform.position.x, center.transform.position.y+center.GetComponent<RectTransform>().sizeDelta.y/2+32);

                n.GetComponent<SliderTimer>().clicked_timing = float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat)-final_offset;
                n.GetComponent<SliderTimer>().end_timing = float.Parse(line[3], CultureInfo.InvariantCulture.NumberFormat)-final_offset;
            }
            n.transform.position = new Vector3(parent.transform.position.x, 1300, parent.transform.position.z);
            n.name += i.ToString();
            notes.Add(n);
            times.Add((float)Math.Round(float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat)));
            n.SetActive(false);
            str = streamReader.ReadLine();

            StartCoroutine(active(i));
            i++;
        }
        gameState.note_amount = i;
    }

    IEnumerator active(int index) {
        while(!notes[index].activeSelf) {
            if(GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>().isStart && Time.time*1000-GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>().start_time >= times[index]) {
                notes[index].SetActive(true);
            }
            yield return new WaitForSeconds(0.001f);
        }
    }
}
