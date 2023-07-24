using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    [SerializeField] Transform SeatPos, ExitPos, PaddleLeft, PaddleRight, LeftIK, RightIK;
    [SerializeField] float Speed = 2, PaddleAngularVel = 5;
    [SerializeField] bool Motor, HandIK;
    [SerializeField] Interactable Interactor;
    [SerializeField] Animator PaddleAnimator;

    CharacterBrain Driver; //for lack of better term
    Rigidbody RB;
    [HideInInspector] public float DriveTime = 0; //for delaying exit inputs
    float LeftAccel, RightAccel;
    [HideInInspector] public bool LeftPaddle, RightPaddle;

    //Make hidden when finished debugging
    public bool AiControlled;
    public Vector3 AiEndPoint;

    Seeker AiSeeking;
    AIPath AiPathing;

    private void Start() {
        RB = GetComponent<Rigidbody>();
        if (AiControlled) //temporary if statement
        {
            AiSeeking = GetComponent<Seeker>();
            AiPathing = GetComponent<AIPath>();
        }
    }

    //Controls the boat on physupdate
    private void Update() {

        PaddleAnimator.SetBool("Stowed", ((Driver == null && !AiControlled) || Motor)); //paddle stowed
        if (AiControlled) {
            AiMovement();
        }
        if (Driver != null) //being controlled
        {
            if (Driver.Possessed) //controlled by player
            {
                if (!Motor)
                {
                    PaddleAnimator.SetBool("Left", Input.GetButton("Fire")); //left paddle
                    PaddleAnimator.SetBool("Right", Input.GetButton("Fire2")); //right paddle
                }
                else
                {
                    RB.AddRelativeForce((Vector3.forward * Speed * Input.GetAxis("Vertical")), ForceMode.Acceleration);
                    RB.AddTorque((Vector3.up * PaddleAngularVel / 3) * Input.GetAxis("Horizontal"), ForceMode.Acceleration);
                }
            }
        }
        else if(!AiControlled){
            PaddleAnimator.SetBool("Left", false); //left paddle
            PaddleAnimator.SetBool("Right", false); //right paddle
        }

        if (DriveTime < 2)
        {
            DriveTime += Time.deltaTime;
        }
    }

    public void FixedUpdate() //paddle acceleration
    {
        if (LeftPaddle)
        {
            RB.AddForceAtPosition((PaddleLeft.forward * Speed) * LeftAccel, PaddleLeft.position, ForceMode.Acceleration);
            RB.AddTorque((Vector3.up * PaddleAngularVel) * LeftAccel, ForceMode.Acceleration);
            LeftAccel += Time.fixedDeltaTime;
        }
        if (RightPaddle)
        {
            RB.AddForceAtPosition((PaddleRight.forward * Speed) * RightAccel, PaddleRight.position, ForceMode.Acceleration);
            RB.AddTorque((Vector3.up * -PaddleAngularVel) * RightAccel, ForceMode.Acceleration);
            RightAccel += Time.fixedDeltaTime;
        }
        if (Mathf.Abs(transform.rotation.z) > 0.5f) //flipped mechanics
        {
            Exit();
        }
    }

    public void Enter() {
        DriveTime = 0;
        Driver = Interactor.Interactor;
        Driver.CharacterCollission.enabled = false;
        Driver.Seated = true;
        /*Driver.w_Ability = SeatWeapon;
        Driver.CurrentWeapon = 2;
        Driver.UpdateWeapon();*/
        Driver.LockVisualRot = true;
        Driver.transform.SetPositionAndRotation(SeatPos.position, SeatPos.rotation);
        Driver.transform.parent = transform;
        Driver.CurrentBoat = this;
        Interactor.gameObject.SetActive(false);
        /*if (HandIK) {
            Driver.LeftArmIK.solver.target = LeftIK;
            Driver.RightArmIK.solver.target = RightIK;
            Driver.LeftArmIK.solver.IKPositionWeight = 1;
            Driver.RightArmIK.solver.IKPositionWeight = 1;
        }*/
        
    }

    public void Exit() {
        if (Driver != null && DriveTime >= 2)
        {
            Driver.transform.parent = null;
            Driver.LockVisualRot = false;
            Driver.RecenterVisuals(); //to recenter the character model to local rotation
            Driver.Seated = false;
            Driver.transform.SetPositionAndRotation(ExitPos.position, new Quaternion(0, 0, 0, 0));
            Driver.CharacterCollission.enabled = true;
            //Driver.Possess(); //so it'll refresh the rigidbody
            Driver.ThisRb.isKinematic = false;
            Driver.ThisRb.AddForce(RB.velocity, ForceMode.VelocityChange); //exiting character keeps the vehicles momentum as they leave
            /*Driver.CurrentWeapon = 0;
            Driver.UpdateWeapon();*/
            Interactor.gameObject.SetActive(true);
            /*if (HandIK)
            {
                Driver.LeftArmIK.solver.IKPositionWeight = 0;
                Driver.RightArmIK.solver.IKPositionWeight = 0;
                Driver.LeftArmIK.solver.target = null;
                Driver.RightArmIK.solver.target = null;
            }*/
            Driver = null;
        }
    }

    public void LeftPaddleIn()
    {
        LeftAccel = 0;
        LeftPaddle = true;
    }

    public void RightPaddleIn()
    {
        RightAccel = 0;
        RightPaddle = true;
    }

    public void LeftPaddleOut()
    {
        LeftPaddle= false;
    }

    public void RightPaddleOut()
    {
        RightPaddle= false;
    }

    public void AiMovement()
    {
        if (AiSeeking.GetCurrentPath() != null) {
            AiSeeking.StartPath(transform.position, AiEndPoint);
            //AiSeeking.GetCurrentPath().path[0].position)
            if (AiSeeking.GetCurrentPath().path.Count > 0)
            {
                if (Vector3.Angle(transform.position, (Vector3)AiSeeking.GetCurrentPath().path[1].position) < 15) //paddle to the left
                {
                    Debug.Log("PADDLE LEFT");
                    PaddleAnimator.SetBool("Left", false);
                    PaddleAnimator.SetBool("Right", true);
                }
                else if (Vector3.Angle(transform.position, (Vector3)AiSeeking.GetCurrentPath().path[1].position) > -15) //paddle to the right
                {
                    Debug.Log("PADDLE RIGHT");
                    PaddleAnimator.SetBool("Left", true);
                    PaddleAnimator.SetBool("Right", false);
                }
                else //paddle forward
                {
                    Debug.Log("PADDLE FORWARD");
                    PaddleAnimator.SetBool("Left", true);
                    PaddleAnimator.SetBool("Right", true);
                }
            }

            /*if (Mathf.DeltaAngle(transform.rotation.eulerAngles.y - 90, AiPathing.steeringTarget.y) < 15) //paddle to the left
            {
                Debug.Log("PADDLE LEFT");
                PaddleAnimator.SetBool("Left", false);
                PaddleAnimator.SetBool("Right", true);
            }
            else if (Mathf.DeltaAngle(transform.rotation.eulerAngles.y - 90, AiPathing.steeringTarget.y) > -15) //paddle to the right
            {
                Debug.Log("PADDLE RIGHT");
                PaddleAnimator.SetBool("Left", true);
                PaddleAnimator.SetBool("Right", false);
            }
            else //paddle forward
            {
                Debug.Log("PADDLE FORWARD");
                PaddleAnimator.SetBool("Left", true);
                PaddleAnimator.SetBool("Right", true);
            }*/
        }
        else {
            AiSeeking.StartPath(transform.position, AiEndPoint);
        }
        
    }

    //debugging
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (AiSeeking != null) {
            if (AiSeeking.GetCurrentPath() != null)
            {
                //pathing nodes: (Vector3)AstarPath.active.GetNearest(transform.position).node.position
                Gizmos.DrawSphere(AiSeeking.GetCurrentPath().vectorPath[1], 1f);
                //Gizmos.DrawSphere((Vector3)AiSeeking.GetCurrentPath().path[1].position, 1f);
            }
        }
    }
}
