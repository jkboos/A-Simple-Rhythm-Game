using AirFishLab.ScrollingList;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using ini_read_write;
using Unity.VisualScripting;
using UnityEngine.Events;

public class ListEvent : MonoBehaviour
{
    [SerializeField]
    private CircularScrollingList _list;

    public SelectMusicLoader musicLoader;
    public Image background;
    public Score score;
    public TMP_Text time;
    public TMP_Text key;
    public GameObject info_panel;
    public bool canEnter = true;
    public GameObject canvas;
    public UnityEvent<GameObject, Action> fadeOut;
    public Image video_icon;
    public Image storyboard_icon;

    public void OnBoxSelected(ListBox listBox) {
        int index = Int32.Parse(listBox.name.Replace("SongButton (", "").Replace(")", ""));
        if(index == StateController.cur_song_index && canEnter)
        {
            fadeOut.Invoke(canvas, () =>
            {
                AudioManager.Instance.stop_BGM();
                SceneManager.LoadScene("PlayScene");
            });
        }
        
    }

    public void OnFocusingChanged(ListBox last_box, ListBox cur_box) {
        if(last_box != null && cur_box != null) {
            last_box.GetComponent<Image>().color = new Color(255, 255, 255);
            cur_box.GetComponent<Image>().color = new Color(140, 0, 255);
        }
    }

    public void OnMovementEnd()
    {
        StartCoroutine(ChangeSong());
    }

    IEnumerator ChangeSong()
    {
        int index = Int32.Parse(_list.GetFocusingBox().name.Replace("SongButton (", "").Replace(")", ""));
        if(index < StateController.songs_path.Length) {
            canEnter = true;
            StateController.cur_song_index = index;
            StateController.cur_song_path = StateController.songs_path[index];
            
            // musicLoader.PlayMusic(StateController.cur_song_index);
            if(!StateController.list_box_init)
            {
                AudioManager.Instance.play_BGM(StateController.cur_song_index);
            }
            else
            {
                StateController.list_box_init = false;
            }
            Task<BGManager.filesSearchResult> fileSearchTask = Task.Run(() =>
            {
                return BGManager.fileSearch(StateController.cur_song_path);
            });
            
            yield return new WaitUntil(() => fileSearchTask.IsCompleted);
            BGManager.filesSearchResult result = fileSearchTask.Result;
            
            if (result.has_video)
            {
                video_icon.color = Color.green;
            }
            else
            {
                video_icon.color = Color.red;
            }

            if (result.has_storyboard)
            {
                storyboard_icon.color = Color.green;
            }
            else
            {
                storyboard_icon.color = Color.red;
            }
            
            yield return StartCoroutine(BGManager.LoadBG(result.path, background));
            score.LoadScore();
            score.LoadTimeAndKey();
        }
        else {
            // musicLoader.StopMusic();
            AudioManager.Instance.stop_BGM();
            yield return StartCoroutine(BGManager.LoadBG("NotFound.png", background));
            time.text = "00:00";
            key.text = "0k";
            canEnter = false;
            info_panel.transform.GetChild(2).gameObject.SetActive(false);
            info_panel.transform.GetChild(3).gameObject.SetActive(true);
        }
        
        yield return null;
    }
}