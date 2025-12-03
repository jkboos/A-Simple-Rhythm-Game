using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ini_read_write;
using UnityEngine.UIElements;

public class NoteTimer : MonoBehaviour
{
    public float clicked_timing = 0;
    public float timing = 0;
    private GameState gameState;
    private AudioSource audioSource;

    private bool canPress = false;

    void Start() {
        gameState = GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>();
        audioSource = GetComponent<AudioSource>();
        
    }

    void Update()
    {   
        timing = Math.Abs(Time.time*1000-gameState.start_time-clicked_timing);
        GameObject[] canPressNotes = GameObject.FindGameObjectsWithTag("canclick"+gameObject.transform.parent.name);
        GameObject[] canPressSliders = GameObject.FindGameObjectsWithTag("canpress"+gameObject.transform.parent.name);
        if(!canPress && Math.Abs(timing) <= 100 && GameObject.FindGameObjectWithTag(transform.parent.name).transform.GetChild(1).name == gameObject.name && canPressNotes.Length == 0 && canPressSliders.Length == 0) {
            canPress = true;
            gameObject.tag = "canclick"+gameObject.transform.parent.name;
        }
    }
}
