using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BGManager : MonoBehaviour
{
    static Sprite ImageToSprite(string path) {
        string filePath = path;
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        Rect rec = new Rect(0, 0, tex.width, tex.height);
        Sprite spriteToUse = Sprite.Create(tex,rec,new Vector2(0.5f,0.5f),100);

        return spriteToUse;
    }

    public static void SetBackgroundImage(string image_path, Image background) {
        DirectoryInfo directory = new DirectoryInfo(image_path);
        FileInfo[] files = directory.GetFiles("*.jpg");
        if(files.Length == 0) {
            files = directory.GetFiles("*.png");
        }
        Debug.Log(files[0].FullName);
        background.sprite = ImageToSprite(files[0].FullName);
    }
}
