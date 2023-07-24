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

    public void Hurt(float Damage)
    {
        Character.Hurt(Damage);
    }
}
