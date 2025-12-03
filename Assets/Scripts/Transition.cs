using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float fade_speed = 0.02f;
    
    
    
    public void fadeOut(GameObject obj, Action callback = null)
    {
        StartCoroutine(_fadeOut(obj, callback));
    }

    public void fadeIn(GameObject obj, Action callback = null)
    {
        StartCoroutine( _fadeIn(obj, callback));
    }
        
        
        
    private IEnumerator _fadeOut(GameObject obj, Action callback = null)
    {
        GameObject mask = new GameObject("fadeout-mask");
        mask.tag = "fadeout-mask";
        mask.transform.parent = obj.transform;
        
        mask.AddComponent<RectTransform>();
        mask.AddComponent<Image>();
        
        mask.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        mask.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        mask.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        while (mask.GetComponent<Image>().color.a <= 1)
        {
            mask.GetComponent<Image>().color =
                new Color(0, 0, 0, mask.GetComponent<Image>().color.a + fade_speed);
            yield return new WaitForSecondsRealtime(0.001f);
        }
        
        // Destroy(mask);
        callback?.Invoke();
    }

    private IEnumerator _fadeIn(GameObject obj, Action callback = null)
    {
        GameObject fadeout_mask = GameObject.FindGameObjectWithTag("fadeout-mask");
        if (fadeout_mask)
        {
            Destroy(fadeout_mask);
        }
        
        
        GameObject mask = new GameObject("fadein-mask");
        mask.tag = "fadein-mask";
        mask.transform.parent = obj.transform;
        
        mask.AddComponent<RectTransform>();
        mask.AddComponent<Image>();
        
        mask.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        mask.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        mask.GetComponent<Image>().color = new Color(0, 0, 0, 1);

        while (mask.GetComponent<Image>().color.a > 0)
        {
            mask.GetComponent<Image>().color =
                new Color(0, 0, 0, mask.GetComponent<Image>().color.a - fade_speed);
            yield return new WaitForSecondsRealtime(0.001f);
        }
        
        Destroy(mask);
        callback?.Invoke();
    }
}
