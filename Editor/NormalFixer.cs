using UnityEngine;
using UnityEditor;
using System.IO;


//GUI is based on Diabolickal's HDRP mask map packer, system changed to invert normal maps instead


public class NormalFixer : EditorWindow
{

    private Texture NormalMap;

    private Texture2D r_NormalMap;

    private Texture2D finalTexture;

    private Vector2Int texSize;
    private static EditorWindow window;
    private Vector2 scrollPos;

    [MenuItem("Tools/Normal Map Correcter")]
    public static void ShowWindow()
    {
        window = GetWindow(typeof(NormalFixer), false);
    }
    private void OnInspectorUpdate()
    {
        if (!window)
            window = GetWindow(typeof(NormalFixer), false);
    }
    private void OnGUI()
    {
        if (window)
        {
            GUILayout.BeginArea(new Rect(0, 0, window.position.size.x, window.position.size.y));
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.ExpandHeight(true));
        }

        GUIStyle BigBold = new GUIStyle();
        BigBold.fontSize = 16;
        BigBold.fontStyle = FontStyle.Bold;
        BigBold.wordWrap = true;
        BigBold.alignment = TextAnchor.MiddleCenter;

        GUIStyle Wrap = new GUIStyle();
        Wrap.wordWrap = true;
        Wrap.alignment = TextAnchor.MiddleCenter;

        GUIStyle subTitle = new GUIStyle();
        subTitle.richText = true;
        subTitle.wordWrap = true;
        subTitle.fontStyle = FontStyle.Bold;
        subTitle.alignment = TextAnchor.MiddleCenter;

        GUIStyle preview = new GUIStyle();
        preview.alignment = TextAnchor.UpperCenter;

        GUILayout.Space(10f);
        GUILayout.Label("Convert DirectX Normal Map to OpenGL", BigBold);
        GUILayout.Space(10f);
        GUILayout.Label("Or from OpenGL to DirectX for some reason, you weirdo", subTitle);
        GUILayout.Space(10f);

        //Normal Map Input
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(10f);
        NormalMap = (Texture2D)EditorGUILayout.ObjectField("Normal Map", NormalMap, typeof(Texture2D), false);

        GUILayout.Space(10f);
        GUILayout.EndVertical();


        if (!NormalMap)
        {
            texSize = Vector2Int.zero;
            GUILayout.Label("No Normal Map selected", Wrap);
        }
        else
        {
            if (GUILayout.Button("Invert Normal Map"))
            {
                EditorUtility.DisplayProgressBar("Packing Textures, please wait...", "", 1f);
                PackTextures();
            }
        }

        GUILayout.Space(100);
        if (window)
        {
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
    TextureImporter rawImporter;
    TextureImporterType textureType;
    bool mipmapEnabled;
    bool isReadable;
    FilterMode filterMode;
    TextureImporterNPOTScale npotScale;
    TextureWrapMode wrapMode;
    bool sRGBTexture;
    System.Int32 maxTextureSize;
    TextureImporterCompression textureCompression;
    private void PackTextures()
    {
        UpdateTexture(false);

        var path = EditorUtility.SaveFilePanelInProject("Save Texture To Diectory", NormalMap.name + "_Inverted", "png", "Saved");
        var pngData = finalTexture.EncodeToPNG();
        if (path.Length != 0)
        {
            if (pngData != null)
            {
                File.WriteAllBytes(path, pngData);
            }
        }
        AssetDatabase.Refresh();

        //restore original texture settings
        rawImporter.textureType = textureType;
        rawImporter.mipmapEnabled = mipmapEnabled;
        rawImporter.isReadable = isReadable;
        rawImporter.filterMode = filterMode;
        rawImporter.npotScale = npotScale;
        rawImporter.wrapMode = wrapMode;
        rawImporter.sRGBTexture = sRGBTexture;
        rawImporter.maxTextureSize = maxTextureSize;
        rawImporter.textureCompression = textureCompression;
        rawImporter.SaveAndReimport();

        //set new normal settings
        TextureImporter NormalImporter = (TextureImporter)AssetImporter.GetAtPath(path);
        NormalImporter.textureType = TextureImporterType.NormalMap;
        NormalImporter.maxTextureSize = maxTextureSize;
        NormalImporter.SaveAndReimport();

        Debug.Log("Texture Saved to: " + path);
    }
    private void UpdateTexture(bool asPreview)
    {
        r_NormalMap = (Texture2D)GetRawTexture(NormalMap);

        finalTexture = new Texture2D(texSize.x, texSize.y, TextureFormat.RGBAFloat, true);

        for (int x = 0; x < texSize.x; x++)
        {
            for (int y = 0; y < texSize.y; y++)
            {
                float R, G, B;
                R = r_NormalMap.GetPixel(x, y).r;
                //R = r_NormalMap.getr(x, y).r;
                G = 1 - r_NormalMap.GetPixel(x, y).g;
                B = r_NormalMap.GetPixel(x, y).b;

                finalTexture.SetPixel(x, y, new Color(R, G, B));
            }
        }
        finalTexture.Apply();
        EditorUtility.ClearProgressBar();
    }

    private Texture GetRawTexture(Texture original, bool sRGBFallback = false)
    {
        string path = AssetDatabase.GetAssetPath(original);

        rawImporter = (TextureImporter)AssetImporter.GetAtPath(path);

        //get current settings
        textureType = rawImporter.textureType;
        mipmapEnabled = rawImporter.mipmapEnabled;
        isReadable = rawImporter.isReadable;
        filterMode = rawImporter.filterMode;
        npotScale = rawImporter.npotScale;
        wrapMode = rawImporter.wrapMode;
        sRGBTexture = rawImporter.sRGBTexture;
        maxTextureSize = rawImporter.maxTextureSize;
        textureCompression = rawImporter.textureCompression;

        //set the required setings for the conversion
        rawImporter.textureType = TextureImporterType.Default;
        rawImporter.mipmapEnabled = false;
        rawImporter.isReadable = true;
        //rawImporter.filterMode = m_bilinearFilter ? FilterMode.Bilinear : FilterMode.Point;
        rawImporter.filterMode = true ? FilterMode.Bilinear : FilterMode.Point;
        rawImporter.npotScale = TextureImporterNPOTScale.None;
        rawImporter.wrapMode = TextureWrapMode.Clamp;

        int w, h;
        rawImporter.GetSourceTextureWidthAndHeight(out w, out h);
        texSize = new Vector2Int(w, h);

        Texture2D originalTex2D = original as Texture2D;
        rawImporter.sRGBTexture = (originalTex2D == null) ? sRGBFallback : (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(original)) as TextureImporter).sRGBTexture;

        rawImporter.maxTextureSize = 8192;

        rawImporter.textureCompression = TextureImporterCompression.Uncompressed;

        rawImporter.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Texture>(path);
    }
}