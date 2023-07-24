using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ArcticGame/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Weapon Visuals")]
    public GameObject Model; //Prefab added to weapon handle when equipped
    public float BarrelLength; // For setting raycast distance when lowering weapon near walls & weapon sway intensity
    public Vector3 FirstPersonOffset = new Vector3(-0.03f, -0.3f, -0.1f); //position offset of first person character while holding weapon

    [Header("Weapon Animations")]
    public float SwayIntensity; //Procedural weapon sway
    public AnimatorOverrideController WeaponAnimator;
    //public AnimationClip a_Equip, a_Unequip, a_Idle, a_Sprint, a_Fire, a_Reload, a_ReloadShort; //Animations which will be played via script

    [Header("Weapon Stats")]
    public bool Automatic;
    public int AmmoClip, AmmoSpare, BurstAmount; //AmmoClip is max ammo in a clip you reload, AmmoSpare is extra inventory ammo 
    public float BurstRate, FireRate, Damage, RecoilBloom, RecoilSpring, BulletDistance; //AiRange;

    //Internal ammo count, to be modified from scripts only
    [HideInInspector] public int a_CurrentClip, a_Extra;
    [HideInInspector] public float i_FireRate;
}
