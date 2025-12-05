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
    public string assetRootFolder = ""; 
    
    [Header("References")]
    public Material additiveMaterial; // UI Additive Material

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

    public GameState gameController;
    
    public bool is_loaded = false;
    
    // --- 新增：圖片快取池 (解決重複讀取造成的卡頓) ---
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Start()
    {
        // 1. 計算縮放
        globalScale = TARGET_HEIGHT / 480f; 
        float contentWidth = 640f * globalScale;
        xOffset = (TARGET_WIDTH - contentWidth) / 2f;

        Debug.Log($"[Storyboard] Init. Scale: {globalScale}, X-Offset: {xOffset}");

        // 2. 初始化
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
        
        // 3. 讀檔
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
            float time = Time.time * 1000f - gameController.start_time;

            // 為了效能，這裡也可以考慮分批更新，但通常 UpdateAnimation 計算量還好
            foreach (var sprite in activeSprites) sprite.UpdateAnimation(time);
            UpdateSamples(time);
        }
    }

    IEnumerator ParseRoutine()
    {
        // 讀取所有行
        string[] rawLines = File.ReadAllLines(osbFilePath);
        Debug.Log($"[Storyboard] Start Parsing {rawLines.Length} lines...");

        // 變數解析
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
        
        // --- 效能優化計時器 ---
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < rawLines.Length; i++)
        {
            // --- 關鍵優化：分時處理 (防止畫面卡死) ---
            // 每經過 15 毫秒 (約一幀的時間)，就暫停一下讓 Unity 渲染畫面
            if (stopwatch.ElapsedMilliseconds > 15)
            {
                yield return null; // 等待下一幀
                stopwatch.Restart();
            }
            // ------------------------------------

            string line = rawLines[i];
            string trimmed = line.Trim();
            
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
                    string layerName = parts[1];
                    string origin = parts[2];
                    string path = CleanPath(parts[3]);
                    float x = float.Parse(parts[4]);
                    float y = float.Parse(parts[5]);
                    
                    Transform parent = layerForeground;
                    if (layerName == "Background") parent = layerBackground;
                    else if (layerName == "Fail") parent = layerFail;
                    else if (layerName == "Pass") parent = layerPass;
                    else if (layerName == "Overlay") parent = layerOverlay;

                    currentSprite = CreateSprite(path, parent, origin, x, y);
                }
                else if (trimmed.StartsWith("Sample"))
                {
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
                    int start = int.Parse(parts[1]);
                    int count = int.Parse(parts[2]);
                    List<string> loopLines = new List<string>();
                    
                    int j = i + 1;
                    while (j < rawLines.Length) {
                        string next = rawLines[j];
                        string nextTrim = next.Trim();
                        foreach(var kvp in variables) 
                            if(nextTrim.Contains(kvp.Key)) nextTrim = nextTrim.Replace(kvp.Key, kvp.Value);
                        
                        if(next.StartsWith("  ") || next.StartsWith("__")) { 
                            loopLines.Add(nextTrim); j++; 
                        } else break;
                    }
                    UnrollLoop(currentSprite, start, count, loopLines);
                    i = j - 1;
                }
                else 
                {
                    ParseCommand(currentSprite, parts, 0);
                }
            }
        }
        
        stopwatch.Stop();
        Debug.Log($"[Storyboard] Parsing Complete. Total Sprites: {activeSprites.Count}. Playing Music...");
        is_loaded = true;
        
        // 釋放記憶體 (選用)
        Resources.UnloadUnusedAssets();
        
    }

    StoryboardSprite CreateSprite(string relPath, Transform parent, string origin, float x, float y)
    {
        GameObject go = new GameObject(Path.GetFileName(relPath));
        go.transform.SetParent(parent, false);
        go.SetActive(false); // 預設關閉
        
        Image img = go.AddComponent<Image>();
        img.raycastTarget = false; 

        // --- 關鍵優化：圖片快取機制 ---
        Sprite sp = null;
        string fullPath = GetValidPath(relPath); // 取得正確路徑

        // 1. 先查快取
        if (spriteCache.TryGetValue(fullPath, out Sprite cachedSprite))
        {
            sp = cachedSprite;
        }
        else if (File.Exists(fullPath))
        {
            // 2. 快取沒有，才讀取檔案
            try 
            {
                byte[] fileData = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.filterMode = FilterMode.Bilinear; 
                tex.wrapMode = TextureWrapMode.Clamp; 

                if (tex.LoadImage(fileData))
                {
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    sp.name = relPath;
                    
                    // 3. 存入快取
                    spriteCache.Add(fullPath, sp);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Storyboard] Load Error: {fullPath}\n{ex.Message}");
                // 失敗也存個 null 進去，避免下次又嘗試讀取壞檔，造成卡頓
                if (!spriteCache.ContainsKey(fullPath)) spriteCache.Add(fullPath, null);
            }
        }
        else
        {
            // 找不到檔案也記錄，避免重複 IO 查詢
             if (!spriteCache.ContainsKey(fullPath)) spriteCache.Add(fullPath, null);
        }

        // 應用 Sprite
        if (sp != null)
        {
            img.sprite = sp;
            img.SetNativeSize(); 
            img.rectTransform.sizeDelta *= globalScale; 
        }
        // ---------------------------

        StoryboardSprite ctrl = go.AddComponent<StoryboardSprite>();
        ctrl.Initialize(img, new Vector2(x, y), origin, additiveMaterial, globalScale, xOffset);
        activeSprites.Add(ctrl);
        return ctrl;
    }

    // --- 路徑搜尋 (包含 sb/ 去除與副檔名修正) ---
    string GetValidPath(string relPath)
    {
        // 為了讓 Cache Key 統一，這裡直接回傳完整路徑
        string fullPath = Path.Combine(baseDirectory, relPath);
        if (File.Exists(fullPath)) return fullPath;

        string alt = CheckAltExtension(fullPath);
        if (alt != null) return alt;

        if (relPath.StartsWith("sb/", System.StringComparison.OrdinalIgnoreCase) || relPath.StartsWith("sb\\", System.StringComparison.OrdinalIgnoreCase))
        {
            string noSbPath = relPath.Substring(3);
            string tryPath = Path.Combine(baseDirectory, noSbPath);
            if (File.Exists(tryPath)) return tryPath;
            
            alt = CheckAltExtension(tryPath);
            if (alt != null) return alt;
        }
        return fullPath; 
    }

    string CheckAltExtension(string path)
    {
        string png = Path.ChangeExtension(path, ".png");
        if (File.Exists(png)) return png;
        string jpg = Path.ChangeExtension(path, ".jpg");
        if (File.Exists(jpg)) return jpg;
        return null;
    }

    // --- 其餘部分 (音效, ParseCommand 等) 保持不變 ---
    IEnumerator LoadAudioClipExternal(string relPath)
    {
        if (audioCache.ContainsKey(relPath)) yield break;
        string fullPath = GetValidPath(relPath);
        string url = "file://" + fullPath;
        
        AudioType type = AudioType.UNKNOWN;
        if (fullPath.EndsWith(".wav")) type = AudioType.WAV;
        else if (fullPath.EndsWith(".mp3")) type = AudioType.MPEG;
        else if (fullPath.EndsWith(".ogg")) type = AudioType.OGGVORBIS;

        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(url, type))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                clip.name = relPath;
                if (!audioCache.ContainsKey(relPath)) audioCache.Add(relPath, clip);
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

    void ParseCommand(StoryboardSprite sprite, string[] p, int offset)
    {
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

        // 防呆
        if ((t == "S" || t == "V") && (v[0] > 1000f || v[1] > 1000f)) return;

        switch (t) {
            case "F": 
                sprite.AddCmd(StoryCmdType.Fade, s, e, ease, v[0], (p.Length > 5) ? v[1] : v[0]); 
                break;
            case "M": 
                sprite.AddCmd(StoryCmdType.MoveX, s, e, ease, v[0], (p.Length > 6) ? v[2] : v[0]); 
                sprite.AddCmd(StoryCmdType.MoveY, s, e, ease, v[1], (p.Length > 6) ? v[3] : v[1]); 
                break;
            case "MX": 
                sprite.AddCmd(StoryCmdType.MoveX, s, e, ease, v[0], (p.Length > 5) ? v[1] : v[0]); 
                break;
            case "MY": 
                sprite.AddCmd(StoryCmdType.MoveY, s, e, ease, v[0], (p.Length > 5) ? v[1] : v[0]); 
                break;
            case "S": 
                sprite.AddCmd(StoryCmdType.ScaleX, s, e, ease, v[0], (p.Length > 5) ? v[1] : v[0]); 
                sprite.AddCmd(StoryCmdType.ScaleY, s, e, ease, v[0], (p.Length > 5) ? v[1] : v[0]); 
                break;
            case "V": 
                sprite.AddCmd(StoryCmdType.ScaleX, s, e, ease, v[0], (p.Length > 6) ? v[2] : v[0]); 
                sprite.AddCmd(StoryCmdType.ScaleY, s, e, ease, v[1], (p.Length > 6) ? v[3] : v[1]); 
                break;
            case "R": 
                sprite.AddCmd(StoryCmdType.Rotate, s, e, ease, v[0], (p.Length > 5) ? v[1] : v[0]); 
                break;
            case "C": 
                Color c1 = new Color(v[0]/255f, v[1]/255f, v[2]/255f);
                Color c2 = (p.Length > 7) ? new Color(v[3]/255f, v[4]/255f, v[5]/255f) : c1;
                sprite.AddColorCmd(s, e, ease, c1, c2); 
                break;
            case "P":
                if(p.Length > 4) {
                    // --- 關鍵修正：如果是單點時間的 Parameter，視為永久生效 (直到物件消失) ---
                    // 這樣 Additive (A) 和 Flip (H/V) 就不會閃一下就消失了
                    int paramEnd = (s == e) ? int.MaxValue : e;

                    if(p[4]=="A") sprite.AddParamCmd(s, paramEnd, StoryParamType.Additive);
                    if(p[4]=="H") sprite.AddParamCmd(s, paramEnd, StoryParamType.FlipH);
                    if(p[4]=="V") sprite.AddParamCmd(s, paramEnd, StoryParamType.FlipV);
                }
                break;
        }
    }
}

