using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.AssetImporters;

public static class AtlasSetup
{
    //Rewritten to take scriptableobject selection as input
    [MenuItem("Assets/- Snowstorm -/Setup Atlas")]
    public static void BatchSetup()
    {
        foreach (AtlasEditor Editor in Selection.objects)
        {
            string EditorPath = AssetDatabase.GetAssetPath(Editor).Replace(".asset", "");

            Editor.R_Metallic = (Texture2D)AssetDatabase.LoadAssetAtPath(EditorPath + "_Metallic.png", typeof(Texture2D));
            Editor.G_MixedAO = (Texture2D)AssetDatabase.LoadAssetAtPath(EditorPath + "_Mixed_AO.png", typeof(Texture2D));
            Editor.B_Roughness = (Texture2D)AssetDatabase.LoadAssetAtPath(EditorPath + "_Roughness.png", typeof(Texture2D));
            Editor.A_Height = (Texture2D)AssetDatabase.LoadAssetAtPath(EditorPath + "_Height.png", typeof(Texture2D));
            Editor.Format = 1;
            Editor.Name = Editor.name + "_Atlas";
            //Debug.Log(AssetDatabase.GetAssetPath(Editor));
        }
    }
}