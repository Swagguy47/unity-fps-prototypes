using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//For Air Vehicles
public class AircraftBrain : MonoBehaviour
{
    [HideInInspector] public CharacterBrain Driver;
    [SerializeField] private Interactable Interactor;
    [SerializeField] private Transform SeatPos, ExitPos, ThrustVector;
    //[SerializeField] private Vector3 ThrustVector;
    [SerializeField] private float RotationSensitivity = 20, RollSensitivity = 20, AccelerationThrust = 30, IdleThrust = 20, DecelerationThrust = 10, GlidePower = 3;

    [SerializeField] private CameraTrack VehicleCameraTrack;
    [SerializeField] private bool Glides;

    private ConstantForce ThisForce;
    private Rigidbody ThisRb;

    private void Start()
    {
        ThisForce = gameObject.GetComponent<ConstantForce>();
        ThisForce.enabled = false;
        ThisRb = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Driver != null)
        {
            if (Driver.Possessed)
            {
                ThisForce.relativeTorque = new Vector3(Input.GetAxis("Vertical") * (RotationSensitivity * 10000), Input.GetAxis("Leaning") * (RotationSensitivity * 10000), -Input.GetAxis("Horizontal") * (RollSensitivity * 1000));

                if (Input.GetAxis("Sprint") > 0)
                {
                    ThisRb.AddForce(ThrustVector.forward * (AccelerationThrust * 10000), ForceMode.Force);
                }
                else if (Input.GetAxis("Crouch") > 0)
                {
                    ThisRb.AddForce(ThrustVector.forward * (DecelerationThrust * 10000), ForceMode.Force);
                }
                else
                {
                    ThisRb.AddForce(ThrustVector.forward * (IdleThrust * 10000), ForceMode.Force);
                }
            }
            Driver.LockRot = SeatPos.rotation;
        }

        if (Glides) //Faux aerodynamics
        {
            Debug.DrawRay(transform.position, ThisRb.velocity - (transform.TransformDirection(Vector3.forward) * (Mathf.Abs(ThisRb.velocity.x) + Mathf.Abs(ThisRb.velocity.y) + Mathf.Abs(ThisRb.velocity.z))));
            //ThisRb.velocity = (ThisRb.velocity - (transform.TransformDirection(Vector3.back) * Mathf.Clamp01(ThisRb.velocity.x + ThisRb.velocity.y + ThisRb.velocity.z)));// * (Mathf.Abs(ThisRb.velocity.x) + Mathf.Abs(ThisRb.velocity.y) + Mathf.Abs(ThisRb.velocity.z)))
            ThisRb.AddForce(ThisRb.velocity - (transform.TransformDirection(Vector3.forward) * (((ThisRb.velocity.x) + (ThisRb.velocity.y) + (ThisRb.velocity.z))) * GlidePower), ForceMode.Impulse);
        }
    }

    public void VehicleEnter()
    {
        if (Driver == null && !Interactor.Interactor.IgnoreVehicles)
        {
            ThisForce.enabled = true;
            Driver = Interactor.Interactor;
            Driver.CharacterCollission.enabled = false;
            Driver.Seated = true;
            Driver.LockVisualRot = true;
            Driver.transform.SetPositionAndRotation(SeatPos.position, SeatPos.rotation);
            Driver.transform.parent = transform;
            if (VehicleCameraTrack != null)
            {
                Driver.CameraTrackOverride = VehicleCameraTrack;
            }
        }
    }

    public void VehicleExit()
    {
        if (Driver != null)
        {
            ThisForce.enabled = false;
            Driver.transform.parent = null;
            Driver.LockVisualRot = false;
            Driver.RecenterVisuals(); //to recenter the character model to local rotation
            Driver.Seated = false;
            Driver.transform.SetPositionAndRotation(ExitPos.position, new Quaternion(0,0,0,0));
            Driver.CharacterCollission.enabled = true;
            Driver.Possess(); //so it'll refresh the rigidbody
            if (VehicleCameraTrack != null)
            {
                Driver.CameraTrackOverride = null;
            }
            Driver = null;
        }
    }
}
