using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

public class Storyboard : MonoBehaviour
{
    [Header("File Settings")]
    public string osbFilePath;      
    public string assetRootFolder = "SB"; 
    
    [Header("References")]
    public Material additiveMaterial; // 請拖入 UI Additive Material

    [Header("Canvas Layers")]
    public Transform layerBackground;
    public Transform layerFail;
    public Transform layerPass;
    public Transform layerForeground;
    public Transform layerOverlay;

    // --- 1080p 轉換參數 ---
    private float globalScale; 
    private float xOffset;     
    private const float TARGET_HEIGHT = 1080f;
    private const float TARGET_WIDTH = 1920f;
    

    // --- 內部狀態 ---
    private List<StoryboardSprite> activeSprites = new List<StoryboardSprite>();
    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
    private List<StorySample> sampleEvents = new List<StorySample>();
    private AudioSource sfxSource;
    private Dictionary<string, string> variables = new Dictionary<string, string>();
    private string baseDirectory;
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    public GameState gameController;
    public bool is_loaded = false;
    public GameObject background;
    public GameObject particle;

    void Start()
    {

        // 計算縮放
        globalScale = TARGET_HEIGHT / 480f; 
        float contentWidth = 640f * globalScale;
        xOffset = (TARGET_WIDTH - contentWidth) / 2f;

        Debug.Log($"[Storyboard] Init. Scale: {globalScale}, X-Offset: {xOffset}");

        GameObject sfxGo = new GameObject("SfxPlayer");
        sfxGo.transform.SetParent(transform);
        sfxSource = sfxGo.AddComponent<AudioSource>();
        
        DirectoryInfo direction = new DirectoryInfo(StateController.cur_song_path);
        FileInfo[] files = direction.GetFiles("*.osb");
        if (files.Length == 0)
        {
            is_loaded = true;
            gameObject.SetActive(false);
            return;
        }

        osbFilePath = files[0].FullName;

        if (File.Exists(osbFilePath))
        {
            baseDirectory = Path.GetDirectoryName(osbFilePath);
            if (!string.IsNullOrEmpty(assetRootFolder))
                baseDirectory = Path.Combine(baseDirectory, assetRootFolder);

            StartCoroutine(ParseRoutine());
        }
        else
        {
            Debug.LogError($"[Storyboard] FileNotFound: {osbFilePath}");
        }
    }

    void Update()
    {
        if (gameController.isStart)
        {
            float time = (Time.time * 1000f - gameController.start_time);

            foreach (var sprite in activeSprites) sprite.UpdateAnimation(time);
            UpdateSamples(time);
        }
    }

