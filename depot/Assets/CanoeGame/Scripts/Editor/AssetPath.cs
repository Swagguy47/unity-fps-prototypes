using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.AssetImporters;

public class AssetPath : EditorWindow
{
    string AssetDirectory;
    private static Texture2D TextureField(string name, Texture2D texture)
    {
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 140;
        GUILayout.Label(name, style);
        var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
        GUILayout.EndVertical();
        return result;
    }

    //DEPRECATED, Works fine, but use the new ScriptableObject based system, its much more effecient
    [MenuItem("Snowstorm Tools/Path Finder")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AssetPath));
    }

    private void OnGUI()
    {
        GUILayout.Space(15f);
        GUILayout.Label("Finds Asset Paths");
        GUILayout.Space(5f);
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Space(5f);
        GUILayout.Label(AssetDirectory);
        //EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find Path"))
        {
            SelectedFolder();
        }
    }

    private void SelectedFolder()
    {
        var Getpath = "";
        var obj = Selection.activeObject;
        if (obj == null) Getpath = "Assets";
        else Getpath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        if (Getpath.Length > 0)
        {
            AssetDirectory = Getpath;
        }
    }
}