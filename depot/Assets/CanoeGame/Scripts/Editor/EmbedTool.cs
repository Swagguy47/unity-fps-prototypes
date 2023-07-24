using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.AssetImporters;

public class EmbedTool : EditorWindow
{
    public Texture2D Albedo, Alpha;
    public int Format = 1, width, height; //Format = 0 jpg, 1 png
    public string Name, Path;

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
    [MenuItem("Snowstorm Tools/Alpha Embedder")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(EmbedTool));
    }

    private void OnGUI()
    {
        GUILayout.Space(15f);
        GUILayout.Label("Embeds any black and white texture to an alpha channel");
        GUILayout.Space(5f);
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //EditorGUILayout.BeginHorizontal();

        //Texture input
        Albedo = TextureField("Albedo (RGB)", Albedo);

        //Optional alpha field if output format is png
        Alpha = TextureField("Embedded Texture (A)", Alpha);

        //EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Begins the operation
        if (Albedo != null && Alpha != null)
        {
            if (GUILayout.Button("Embed texture!"))
            {
                MakeAtlas();
            }
        }
    }


    private void MakeAtlas()
    {
        Format = 1;
        if (Albedo != null)
        {
            //Output resolution
            width = Albedo.width;
            height = Albedo.height;

            //Output name
            Name = Albedo.name;

            //Gets output path
            string fullpath = AssetDatabase.GetAssetPath((UnityEngine.Object)Albedo);
            Path = fullpath.Substring(0, fullpath.IndexOf(((UnityEngine.Object)Albedo).name));

            //break;
            FixInput(0);
            FixInput(1); //Makes readable

            //Creates new texture
            Texture2D Atlas = new Texture2D(width, height);

            Atlas.SetPixels(ColorArray(Atlas));

            //Preps for filestream
            string FormatTxt = "";
            if (Format == 0)
            {
                byte[] tex = Atlas.EncodeToJPG();
                FormatTxt = ".jpg";
                ExportAtlas(Atlas, FormatTxt, tex);
            }
            else
            {
                byte[] tex = Atlas.EncodeToPNG();
                FormatTxt = ".png";
                ExportAtlas(Atlas, FormatTxt, tex);
            }
        }
    }

    private Color[] ColorArray(Texture2D Atlas) //Where the magic happens
    {

        Color[] cl = new Color[width * height];

        for (int j = 0; j < cl.Length; j++)
        {

            cl[j] = new Color();
            //R -----------------------------------------------
            cl[j].r = Albedo.GetPixel(j % width, j / width).r;
            //G -----------------------------------------------
            cl[j].g = Albedo.GetPixel(j % width, j / width).g;
            //B -----------------------------------------------
            cl[j].b = Albedo.GetPixel(j % width, j / width).b;
            //A -----------------------------------------------
            cl[j].a = Alpha.GetPixel(j % width, j / width).r;
            //-------------------------------------------------
        }

        return cl;
    }

    private void ExportAtlas(Texture2D Atlas, string FormatTxt, byte[] tex)
    {
        //writes the texture data as a new file
        FileStream stream = new FileStream(Path + Name + FormatTxt, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        BinaryWriter writer = new BinaryWriter(stream);
        for (int j = 0; j < tex.Length; j++)
            writer.Write(tex[j]);

        writer.Close();
        stream.Close();

        AssetDatabase.ImportAsset(Path + Name + FormatTxt, ImportAssetOptions.ForceUpdate); //imports atlas
    }

    private void FixInput(int Iterator) //Makes textures readable
    {
        Texture2D[] Tex = { Albedo, Alpha};
        TextureImporter Import = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]));
        Import.isReadable = true;

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]), ImportAssetOptions.ForceUpdate);
        Tex[Iterator] = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]), typeof(Texture2D));
    }
}