
//using UnityEditor;
using UnityEngine;

public class StaticItemPool : MonoBehaviour
{
    public static ItemSandbox Items;
    public ItemSandbox ItemThang;

    void Start()
    {
        Items = ItemThang;
        //Items = (ItemSandbox)AssetDatabase.LoadAssetAtPath("Assets/CanoeGame/ScriptableObjs/Sandbox.asset", typeof(ItemSandbox));
    }

}