// --- 控制器類別 ---
// 請替換原本的 StoryboardSprite 類別
public class StoryboardSprite : MonoBehaviour
{
    private Image img;
    private RectTransform rect;
    private Material defaultMat;
    private Material additiveMat;
    
    private Vector2 initialOsuPos;
    private float globalScale; 
    private float globalXOffset; 
    
    // 生命週期
    private float minTime = float.MaxValue;
    private float maxTime = float.MinValue;
    private bool hasCommands = false;

    // 指令軌道
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
        
        switch (origin)
        {
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
        gameObject.SetActive(false);
    }

    public void AddCmd(StoryCmdType t, int s, int e, int ea, float v1, float v2)
    {
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
        if (e > maxTime) maxTime = e;
        hasCommands = true;
    }

    public void UpdateAnimation(float time)
    {
        bool shouldActive = hasCommands && (time >= minTime && time <= maxTime);
        if (gameObject.activeSelf != shouldActive) gameObject.SetActive(shouldActive);
        if (!shouldActive) return;

        // 1. Fade (透明度)
        // 修正：預設值給 1 (不透明)，且不再強制設為 0
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

        // 5. Params
        bool isAdd = false;
        foreach (var p in paramCmds) {
            if (time >= p.Start && time <= p.End) {
                if (p.Type == StoryParamType.Additive) isAdd = true;
                if (p.Type == StoryParamType.FlipH) rect.localScale = new Vector3(-sx, sy, 1);
                if (p.Type == StoryParamType.FlipV) rect.localScale = new Vector3(sx, -sy, 1);
            }
        }
        img.material = isAdd ? additiveMat : defaultMat;
    }

