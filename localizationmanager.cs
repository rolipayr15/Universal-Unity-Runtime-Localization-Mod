using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationManager : MonoBehaviour
{
    public static Dictionary<string, string> Dict = new Dictionary<string, string>(StringComparer.Ordinal);

    private static LocalizationManager _inst;
    private static TMP_FontAsset _customFont;

    public static void Init()
    {
        if (_inst != null) return;
        GameObject go = new GameObject("~LocalizationManager~");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        _inst = go.AddComponent<LocalizationManager>();
        LoadFromFile();
        UnityEngine.Debug.Log(string.Format("[Localization] Init OK, dict size={0}", Dict.Count));
    }

    public static string DictPath
    {
        get { return System.IO.Path.Combine(Application.dataPath, "russian.txt"); }
    }

    public static void LoadFromFile()
    {
        try
        {
            if (!System.IO.File.Exists(DictPath)) return;
            byte[] bytes = System.IO.File.ReadAllBytes(DictPath);
            string allText = System.Text.Encoding.UTF8.GetString(bytes);
            string[] lines = allText.Split('\n');
            
            Dict.Clear();
            int added = 0;
            
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0 || eq >= line.Length - 1) continue;
                
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();
                
                val = val.Replace("\\n", "\n");
                key = key.Replace("\\n", "\n");
                
                Dict[key] = val;
                added++;
            }
            UnityEngine.Debug.Log(string.Format("[Localization] Loaded {0} entries.", added));
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("[Localization] Load error: " + e.Message);
        }
    }

    private float _scanTimer;
    private GameObject _consoleRoot;
    private TMP_Text _consoleText;

    private void Awake()
    {
        CreateConsole();
        LoadCustomFont();
    }

    private void LoadCustomFont()
    {
        try
        {
            Font sysFont = null;
            
            // Загружаем системный шрифт через рефлексию (чтобы компилятор dnSpy не ругался)
            try 
            {
                var method = typeof(Font).GetMethod("CreateDynamicFontFromOSFont", new Type[] { typeof(string), typeof(int) });
                if (method != null)
                {
                    sysFont = (Font)method.Invoke(null, new object[] { "Georgia", 16 });
                    if (sysFont == null) sysFont = (Font)method.Invoke(null, new object[] { "Times New Roman", 16 });
                    if (sysFont == null) sysFont = (Font)method.Invoke(null, new object[] { "Arial", 16 });
                }
            }
            catch { }

            // Если рефлексия не сработала, берем встроенный Arial
            if (sysFont == null)
            {
                sysFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            if (sysFont != null)
            {
                _customFont = TMP_FontAsset.CreateFontAsset(sysFont);
                UnityEngine.Debug.Log("[Localization] Font successfully created.");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("[Localization] Font error: " + e.Message);
        }
    }

    private void CreateConsole()
    {
        _consoleRoot = new GameObject("~LocConsole~");
        _consoleRoot.transform.SetParent(this.transform, false);
        
        Canvas canvas = _consoleRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        CanvasScaler scaler = _consoleRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        _consoleRoot.AddComponent<GraphicRaycaster>();

        GameObject bgObj = new GameObject("BG");
        bgObj.transform.SetParent(_consoleRoot.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(-100, -100);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bgObj.transform, false);
        _consoleText = textObj.AddComponent<TextMeshProUGUI>();
        _consoleText.fontSize = 18;
        _consoleText.color = Color.white;
        _consoleText.alignment = TextAlignmentOptions.TopLeft;
        
        if (_customFont != null) _consoleText.font = _customFont;
        else if (TMP_Settings.defaultFontAsset != null) _consoleText.font = TMP_Settings.defaultFontAsset;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0, 25);
        textRect.sizeDelta = new Vector2(-40, -90); 

        GameObject btnObj = new GameObject("DumpBtn");
        btnObj.transform.SetParent(bgObj.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(DumpToConsole);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0);
        btnRect.anchorMax = new Vector2(0, 0);
        btnRect.pivot = new Vector2(0, 0);
        btnRect.anchoredPosition = new Vector2(20, 20);
        btnRect.sizeDelta = new Vector2(200, 40);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "DUMP STRINGS";
        btnText.fontSize = 20;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        
        if (_customFont != null) btnText.font = _customFont;
        else if (TMP_Settings.defaultFontAsset != null) btnText.font = TMP_Settings.defaultFontAsset;
            
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.pivot = new Vector2(0.5f, 0.5f);
        btnTextRect.anchoredPosition = Vector2.zero;
        btnTextRect.sizeDelta = Vector2.zero;

        _consoleRoot.SetActive(false);
    }

    private void Update()
    {
        _scanTimer += Time.unscaledDeltaTime;
        if (_scanTimer >= 0.3f)
        {
            _scanTimer = 0f;
            TranslateAll();
        }

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        
        if (kb.f9Key.wasPressedThisFrame)
        {
            _consoleRoot.SetActive(!_consoleRoot.activeSelf);
            if (_consoleRoot.activeSelf && string.IsNullOrEmpty(_consoleText.text))
                DumpToConsole();
        }
        if (kb.f10Key.wasPressedThisFrame)
        {
            LoadFromFile();
            TranslateAll();
        }
    }

    public void DumpToConsole()
    {
        StringBuilder sb = new StringBuilder();
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        
        sb.AppendLine("=== UNIQUE STRINGS DUMP ===");
        sb.AppendLine("Date: " + System.DateTime.Now.ToString());
        sb.AppendLine();

        TMP_Text[] tmps = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (var t in tmps)
        {
            if (t == null || string.IsNullOrEmpty(t.text)) continue;
            string s = t.text.Trim();
            if (s.Length == 0 || IsRussian(s)) continue;
            if (seen.Add(s)) sb.AppendLine(s);
        }
        
        Text[] legacy = Resources.FindObjectsOfTypeAll<Text>();
        foreach (var t in legacy)
        {
            if (t == null || string.IsNullOrEmpty(t.text)) continue;
            string s = t.text.Trim();
            if (s.Length == 0 || IsRussian(s)) continue;
            if (seen.Add(s)) sb.AppendLine(s);
        }
        
        string result = sb.ToString();
        string preview = result.Length > 1000 ? result.Substring(0, 1000) + "\n... (см. в буфере)" : result;
        _consoleText.text = string.Format("УСПЕХ! Скопировано: {0}\n\n{1}", seen.Count, preview);
        
        UnityEngine.GUIUtility.systemCopyBuffer = result;
    }

    public static void TranslateAll()
    {
        TMP_Text[] tmps = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (var t in tmps) { if (t != null) TranslateText(t); }

        Text[] legacy = Resources.FindObjectsOfTypeAll<Text>();
        foreach (var t in legacy) { if (t != null) TranslateLegacy(t); }
    }

    private static void TranslateText(TMP_Text t)
    {
        string cur = t.text;
        if (string.IsNullOrEmpty(cur) || IsRussian(cur)) return;
        
        string translated;
        if (Dict.TryGetValue(cur, out translated))
        {
            t.text = translated;
            
            if (_customFont != null)
            {
                t.font = _customFont;
            }

            // Чиним размеры в чертежах и кнопках
            t.enableAutoSizing = true;
            t.fontSizeMin = 6f;               
            t.fontSizeMax = t.fontSize;       
            t.textWrappingMode = TextWrappingModes.Normal; 
            t.overflowMode = TextOverflowModes.Truncate;   
            
            if (t.alignment == TextAlignmentOptions.Top || t.alignment == TextAlignmentOptions.TopLeft)
            {
                t.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    private static void TranslateLegacy(Text t)
    {
        string cur = t.text;
        if (string.IsNullOrEmpty(cur) || IsRussian(cur)) return;
        string translated;
        if (Dict.TryGetValue(cur, out translated)) t.text = translated;
    }

    private static bool IsRussian(string s)
    {
        foreach (char c in s) { if ((c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё') return true; }
        return false;
    }
}