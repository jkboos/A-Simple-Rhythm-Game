using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class filesSearchResult
{
    public bool has_video;
    public bool has_storyboard;
    public string path;
}

public class BGManager : MonoBehaviour
{
    public class filesSearchResult
    {
        public bool has_video;
        public bool has_storyboard;
        public string path;
    }
    
    public static filesSearchResult fileSearch(string path)
    {
        filesSearchResult result = new filesSearchResult();
        
        DirectoryInfo direction = new DirectoryInfo(path);
        FileInfo[] files = direction.GetFiles("*.mp4");
        if (files.Length > 0)
        {
            result.has_video = true;
        }
        else
        {
            result.has_video = false;
        }

        direction = new DirectoryInfo(path);
        files = direction.GetFiles("*.osb");
        if (files.Length > 0)
        {
            result.has_storyboard = true;
        }
        else
        {
            result.has_storyboard = false;
        }

        DirectoryInfo directory = new DirectoryInfo(path);
        files = directory.GetFiles("*.jpg");
        if (files.Length == 0)
        {
            files = directory.GetFiles("*.png");
        }
        
        result.path = files[0].FullName;
        Debug.Log(files[0].FullName);

        return result;
    }
    public static IEnumerator LoadBG(string image_path, Image background) {
        
        
        string url = "file://" + image_path;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("背景載入失敗: " + uwr.error);
            }
            else
            {
                
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                
                Sprite spriteToUse = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
                
                background.sprite = spriteToUse;
                background.color = Color.white;
            }
        }
        
    }
    
}
