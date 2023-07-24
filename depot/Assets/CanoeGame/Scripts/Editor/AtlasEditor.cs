using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Atlas Editor", menuName = "ArcticGame/Atlas Editor")]
public class AtlasEditor : ScriptableObject
{
    public string Name = "_Atlas";
    public Texture2D R_Metallic, G_MixedAO, B_Roughness, A_Height;
    public int Format = 1;
}
