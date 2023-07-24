using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.AssetImporters;

public class AtlasTool : EditorWindow
{
    public Texture2D R, G, B, A;
    public int Format, width, height; //Format = 0 jpg, 1 png
    public string Name, Path;

    private static Texture2D TextureField(string name, Texture2D texture)
    {
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 80;
        GUILayout.Label(name, style);
        var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
        GUILayout.EndVertical();
        return result;
    }

    //DEPRECATED, Works fine, but use the new ScriptableObject based system, its much more effecient
    //[MenuItem("Snowstorm Tools/Texture Atlas Generator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AtlasTool));
    }

    private void OnGUI()
    {
        GUILayout.Space(15f);
        GUILayout.Label("Takes multiple black and white textures\nand combines them into a single atlas to be\nused in custom shaders (more effecient)");
        GUILayout.Space(5f);
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        //Format input
        string[] options = new string[]
        {
                ".jpg", ".png",
        };
        Format = EditorGUILayout.Popup("Atlas Format:", Format, options);

        EditorGUILayout.BeginHorizontal();

        //Texture input
        R = TextureField("(R) Metallic", R);
        G = TextureField("(G) Mixed AO", G);
        B = TextureField("(B) Roughness", B);

        //Optional alpha field if output format is png
        if (Format == 1)
        {
            A = TextureField("(A) Height", A);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        //Begins the operation
        if (R != null || G != null || B != null || A != null)
        {
            if (GUILayout.Button("Generate Atlas"))
            {
                MakeAtlas();
            }
        }
    }


    private void MakeAtlas()
    {
        int TexIteration = 0;
        //Finds texture resolution, path & name for atlas
        foreach (Texture2D Tex in new Texture2D[] { R, G, B, A })
        {
            if (Tex != null)
            {
                //Output resolution
                width = Tex.width;
                height = Tex.height;

                //Output name
                Name = Tex.name;
                //Removes unwanted texture identifiers
                Name.Replace("_Metallic", "");
                Name.Replace("_Mixed_AO", "");
                Name.Replace("_Roughness", "");
                Name.Replace("_Height", "");
                Name += "_Atlas";

                //Gets output path
                string fullpath = AssetDatabase.GetAssetPath((UnityEngine.Object)Tex);
                Path = fullpath.Substring(0, fullpath.IndexOf(((UnityEngine.Object)Tex).name));

                //break;
                FixInput(TexIteration); //Makes readable
            }
            TexIteration++;
        }

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

    private Color[] ColorArray(Texture2D Atlas) //Where the magic happens
    {

        Color[] cl = new Color[width * height];

        for (int j = 0; j < cl.Length; j++)
        {

            cl[j] = new Color();
            //R -----------------------------------------------
            if (R != null) 
                cl[j].r = R.GetPixel(j % width, j / width).r;
            else
                cl[j].r = 0;
            //G -----------------------------------------------
            if (G != null) 
                cl[j].g = G.GetPixel(j % width, j / width).r;
            else
                cl[j].g = 0;
            //B -----------------------------------------------
            if (B != null) 
                cl[j].b = B.GetPixel(j % width, j / width).r;
            else
                cl[j].b = 0;
            //A -----------------------------------------------
            if (A != null && Format != 0) 
                cl[j].a = A.GetPixel(j % width, j / width).r;
            else
                cl[j].a = 0;
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
        Texture2D[] Tex = { R, G, B, A };
        TextureImporter Import = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]));
        Import.isReadable = true;

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]), ImportAssetOptions.ForceUpdate);
        Tex[Iterator] = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]), typeof(Texture2D));
    }
}