    IEnumerator ParseRoutine()
    {
        string[] rawLines = File.ReadAllLines(osbFilePath);
        Debug.Log($"[Storyboard] Parsing {rawLines.Length} lines...");

        bool inVars = false;
        foreach(var line in rawLines) {
            string t = line.Trim();
            if(t == "[Variables]") inVars = true;
            else if(t.StartsWith("[")) inVars = false;
            else if(inVars && t.StartsWith("$")) {
                var p = t.Split('=');
                if(p.Length==2) variables[p[0].Trim()] = p[1].Trim();
            }
        }

        StoryboardSprite currentSprite = null;
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < rawLines.Length; i++)
        {
            if (stopwatch.ElapsedMilliseconds > 15) { yield return null; stopwatch.Restart(); }

            try 
            {
                string line = rawLines[i];
                string trimmed = line.Trim().TrimStart('_'); 
                foreach (var kvp in variables) 
                    if (trimmed.Contains(kvp.Key)) trimmed = trimmed.Replace(kvp.Key, kvp.Value);

                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("[")) continue;
                string[] parts = trimmed.Split(',');

                int indent = 0;
                if (line.StartsWith("  ") || line.StartsWith("__")) indent = 2;
                else if (line.StartsWith(" ") || line.StartsWith("_")) indent = 1;

                if (indent == 0)
                {
                    if (trimmed.StartsWith("Sprite"))
                    {
                        if (parts.Length < 6) continue;
                        string layerName = parts[1];
                        string origin = parts[2];
                        string path = CleanPath(parts[3]);
                        if (!float.TryParse(parts[4], out float x)) x = 320;
                        if (!float.TryParse(parts[5], out float y)) y = 240;
                        
                        Transform parent = layerForeground;
                        if (layerName == "Background" && layerBackground != null) parent = layerBackground;
                        else if (layerName == "Fail" && layerFail != null) parent = layerFail;
                        else if (layerName == "Pass" && layerPass != null) parent = layerPass;
                        else if (layerName == "Overlay" && layerOverlay != null) parent = layerOverlay;

                        currentSprite = CreateSprite(path, parent, origin, x, y);
                    }
                    else if (trimmed.StartsWith("Animation")) // <--- 新增這裡
                    {
                        // 格式: Animation, Layer, Origin, Path, X, Y, FrameCount, FrameDelay, LoopType
                        if (parts.Length < 9) continue;
                        string layerName = parts[1];
                        string origin = parts[2];
                        string path = CleanPath(parts[3]);
                        if (!float.TryParse(parts[4], out float x)) x = 320;
                        if (!float.TryParse(parts[5], out float y)) y = 240;
                        int frameCount = int.Parse(parts[6]);
                        double frameDelay = double.Parse(parts[7]);
                        string loopType = parts[8]; // LoopForever or LoopOnce

                        Transform parent = layerForeground;
                        if (layerName == "Background" && layerBackground != null) parent = layerBackground;
                        else if (layerName == "Fail" && layerFail != null) parent = layerFail;
                        else if (layerName == "Pass" && layerPass != null) parent = layerPass;
                        else if (layerName == "Overlay" && layerOverlay != null) parent = layerOverlay;

                        // 呼叫建立動畫的新函式
                        currentSprite = CreateAnimation(path, parent, origin, x, y, frameCount, frameDelay, loopType);
                    }
                    else if (trimmed.StartsWith("Sample"))
                    {
                         if (parts.Length < 4) continue;
                         int time = int.Parse(parts[1]);
                         string path = CleanPath(parts[3]);
                         float vol = (parts.Length > 4) ? float.Parse(parts[4]) : 100f;
                         StartCoroutine(LoadAudioClipExternal(path));
                         sampleEvents.Add(new StorySample { Time = time, Path = path, Volume = vol / 100f });
                    }
                }
                else if (indent == 1 && currentSprite != null)
                {
                    if (parts[0] == "L") 
                    {
                        if (parts.Length < 3) continue;
                        int start = int.Parse(parts[1]);
                        int count = int.Parse(parts[2]);
                        List<string> loopLines = new List<string>();
                        int j = i + 1;
                        while (j < rawLines.Length) {
                            string next = rawLines[j];
                            string nextTrim = next.Trim().TrimStart('_'); 
        
                            foreach(var kvp in variables) 
                                if(nextTrim.Contains(kvp.Key)) nextTrim = nextTrim.Replace(kvp.Key, kvp.Value);
                            
                            if(next.StartsWith("  ") || next.StartsWith("__")) { 
                                loopLines.Add(nextTrim); 
                                j++; 
                            } else break;
                        }
                        UnrollLoop(currentSprite, start, count, loopLines);
                        i = j - 1;
                    }
                    else ParseCommand(currentSprite, parts, 0);
                }
            }
            catch (System.Exception ex) { Debug.LogError($"Error line {i}: {ex.Message}"); }
        }
        stopwatch.Stop();
        is_loaded = true;
        background.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
        particle.SetActive(false);
        Debug.Log($"[Storyboard] Parsing Complete. Total Sprites: {activeSprites.Count}. Playing Music...");
    }
    
    StoryboardSprite CreateAnimation(string relPath, Transform parent, string origin, float x, float y, int frameCount, double frameDelay, string loopType)
    {
        // 準備一個 List 存所有影格
        List<Sprite> frames = new List<Sprite>();
        string ext = Path.GetExtension(relPath); // e.g. ".png"
        string pathNoExt = relPath.Substring(0, relPath.Length - ext.Length); // e.g. "sb/live2d/live2d-"

        // osu! 的動畫檔名規則是原始檔名 + 數字 (從 0 開始)
        for (int i = 0; i < frameCount; i++)
        {
            string framePath = pathNoExt + i + ext; // e.g. "sb/live2d/live2d-0.png"
            string fullPath = GetValidPath(framePath);

            Sprite sp = null;
            if (spriteCache.TryGetValue(fullPath, out Sprite cached)) sp = cached;
            else if (File.Exists(fullPath))
            {
                byte[] data = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                if (tex.LoadImage(data))
                {
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    spriteCache[fullPath] = sp;
                }
            }
            // 如果讀不到圖，加 null 佔位或加空圖，避免索引錯誤
            frames.Add(sp);
        }

        // 建立 GameObject (使用第一張圖當作預設大小參考，如果有的話)
        GameObject go = new GameObject(Path.GetFileName(relPath));
        go.transform.SetParent(parent, false);
        go.SetActive(false);

        Image img = go.AddComponent<Image>();
        img.raycastTarget = false;
        if (frames.Count > 0 && frames[0] != null)
        {
            img.sprite = frames[0];
            img.SetNativeSize();
            img.rectTransform.sizeDelta *= globalScale;
        }

        StoryboardSprite ctrl = go.AddComponent<StoryboardSprite>();
        // 初始化基本參數
        ctrl.Initialize(img, new Vector2(x, y), origin, additiveMaterial, globalScale, xOffset);
        // 設定動畫專用參數
        ctrl.SetAnimationData(frames.ToArray(), frameDelay, loopType == "LoopForever");
        
        activeSprites.Add(ctrl);
        return ctrl;
    }
    StoryboardSprite CreateSprite(string relPath, Transform parent, string origin, float x, float y)
    {
        string fullPath = GetValidPath(relPath);
        
        // 先查快取
        Sprite sp = null;
        if (spriteCache.TryGetValue(fullPath, out Sprite cached)) sp = cached;
        else if (File.Exists(fullPath))
        {
            byte[] data = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            if (tex.LoadImage(data))
            {
                sp = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f), 100f);
                spriteCache[fullPath] = sp;
            }
        }

        GameObject go = new GameObject(Path.GetFileName(relPath));
        go.transform.SetParent(parent, false);
        go.SetActive(false); // 預設關閉，由 Controller 決定
        
        Image img = go.AddComponent<Image>();
        img.raycastTarget = false;
        if (sp != null) {
            img.sprite = sp;
            img.SetNativeSize();
            img.rectTransform.sizeDelta *= globalScale;
        }

        StoryboardSprite ctrl = go.AddComponent<StoryboardSprite>();
        ctrl.Initialize(img, new Vector2(x, y), origin, additiveMaterial, globalScale, xOffset);
        activeSprites.Add(ctrl);
        return ctrl;
    }

    // 修正 2: 智慧路徑 + sb/ 前綴處理
    string GetValidPath(string relPath)
    {
        string fullPath = Path.Combine(baseDirectory, relPath);
        if (File.Exists(fullPath)) return fullPath;
        
        // 嘗試去掉 sb/
        if (relPath.StartsWith("sb\\") || relPath.StartsWith("sb/")) {
            string noSb = relPath.Substring(3);
            string tryPath = Path.Combine(baseDirectory, noSb);
            if (File.Exists(tryPath)) return tryPath;
        }
        return fullPath; 
    }

    // --- 音效與輔助 ---
    IEnumerator LoadAudioClipExternal(string relPath) {
        if (audioCache.ContainsKey(relPath)) yield break;
        string fullPath = GetValidPath(relPath);
        string url = "file://" + fullPath;
        AudioType type = AudioType.UNKNOWN;
        if (fullPath.EndsWith(".wav")) type = AudioType.WAV;
        else if (fullPath.EndsWith(".mp3")) type = AudioType.MPEG;
        else if (fullPath.EndsWith(".ogg")) type = AudioType.OGGVORBIS;

        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, type)) {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success) {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                audioCache[relPath] = clip;
            }
        }
    }
    void UpdateSamples(float time) {
        foreach (var s in sampleEvents) {
            if (!s.HasPlayed && time >= s.Time && time < s.Time + 100) {
                if (audioCache.TryGetValue(s.Path, out AudioClip clip)) sfxSource.PlayOneShot(clip, s.Volume);
                s.HasPlayed = true;
            }
            if (s.HasPlayed && time < s.Time) s.HasPlayed = false;
        }
    }
    string CleanPath(string raw) => raw.Replace("\"", "").Replace("\\", "/");
    void UnrollLoop(StoryboardSprite sprite, int start, int count, List<string> lines) {
        int maxDur = 0;
        foreach(var l in lines) {
            var p = l.Split(',');
            int e = (p[3]=="") ? int.Parse(p[2]) : int.Parse(p[3]);
            if(e > maxDur) maxDur = e;
        }
        for(int i=0; i<count; i++) {
            int offset = start + (i * maxDur);
            foreach(var l in lines) ParseCommand(sprite, l.Split(','), offset);
        }
    }

    // 修正 3: 簡寫防呆 (Handle Shorthand)
    // 這是解決圖片移動到 0,0 或縮放成 0 的關鍵
    void ParseCommand(StoryboardSprite sprite, string[] p, int offset) {
        if (p.Length < 2) return;
        string t = p[0];
        if (t == "T" || t == "L") return;

        int ease = 0, s = 0, e = 0;
        try {
            ease = int.Parse(p[1]);
            s = int.Parse(p[2]) + offset;
            e = (p[3] == "") ? s : int.Parse(p[3]) + offset;
        } catch { return; }

        float[] v = new float[10];
        for (int k = 4; k < p.Length; k++) {
            if (k - 4 >= v.Length) break;
            float.TryParse(p[k], out v[k - 4]);
        }

        if ((t == "S" || t == "V") && (v[0] > 1000f || v[1] > 1000f)) return;

        switch (t) {
            case "F": 
                // Fade: 若無結束值，結束值 = 起始值
                float fStart = v[0];
                float fEnd = (p.Length > 5) ? v[1] : fStart;
                sprite.AddCmd(StoryCmdType.Fade, s, e, ease, fStart, fEnd); 
                break;
            case "M": 
                // Move: 若無結束座標，結束座標 = 起始座標
                float mxStart = v[0], myStart = v[1];
                float mxEnd = (p.Length > 6) ? v[2] : mxStart;
                float myEnd = (p.Length > 6) ? v[3] : myStart;
                sprite.AddCmd(StoryCmdType.MoveX, s, e, ease, mxStart, mxEnd); 
                sprite.AddCmd(StoryCmdType.MoveY, s, e, ease, myStart, myEnd); 
                break;
            case "MX": 
                float mx1 = v[0];
                float mx2 = (p.Length > 5) ? v[1] : mx1;
                sprite.AddCmd(StoryCmdType.MoveX, s, e, ease, mx1, mx2); 
                break;
            case "MY": 
                float my1 = v[0];
                float my2 = (p.Length > 5) ? v[1] : my1;
                sprite.AddCmd(StoryCmdType.MoveY, s, e, ease, my1, my2); 
                break;
            case "S": 
                float s1 = v[0];
                float s2 = (p.Length > 5) ? v[1] : s1;
                sprite.AddCmd(StoryCmdType.ScaleX, s, e, ease, s1, s2); 
                sprite.AddCmd(StoryCmdType.ScaleY, s, e, ease, s1, s2); 
                break;
            case "V": 
                float vx1 = v[0], vy1 = v[1];
                float vx2 = (p.Length > 6) ? v[2] : vx1;
                float vy2 = (p.Length > 6) ? v[3] : vy1;
                sprite.AddCmd(StoryCmdType.ScaleX, s, e, ease, vx1, vx2); 
                sprite.AddCmd(StoryCmdType.ScaleY, s, e, ease, vy1, vy2); 
                break;
            case "R": 
                float r1 = v[0];
                float r2 = (p.Length > 5) ? v[1] : r1;
                sprite.AddCmd(StoryCmdType.Rotate, s, e, ease, r1, r2); 
                break;
            case "C": 
                Color c1 = new Color(v[0]/255f, v[1]/255f, v[2]/255f);
                Color c2 = (p.Length > 7) ? new Color(v[3]/255f, v[4]/255f, v[5]/255f) : c1;
                sprite.AddColorCmd(s, e, ease, c1, c2); 
                break;
            case "P":
                if(p.Length > 4) {
                    // P 指令如果是單點時間，視為永久生效
                    int paramEnd = (s == e) ? int.MaxValue : e;
                    if(p[4]=="A") sprite.AddParamCmd(s, paramEnd, StoryParamType.Additive);
                    if(p[4]=="H") sprite.AddParamCmd(s, paramEnd, StoryParamType.FlipH);
                    if(p[4]=="V") sprite.AddParamCmd(s, paramEnd, StoryParamType.FlipV);
                }
                break;
        }
    }
}

