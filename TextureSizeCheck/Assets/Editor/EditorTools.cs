using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

public class EditorTools{

    private static string defaultWhiteTexPath_relative = "Assets/Resources/Atlas/BadgeAtlas.png";

    private static Texture2D defaultWhiteTex = null;

    static Dictionary<string, int[]> generatedTextures = new Dictionary<string, int[]>();


    [MenuItem("EffortForETC1/Depart RGB and Alpha Channel")]
    public static void SeperateAllTexturesRGBandAlphaChannel()
    {
        Debug.Log("Start Departing.");
        if (!GetDefaultWhiteTexture())
        {
            return;
        }

        string strSearchPath = Path.Combine(Application.dataPath, "Resources/Atlas");

        string[] paths = Directory.GetFiles(strSearchPath, "*.*", SearchOption.AllDirectories);

        foreach (string path in paths)
        {
            //Debug.Log(path);
            if (!string.IsNullOrEmpty(path) && IsTextureFile(path) && !IsTextureConverted(path))
            {
                Debug.Log(path);
                SeperateRGBAandAlphaChannel(path);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Finish Departing.");

    }

    public static void SeperateRGBAandAlphaChannel(string _texPath)
    {
        // 获得相对AssetPath.
        string relativePath = GetRelativeAssetPath(_texPath);

        // Set readable flag and set textureFormat TureColor.
        SetTextureReadableEx(relativePath);

        Texture2D originalTex = AssetDatabase.LoadAssetAtPath(relativePath, typeof(Texture2D)) as Texture2D;

        if (null == originalTex)
        {
            Debug.LogError("Load Texture Failed: " + relativePath);
            return;
        }


        // 判断是否有透明通道.
        if (!IsHaveAlphaChannel(originalTex))
        {
            Debug.LogError("not found texture alpha channel, texture path is " + relativePath);
            return;
        }


        // 获得原始Tex的所有Color[]
        Color[] colors = originalTex.GetPixels();

        // 这里不对mipMap处理 直接输出RGB24的不含有透明通道的图.
        Texture2D rgbTex = new Texture2D(originalTex.width, originalTex.height, TextureFormat.RGB24, false);
        rgbTex.SetPixels(colors);
        rgbTex.Apply();

        // 这里都没有透明通道了 为什么还用PNG处理.
        byte[] bytes = rgbTex.EncodeToPNG();
        string rgbPath = GetRGBTexturePath(relativePath);
        File.WriteAllBytes(rgbPath, bytes);
        AddToReimportList(rgbPath, originalTex.width, originalTex.height);



        // Alpha Colors为什么贴图要用RGB24.
        Texture2D alphaTex = new Texture2D(originalTex.width, originalTex.height, TextureFormat.RGB24, false);
        Color[] alphaColors = new Color[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            alphaColors[i].r = colors[i].a;
            alphaColors[i].g = colors[i].a;
            alphaColors[i].b = colors[i].a;
        }

        alphaTex.SetPixels(alphaColors);
        alphaTex.Apply();
        bytes = alphaTex.EncodeToPNG();
        string alphaPath = GetAlphaTexturePath(relativePath);
        File.WriteAllBytes(alphaPath, bytes);
        AddToReimportList(alphaPath, originalTex.width, originalTex.height);




        //// 获得TextureImporter对象.
        //TextureImporter ti = null;
        //try
        //{
        //    ti = (TextureImporter)TextureImporter.GetAtPath(relativePath);
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError("Load Texture failed: " + relativePath);
        //    return;
        //}

        //if (ti == null)
        //{
        //    return;
        //}


        //// 在内存里建立一块Texture2D
        //bool bGenerateMipMap = ti.mipmapEnabled;
        //Texture2D rgbTex = new Texture2D(sourcetex.width, sourcetex.height, TextureFormat.RGB24, bGenerateMipMap);
        //rgbTex.SetPixels(sourcetex.GetPixels());

        //// 建立一块带透明通道的RGBA32 mipMap
        //Texture2D mipMapTex = new Texture2D(sourcetex.width, sourcetex.height, TextureFormat.RGBA32, true);
        //mipMapTex.SetPixels(sourcetex.GetPixels());
        //mipMapTex.Apply(); // Apply是什么意思.

        //// Second level of Mipmap.
        //// 这里没用原始的Base 在内存里先打开mipmap 然后取了512 * 512
        //Color[] colors2rdLevel = mipMapTex.GetPixels(1);

        //// 这个Color不是二维的.. 可能内部有映射关系
        //// 只保留一个通道的话 为什么用Color对象?
        //Color[] colorsAlpha = new Color[colors2rdLevel.Length];

        //int nTestValue = ((mipMapTex.width + 1) / 2 * (mipMapTex.height + 1) / 2);
        //if (colors2rdLevel.Length != nTestValue)
        //{
        //    Debug.LogError("Size Error.");
        //    return;
        //}




    }

    public static string GetAlphaTexturePath(string originalPath)
    {
        string tmp = originalPath.Replace(".", "_Alpha.");
        string postfix = GetPathPostfix(originalPath);
        return tmp.Replace(postfix, ".png");
    }

    public static void AddToReimportList(string relativePath, int texWidth, int texHeight)
    {
        if (generatedTextures.ContainsKey(relativePath))
        {
            return;
        }

        generatedTextures.Add(relativePath, new int[]{texWidth, texHeight});
    }

    public static string GetRGBTexturePath(string originalPath)
    {
        string tmp = originalPath.Replace(".", "_RGB.");

        string postfix = GetPathPostfix(originalPath);

        return tmp.Replace(postfix, ".png");

    }

    public static string GetPathPostfix(string path)
    {
        int index = path.IndexOf(".");
        return path.Substring(index);
    }


    public static bool IsHaveAlphaChannel(Texture2D tex)
    {
        Color[] colors = tex.GetPixels();

        foreach (Color color in colors)
        {
            if (color.a < 1.0f - 0.001f)
            {
                return true;
            }
        }

        return false;
    }

    public static void SetTextureReadableEx(string _relativeAssetPath)
    {
        TextureImporter ti = null;
        try
        {
            ti = (TextureImporter) TextureImporter.GetAtPath(_relativeAssetPath);
        }
        catch (Exception e)
        {
            Debug.LogError("Load Texture failed: " + _relativeAssetPath);
            return;
        }

        if (ti == null)
        {
            return;
        }

        ti.isReadable = true;
        //ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        AssetDatabase.ImportAsset(_relativeAssetPath);
    }

    public static void ReimportGeneratedTextures()
    {
        TextureImporter ti = null;
        foreach (var item in generatedTextures)
        {
            ti = (TextureImporter)TextureImporter.GetAtPath(item.Key);
            if (ti == null)
            {
                Debug.Log("texture importer is null, asset path is " + item.Key);
                continue;
            }
            ti.textureType = TextureImporterType.Default;
            ti.isReadable = false;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.anisoLevel = 1;
            ti.maxTextureSize = Mathf.Max(item.Value[0], item.Value[1]);
            ti.textureCompression = TextureImporterCompression.Compressed;
            AssetDatabase.ImportAsset(item.Key);
        }

    }

    public static string GetRelativeAssetPath(string _fullPath)
    {
        _fullPath = GetRightFormatPath(_fullPath);
        int idx = _fullPath.IndexOf("Assets");

        string assetRelativePath = _fullPath.Substring(idx);
        return assetRelativePath;
    }

    public static string GetRightFormatPath(string _path)
    {
        return _path.Replace("\\", "/");
    }

    public static bool GetDefaultWhiteTexture()
    {
        defaultWhiteTex = AssetDatabase.LoadAssetAtPath(defaultWhiteTexPath_relative, typeof(Texture2D)) as Texture2D;  //not just the textures under Resources file  
        if (!defaultWhiteTex)
        {
            Debug.LogError("Load Texture Failed : " + defaultWhiteTexPath_relative);
            return false;
        }
        return true;
    }

    public static bool IsTextureFile(string _path)
    {
        string path = _path.ToLower();

        return path.EndsWith(".psd") || path.EndsWith(".tga") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".bmp") || path.EndsWith(".tif") || path.EndsWith(".gif");

    }

    public static bool IsTextureConverted(string _path)
    {
        return _path.Contains("_RGB.") || _path.Contains("_Alpha.");
    }
}
