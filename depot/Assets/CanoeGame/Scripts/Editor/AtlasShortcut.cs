using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEditor.AssetImporters;

public static class AtlasShortcut
{
    new static Texture2D R, G, B, A;
    new static int Format, width, height; //Format = 0 jpg, 1 png
    new static string Name, Path;

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

    private static void MakeAtlas()
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
                //Name = Tex.name;
                //Removes unwanted texture identifiers

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

    new static Color[] ColorArray(Texture2D Atlas) //Where the magic happens
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

    private static void ExportAtlas(Texture2D Atlas, string FormatTxt, byte[] tex)
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

    private static void FixInput(int Iterator) //Makes textures readable
    {
        Texture2D[] Tex = { R, G, B, A };
        TextureImporter Import = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]));
        Import.isReadable = true;

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]), ImportAssetOptions.ForceUpdate);
        Tex[Iterator] = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath((UnityEngine.Object)Tex[Iterator]), typeof(Texture2D));
    }

    //Rewritten to take scriptableobject selection as input
    [MenuItem("Assets/- Snowstorm -/Compile Atlas")]
    public static void BatchConvert()
    {
        foreach (AtlasEditor Editor in Selection.objects)
        {
            R = Editor.R_Metallic;
            G = Editor.G_MixedAO;
            B = Editor.B_Roughness;
            A = Editor.A_Height;
            Format = Editor.Format;
            Name = Editor.Name;

            MakeAtlas();
        }
    }
}