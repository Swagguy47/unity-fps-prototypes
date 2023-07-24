using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAnimForwarder : MonoBehaviour
{
    [SerializeField] private CharacterBrain Character;
    public void StopClamber()
    {
        Character.clambering = false;
        if (Character.Possessed)
        {
            Character.ThisRb.isKinematic = false;
        }
    }

    //For ending weapon unequip sequence by disabling animator's "SwapWeapon" bool
    public void StopSwapping()
    {
        Character.CharacterAnimator.SetBool("SwapWeapon", false);
        Character.UpdateWeapon();
    }

    //Reloading mechanics
    public void MagOut()
    {
        Character.MagOut();
    }

    public void MagIn()
    {
        Character.MagIn();
    }

    public void EndOverride()
    {
        Character.EndOverride();
    }
}
