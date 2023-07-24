using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//For Ground Vehicles
public class VehicleBrain : MonoBehaviour
{
    //Character who's in control of the vehicle
    [HideInInspector] public CharacterBrain Driver;

    //Interactor callback to find the character who will become the driver
    [SerializeField] private Interactable Interactor;

    //SeatPos is where the occupant will be located, ExitPos is where they will appear when they get out
    [SerializeField] private Transform SeatPos, ExitPos;

    //Wheels
    [SerializeField] private WheelCollider[] Wheels;
    //0-FL
    //1-FR
    //2-BL
    //3-BR

    //Stats
    [SerializeField] private float EnginePower = 300f, MaxSteerAngle = 45f;

    //Overrides the character's thirdperson camera track to better fit the vehicle on screen
    [SerializeField] private CameraTrack VehicleCameraTrack;

    //Animates the vehicle if assigned, but is not essential for function, so that subseats can exist without animation
    [SerializeField] private Animator VehicleAnimator;

    //Subseats for passengers
    [SerializeField] private VehicleBrain[] SubSeats;

    //"Weapon" for posing character in seat, not for actual combat
    [SerializeField] private Weapon SeatWeapon;

    //General health of vehicle
    [SerializeField] private int VehicleHealth = 100;
    float Hp;
    Rigidbody Rb;

    private void Start()
    {
        Hp = VehicleHealth;
        Rb= GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Hp > 0) //check to ensure vehicle is not destroyed
        {
            if (Driver != null)
            {
                if (Driver.Possessed)
                {
                    //Player controls the wheels of the vehicle
                    int WheelCount = 0;
                    foreach (WheelCollider Wheel in Wheels)
                    {
                        Wheel.motorTorque = Input.GetAxis("Vertical") * EnginePower;
                        Wheel.brakeTorque = Input.GetAxis("Sprint") * (EnginePower * 1.5f);
                        if (WheelCount < 2)
                        {
                            Wheel.steerAngle = Input.GetAxis("Horizontal") * MaxSteerAngle;
                            WheelCount++;
                        }
                    }

                    //if (Input.GetButtonDown("Interact"))
                    //{
                    //    VehicleExit();
                    //}
                }

                //Aligns driver character to orientation of vehicle
                Driver.LockRot = SeatPos.rotation;
            }

            AnimateVehicle();
        }
    }

    public void VehicleEnter()
    {
        if (Driver == null && Hp > 0 && !Interactor.Interactor.IgnoreVehicles)
        {
            Driver = Interactor.Interactor;
            Driver.CharacterCollission.enabled = false;
            Driver.Seated = true;
            Driver.w_Ability = SeatWeapon;
            Driver.CurrentWeapon = 2;
            Driver.UpdateWeapon();
            Driver.LockVisualRot = true;
            Driver.transform.SetPositionAndRotation(SeatPos.position, SeatPos.rotation);
            Driver.transform.parent = transform;
            if (VehicleCameraTrack != null)
            {
                Driver.CameraTrackOverride= VehicleCameraTrack;
            }
        }
        else if(SubSeats != null && Hp > 0) //if driver spot is taken, attempt to become passenger
        {
            foreach (VehicleBrain Seat in SubSeats) //finds any open passenger seat
            {
                Debug.Log("Searching subseat: " + Seat.name);
                if (Seat.Driver == null)
                {
                    Seat.VehicleEnter();
                    Debug.Log("This seat is empty :D");
                    break;
                }
            }
        }
    }

    public void VehicleExit()
    {
        if (Driver != null)
        {
            foreach (WheelCollider Wheel in Wheels)
            {
                Wheel.motorTorque = 0;
                Wheel.brakeTorque = EnginePower;
            }
            Driver.transform.parent = null;
            Driver.LockVisualRot= false;
            Driver.RecenterVisuals(); //to recenter the character model to local rotation
            Driver.Seated = false;
            Driver.transform.SetPositionAndRotation(ExitPos.position, new Quaternion(0,0,0,0));
            Driver.CharacterCollission.enabled = true;
            //Driver.Possess(); //so it'll refresh the rigidbody
            Driver.ThisRb.isKinematic = false;
            Driver.ThisRb.AddForce(Rb.velocity, ForceMode.VelocityChange); //exiting character keeps the vehicles momentum as they leave
            if (VehicleCameraTrack != null)
            {
                Driver.CameraTrackOverride = null;
            }
            Driver.CurrentWeapon = 0;
            Driver.UpdateWeapon();
            Driver = null;
        }
    }

    //Collisions that damage vehicle
    private void OnCollisionEnter(Collision collision) //angular velocity is ignored
    {
        if (collision.relativeVelocity.magnitude > 12 && Hp > 0)
        {
            DamageVehicle(collision.relativeVelocity.magnitude);
            Debug.Log("IMPACT! " + collision.relativeVelocity.magnitude + " Damage. Hp Remaining: " + Hp);
            if (collision.gameObject.layer == 6)
            {
                collision.gameObject.GetComponent<CharacterBrain>().Hurt(1, collision.relativeVelocity.magnitude * 3);
            }
        }
    }

    //Damage for impacts or weaponry
    public void DamageVehicle(float Damage)
    {
        Hp -= Damage;
        if (Hp <= 0)
        {
            DestroyVehicle();
        }
    }

    //Destroys vehicle and kills any occupying characters
    public void DestroyVehicle()
    {
        if (Driver != null)//kills driver
        {
            Driver.Die();
        }

        if (SubSeats != null) //kills all passengers
        {
            foreach (VehicleBrain Seat in SubSeats)
            {
                if (Seat.Driver != null)
                {
                    Seat.Driver.Die();
                }
                Destroy(Seat.gameObject);
            }
        }

        Debug.Log("!!VEHICLE DESTROYED!! ( " + gameObject.name + " )");

        foreach (WheelCollider Wheel in Wheels)
        {
            Wheel.motorTorque = 0;
            Wheel.brakeTorque = EnginePower / 2; //not fully braking so it'll glide for a bit.
        }
        Destroy(Interactor.gameObject);
        Driver = null;
        //One last animator call to update damage states
        AnimateVehicle();
        //Destroy(gameObject);
    }

    //drives animator parameters for the visuals
    private void AnimateVehicle()
    {
        //The animator (if applied) should have all of these parameters, but does not need to make use of all of them

        if (VehicleAnimator != null)
        {
            //If driver seat is occupied 'Active' is true
            VehicleAnimator.SetBool("Active", (Driver != null));

            //'WheelVelocity' correlates to the rpm of the first wheel, assumes all other wheels have similar rpm
            VehicleAnimator.SetFloat("WheelVelocity", Wheels[0].rpm / 30);

            //'SteerAngle' refers to the angle the front two wheels will be at, only samples from wheel 0, but both should be identical. Divides angle from 0-180 to 0-1. forward position should be at 0.5
            VehicleAnimator.SetFloat("SteerAngle", (Wheels[0].steerAngle / 180) + 0.5f);

            //'Health' refers to the internal health value of the vehicle, this way the animator can simply swap to different damage states on the fly
            VehicleAnimator.SetFloat("Health", Hp);
        }
    }
}