// 修正 4: 嚴格時間控制的 Controller
public class StoryboardSprite : MonoBehaviour
{
    private Image img;
    private RectTransform rect;
    private Material defaultMat;
    private Material additiveMat;
    private Vector2 initialOsuPos;
    private float globalScale; 
    private float globalXOffset; 
    
    private bool isAnimation = false;
    private Sprite[] animFrames;
    private double animFrameDelay;
    private bool animLoopForever;
    
    // 生命週期: minTime 初始值要大，maxTime 初始值要小
    private float minTime = float.MaxValue; 
    private float maxTime = float.MinValue; 
    private bool hasCommands = false;

    private List<StoryCommand> fadeCmds = new List<StoryCommand>();
    private List<StoryCommand> moveXCmds = new List<StoryCommand>();
    private List<StoryCommand> moveYCmds = new List<StoryCommand>();
    private List<StoryCommand> scaleXCmds = new List<StoryCommand>();
    private List<StoryCommand> scaleYCmds = new List<StoryCommand>();
    private List<StoryCommand> rotateCmds = new List<StoryCommand>();
    private List<StoryColorCommand> colorCmds = new List<StoryColorCommand>();
    private List<StoryParamCommand> paramCmds = new List<StoryParamCommand>();

    public void Initialize(Image image, Vector2 startPos, string origin, Material addMat, float gScale, float xOff)
    {
        img = image;
        rect = GetComponent<RectTransform>();
        initialOsuPos = startPos;
        defaultMat = null; 
        additiveMat = addMat;
        globalScale = gScale;
        globalXOffset = xOff;

        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        
        switch (origin) {
            case "TopLeft":      rect.pivot = new Vector2(0, 1); break;
            case "TopCentre":    rect.pivot = new Vector2(0.5f, 1); break;
            case "TopRight":     rect.pivot = new Vector2(1, 1); break;
            case "CentreLeft":   rect.pivot = new Vector2(0, 0.5f); break;
            case "Centre":       rect.pivot = new Vector2(0.5f, 0.5f); break;
            case "CentreRight":  rect.pivot = new Vector2(1, 0.5f); break;
            case "BottomLeft":   rect.pivot = new Vector2(0, 0); break;
            case "BottomCentre": rect.pivot = new Vector2(0.5f, 0); break;
            case "BottomRight":  rect.pivot = new Vector2(1, 0); break;
            default:             rect.pivot = new Vector2(0.5f, 0.5f); break;
        }
        gameObject.SetActive(false); // 初始關閉
    }
    