    // --- 關鍵修正：取值邏輯 ---
    float GetValue(List<StoryCommand> cmds, float time, float defaultVal) {
        if (cmds.Count == 0) return defaultVal;

        StoryCommand active = null;
        // 尋找當前時間點的指令
        foreach (var c in cmds) { 
            if (c.Start <= time) active = c; 
            else break; // 假設指令已按時間排序
        }

        // 情況 A: 時間還沒到第一個指令 -> 回傳第一個指令的「起始值」
        if (active == null) return cmds[0].Val1;

        // 情況 B: 時間超過該指令結束 -> 保持該指令的「結束值」
        if (time >= active.End) return active.Val2;

        // 情況 C: 指令進行中 -> 計算插值
        float t = (time - active.Start) / (active.End - active.Start);
        return Mathf.Lerp(active.Val1, active.Val2, Easing(active.Easing, t));
    }
    
    Color GetColorValue(float time) {
        if (colorCmds.Count == 0) return Color.white;
        StoryColorCommand active = null;
        foreach(var c in colorCmds) { if(c.Start <= time) active = c; else break; }
        
        // 顏色也套用同樣的邏輯
        if (active == null) return colorCmds[0].C1; 
        if (time >= active.End) return active.C2;

        float t = (time - active.Start) / (active.End - active.Start);
        return Color.Lerp(active.C1, active.C2, Easing(active.Easing, t));
    }

    float Easing(int type, float t) {
        // 簡單的 Easing 實作，完整 osu Easing 有 30 種
        switch (type) {
            case 0: return t; // Linear
            case 1: return -t * (t - 2); // Out (Quad)
            case 2: return t * t; // In (Quad)
            default: return t; 
        }
    }
}

// --- 資料結構 ---
public class StoryCommand { public int Start, End, Easing; public float Val1, Val2; }
public class StoryColorCommand { public int Start, End, Easing; public Color C1, C2; }
public class StoryParamCommand { public int Start, End; public StoryParamType Type; }
public class StorySample { public int Time; public string Path; public float Volume; public bool HasPlayed; }
public enum StoryCmdType { MoveX, MoveY, ScaleX, ScaleY, Rotate, Fade }
public enum StoryParamType { Additive, FlipH, FlipV }