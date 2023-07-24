using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RopeCleat : MonoBehaviour
{
    public Rigidbody Base;
    [HideInInspector] public List<RopeInstance> Ropes = new List<RopeInstance>();

    public void Attach()
    {
        RopeInstance Rope = FindRope();
        

        Debug.Log(Rope.UsedEnds);

        if (Rope.UsedEnds <= 2) {

            Ropes.Add(Rope);

            SpringJoint RopeJoint = Base.gameObject.AddComponent<SpringJoint>();

            //sets cleat references in rope
            if (Rope.A == null || Rope.HoldingA)
            {
                Rope.A = this;
                Rope.RopeA = RopeJoint;
                Debug.Log("SET ROPE A to:" + gameObject.name);
                if (Rope.HoldingA)
                {
                    PlayerCallback.PlayerBrain.CurrentCharBrain.WeaponHandle.GetComponent<SpringJoint>().connectedBody = null;
                }
                Rope.HoldingA = false;
                PlayerCallback.PlayerBrain.CurrentCharBrain.HeldRope = null;
            }
            else if (Rope.B == null || Rope.HoldingB) //|| Rope.B.gameObject == PlayerCallback.PlayerBrain.PlayerCharacter.WeaponHandle
            {
                Rope.B = this;
                Rope.RopeB = RopeJoint;
                Debug.Log("SET ROPE B to:" + gameObject.name);
                if (Rope.HoldingB) {
                    PlayerCallback.PlayerBrain.CurrentCharBrain.WeaponHandle.GetComponent<SpringJoint>().connectedBody = null;
                }
                Rope.HoldingB = false;
                PlayerCallback.PlayerBrain.CurrentCharBrain.HeldRope = null;
            }
            if (Rope.UsedEnds > 0)
            {
                if (Rope.A == null) {
                    Rope.A = PlayerCallback.PlayerBrain.CurrentCharBrain.WeaponHandle.GetComponent<RopeCleat>();
                    Rope.RopeA = PlayerCallback.PlayerBrain.CurrentCharBrain.WeaponHandle.GetComponent<SpringJoint>();
                    Rope.HoldingA = true;
                    Debug.Log("SET ROPE A to: WeaponPos");
                    PlayerCallback.PlayerBrain.CurrentCharBrain.HeldRope = Rope;
                }
                else if (Rope.B == null) {
                    Rope.B = PlayerCallback.PlayerBrain.CurrentCharBrain.WeaponHandle.GetComponent<RopeCleat>();
                    Rope.RopeB = PlayerCallback.PlayerBrain.CurrentCharBrain.WeaponHandle.GetComponent<SpringJoint>();
                    Debug.Log("SET ROPE B to: WeaponPos");
                    Rope.HoldingB = true;
                    PlayerCallback.PlayerBrain.CurrentCharBrain.HeldRope = Rope;
                }
                if (Rope.UsedEnds < 2)
                {
                    Rope.RefreshRope();
                }
            }
            Rope.UsedEnds++;
        }
    }

    private RopeInstance FindRope()
    {
        if (PlayerCallback.PlayerBrain.CurrentCharBrain.HeldRope != null)
        {
            RopeInstance Rope = PlayerCallback.PlayerBrain.CurrentCharBrain.HeldRope;
            Debug.Log("Player was already holding a rope :)");
            return Rope;
        }
        else
        {
            RopeInstance Rope = new GameObject().AddComponent<RopeInstance>();
            Rope.gameObject.name = "NewRopeInstance";
            LineRenderer LR = Rope.gameObject.AddComponent<LineRenderer>();
            LR.positionCount = 2;
            LR.widthMultiplier = 0.1f;
            //LR.material = (Material)AssetDatabase.LoadAssetAtPath("Assets/CanoeGame/Textures/Rope.mat", typeof(Material));
            //PlayerCallback.PlayerBrain.PlayerCharacter.HeldRope = Rope;
            Debug.Log("We had to make a new rope for this :(");
            return Rope;
        }
    }
    public void Remove()
    {
        if (Ropes.Count > 0)
        {
            if (Ropes[0].A == this)
            {
                Ropes[0].A = null; //Removes cleat
                Destroy(Ropes[0].RopeA); //Destroys joint
                Ropes[0].RopeA = null; //Removes joint
                Ropes[0].UsedEnds--;
            }
            else
            {
                Ropes[0].B = null; //Removes cleat
                Destroy(Ropes[0].RopeB); //Destroys joint
                Ropes[0].RopeB = null; //Removes joint
                Ropes[0].UsedEnds--;
            }
            CharacterBrain PlayerChar = PlayerCallback.PlayerBrain.CurrentCharBrain;
            PlayerChar.Weapons[PlayerChar.CurrentWeapon] = StaticItemPool.Items.UniqueItem(1);
            Ropes[0].RefreshRope();
            Ropes.Remove(Ropes[0]);
        }
    }
}