    public void SetAnimationData(Sprite[] frames, double delay, bool loop) {
        this.animFrames = frames;
        this.animFrameDelay = delay;
        this.animLoopForever = loop;
        this.isAnimation = true;
    }

    public void AddCmd(StoryCmdType t, int s, int e, int ea, float v1, float v2) {
        var c = new StoryCommand { Start = s, End = e, Easing = ea, Val1 = v1, Val2 = v2 };
        UpdateLifeTime(s, e);
        if(t==StoryCmdType.Fade) fadeCmds.Add(c);
        else if(t==StoryCmdType.MoveX) moveXCmds.Add(c);
        else if(t==StoryCmdType.MoveY) moveYCmds.Add(c);
        else if(t==StoryCmdType.ScaleX) scaleXCmds.Add(c);
        else if(t==StoryCmdType.ScaleY) scaleYCmds.Add(c);
        else if(t==StoryCmdType.Rotate) rotateCmds.Add(c);
    }
    public void AddColorCmd(int s, int e, int ea, Color c1, Color c2) {
        colorCmds.Add(new StoryColorCommand { Start = s, End = e, Easing = ea, C1 = c1, C2 = c2 });
        UpdateLifeTime(s, e);
    }
    public void AddParamCmd(int s, int e, StoryParamType p) {
        paramCmds.Add(new StoryParamCommand { Start = s, End = e, Type = p });
        UpdateLifeTime(s, e);
    }

