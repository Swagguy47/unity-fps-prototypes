using UnityEngine;

[CreateAssetMenu(fileName = "Character Class", menuName = "ArcticGame/Character Class")]
public class CharacterClass : ScriptableObject
{
    //Ensure this is unique for each class, will be used to identify it in scripts
    public int ClassValue;

    //Character stats
    public float MoveSpeed = 3f, SprintMult = 0.6f, JumpHeight = 3f, LeanAngle = 30f;

    //Class allowed weapons:
    public Weapon[] Primaries, Secondaries;
    //Having multiple means each character will randomly select a gun from the array on start

    //Per-limb Armor points:
    public float HeadAP = 5, BodyAP = 15, LArmAP = 10, RArmAP = 10, LLegAP = 10, RLegAP = 10;

    //Mech specific
    public bool TopGimbal, IgnoreVehicles;
}
