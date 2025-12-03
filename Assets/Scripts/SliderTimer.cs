using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements.Experimental;

public class SliderTimer : MonoBehaviour
{
    public float clicked_timing = 0;
    public float end_timing = 0;
    private GameState gameState;
    private AudioSource audioSource;
    private bool canPress = false;
    public float start_timing = -1;
    public float finish_timing = -1;
    public float average = 0;
    private float timing = 0;
    public bool pressed = false;
    public bool canRealse = false;
    void Start() {
        gameState = GameObject.FindGameObjectWithTag("gamecontroller").GetComponent<GameState>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        timing = Math.Abs(Time.time*1000-gameState.start_time-clicked_timing);
        GameObject[] canPressNotes = GameObject.FindGameObjectsWithTag("canclick"+gameObject.transform.parent.name);
        GameObject[] canPressSliders = GameObject.FindGameObjectsWithTag("canpress"+gameObject.transform.parent.name);
        if(!canPress && Math.Abs(timing) <= 100 && GameObject.FindGameObjectWithTag(transform.parent.name).transform.GetChild(1).name == gameObject.name && canPressSliders.Length == 0 && canPressNotes.Length == 0) {
            canPress = true;
            gameObject.tag = "canpress"+gameObject.transform.parent.name;
        }
    }
}
