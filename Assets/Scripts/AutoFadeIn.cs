using System;
using UnityEngine;
using UnityEngine.Events;

public class AutoFadeIn : MonoBehaviour
{
    public GameObject canvas;
    public UnityEvent<GameObject, Action> fadein;
    void Start()
    {
        fadein.Invoke(canvas, null);
    }
}
