using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HitboxCharForwarder : MonoBehaviour
{
    //Dumbs down damage void from CharacterBrain into a bunch of different functions to easily be called by
    //Unity events by hitboxes, then forwards to character brain as proper function call thingy

    private CharacterBrain Character;
    void Start()
    {
        Character= gameObject.GetComponent<CharacterBrain>();
    }

    public void HurtHead(Hitbox Hitbox)
    {
        Character.Hurt(0, Hitbox.DamageTaken);
    }
    public void HurtBody(Hitbox Hitbox)
    {
        Character.Hurt(1, Hitbox.DamageTaken);
    }
    public void HurtLArm(Hitbox Hitbox)
    {
        Character.Hurt(2, Hitbox.DamageTaken);
    }
    public void HurtRArm(Hitbox Hitbox)
    {
        Character.Hurt(3, Hitbox.DamageTaken);
    }
    public void HurtLLeg(Hitbox Hitbox)
    {
        Character.Hurt(4, Hitbox.DamageTaken);
    }
    public void HurtRLeg(Hitbox Hitbox)
    {
        Character.Hurt(5, Hitbox.DamageTaken);
    }
}