    void UpdateLifeTime(int s, int e) {
        if (s < minTime) minTime = s;
        if (e != int.MaxValue && e > maxTime) maxTime = e;
        hasCommands = true;
    }

    public void UpdateAnimation(float time)
    {
        // 嚴格時間檢查：只在指令範圍內顯示
        bool shouldActive = hasCommands && (time >= minTime && time <= maxTime);
        if (gameObject.activeSelf != shouldActive) gameObject.SetActive(shouldActive);
        if (!shouldActive) return;

        // 1. Fade (預設 1, 時間未到取起始值)
        float alpha = GetValue(fadeCmds, time, 1f); 
        
        // 2. Position
        float osuX = GetValue(moveXCmds, time, initialOsuPos.x);
        float osuY = GetValue(moveYCmds, time, initialOsuPos.y);
        float finalX = globalXOffset + (osuX * globalScale);
        float finalY = -(osuY * globalScale); 
        rect.anchoredPosition = new Vector2(finalX, finalY);

        // 3. Scale & Rot
        float sx = GetValue(scaleXCmds, time, 1f);
        float sy = GetValue(scaleYCmds, time, 1f);
        float rot = GetValue(rotateCmds, time, 0f);
        rect.localScale = new Vector3(sx, sy, 1f);
        rect.localEulerAngles = new Vector3(0, 0, -rot * Mathf.Rad2Deg);

        // 4. Color
        Color col = GetColorValue(time);
        col.a = alpha;
        img.color = col;

        if (isAnimation && animFrames != null && animFrames.Length > 0)
        {
            // 動畫是從物件出現的時間 (minTime) 開始播放
            double activeTime = time - minTime;

            // 計算當前應該是第幾幀
            int frameIndex = 0;
            if (activeTime >= 0)
            {
                int totalFrames = animFrames.Length;
                double totalDuration = totalFrames * animFrameDelay;

                if (animLoopForever)
                {
                    frameIndex = (int)((activeTime / animFrameDelay) % totalFrames);
                }
                else
                {
                    frameIndex = (int)(activeTime / animFrameDelay);
                    // LoopOnce: 播完最後一幀後，osu! 通常是停在最後一幀或消失
                    // 這裡我們讓它停在最後一幀
                    if (frameIndex >= totalFrames) frameIndex = totalFrames - 1;
                }
            }

            // 只有當圖片真的改變時才賦值，優化效能
            if (img.sprite != animFrames[frameIndex] && animFrames[frameIndex] != null)
            {
                img.sprite = animFrames[frameIndex];
            }
        }
        

        // 5. Params
        bool isAdd = false;
        
        // 用來標記是否已經處理過該類型的第一個指令
        bool processedAdd = false;
        bool processedFlipH = false;
        bool processedFlipV = false;

        foreach (var p in paramCmds) {
            // 判斷指令是否在有效區間 (包含時間未到但這是第一個指令的情況)
            
            // 處理 Additive
            if (p.Type == StoryParamType.Additive) {
                if (!processedAdd) {
                    // 如果這是第一個 Additive 指令，且時間還沒超過它的結束時間 (回溯生效)
                    if (time <= p.End) isAdd = true;
                    processedAdd = true;
                } else if (time >= p.Start && time <= p.End) {
                    // 後續指令嚴格遵守時間
                    isAdd = true;
                }
            }

            // 處理 FlipH
            if (p.Type == StoryParamType.FlipH) {
                if (!processedFlipH) {
                    if (time <= p.End) rect.localScale = new Vector3(-sx, sy, 1);
                    processedFlipH = true;
                } else if (time >= p.Start && time <= p.End) {
                    rect.localScale = new Vector3(-sx, sy, 1);
                }
            }
            
            // 處理 FlipV
            if (p.Type == StoryParamType.FlipV) {
                if (!processedFlipV) {
                    if (time <= p.End) rect.localScale = new Vector3(sx, -sy, 1);
                    processedFlipV = true;
                } else if (time >= p.Start && time <= p.End) {
                    rect.localScale = new Vector3(sx, -sy, 1);
                }
            }
        }
        img.material = isAdd ? additiveMat : defaultMat;
    }

