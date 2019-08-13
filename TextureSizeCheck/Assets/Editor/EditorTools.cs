using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorTools{

    private static string defaultWhiteTexPath_relative = "Assets/Default_Alpha.png";

    private static Texture2D defaultWhiteTex = null;


    [MenuItem("EffortForETC1/Depart RGB and Alpha Channel")]
    public static void SeperateAllTexturesRGBandAlphaChannel()
    {
        Debug.Log("HERE");


        Debug.Log("Start Departing.");
        if (!GetDefaultWhiteTexture())
        {
            return;
        }

        
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
}