    float GetValue(List<StoryCommand> cmds, float time, float defaultVal) {
        if (cmds.Count == 0) return defaultVal;
        StoryCommand active = null;
        foreach (var c in cmds) { if (c.Start <= time) active = c; else break; }

        if (active == null) return cmds[0].Val1; // 時間未到，回傳起始值
        if (time >= active.End) return active.Val2; // 時間已過，回傳結束值

        float t = (time - active.Start) / (active.End - active.Start);
        return Mathf.Lerp(active.Val1, active.Val2, Easing(active.Easing, t));
    }
    
    Color GetColorValue(float time) {
        if (colorCmds.Count == 0) return Color.white;
        StoryColorCommand active = null;
        foreach(var c in colorCmds) { if(c.Start <= time) active = c; else break; }
        
        if (active == null) return colorCmds[0].C1;
        if (time >= active.End) return active.C2;

        float t = (time - active.Start) / (active.End - active.Start);
        return Color.Lerp(active.C1, active.C2, Easing(active.Easing, t));
    }

    float Easing(int type, float t) {
        switch (type) {
            case 0: return t;
            case 1: return -t * (t - 2);
            case 2: return t * t;
            default: return t; 
        }
    }
}

// 資料結構
public class StoryCommand { public int Start, End, Easing; public float Val1, Val2; }
public class StoryColorCommand { public int Start, End, Easing; public Color C1, C2; }
public class StoryParamCommand { public int Start, End; public StoryParamType Type; }
public class StorySample { public int Time; public string Path; public float Volume; public bool HasPlayed; }
public enum StoryCmdType { MoveX, MoveY, ScaleX, ScaleY, Rotate, Fade }
public enum StoryParamType { Additive, FlipH, FlipV }