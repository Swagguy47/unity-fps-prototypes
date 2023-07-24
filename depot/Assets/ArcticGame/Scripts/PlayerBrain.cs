using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;

public class PlayerBrain : MonoBehaviour
{
    //Player cam & body rotation
    [SerializeField] private float LookSensitivity = 100f, CamSpeed = 5f;
    private Transform CharacterController;
    [HideInInspector] public CharacterBrain CurrentCharBrain;
    [HideInInspector]public float xRot = 0f, XMouse, YMouse;
    private GameObject InteractPrompt, Interactable;

    //Camera interpolation between characters
    private bool Interpolating;

    //Team to filter quick swapping to
    public int PlayerTeam;

    //Default thirdperson camera offset
    public Vector3 DefaultCamTrack;
    [HideInInspector] public bool ThirdPerson;
    private Transform PlayerCam;

    void Start()
    {
        CharacterController = this.transform;
        Cursor.lockState = CursorLockMode.Locked;
        InteractPrompt = GameObject.Find("InteractPrompt");
        InteractPrompt.SetActive(false);
        PlayerCam = GameObject.Find("PlayerCam").transform;
    }

    void Update()
    {
        //Rotates the player & camera
        PlayerRotation();

        //Debug/fallback camera movement
        if (CharacterController == transform)
        {
            transform.position = transform.position + transform.right * ( Input.GetAxis("Horizontal") * CamSpeed * Time.deltaTime ) + transform.forward * ( Input.GetAxis("Vertical") * CamSpeed * Time.deltaTime );
            ThirdPerson = false;

            CamSpeed += Input.GetAxis("Mouse ScrollWheel") * 15; //Debug cam speed controls

            if (CamSpeed < 0) //to ensure the speed cannot go negative
            {
                CamSpeed = 0;
            }

            if (Input.GetButtonDown("Sprint")) //resets the speed back to 5
            {
                CamSpeed = 5;
            }
        }

        if (Input.GetButtonDown("Thirdperson"))
        {
            ThirdPerson = !ThirdPerson;
        }

        if (ThirdPerson && CharacterController != null)
        {
            if (CurrentCharBrain.CameraTrackOverride == null)
            {
                ThirdPersonCamOffset(DefaultCamTrack); //PlayerCam.localPosition = Vector3.Slerp(PlayerCam.localPosition, DefaultCamTrack, 0.1f);
            }
            else
            {
                ThirdPersonCamOffset(CurrentCharBrain.CameraTrackOverride.CamTrack);
            }
        }
        else
        {
            PlayerCam.localPosition = Vector3.Slerp(PlayerCam.localPosition, new Vector3(0,0,0), 0.1f);
        }

        //Swaps player to character in crosshair
        if (Input.GetButtonUp("Quick Swap"))
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 30))
            {
                if (hit.collider.gameObject.layer == 6) //For possessing characters
                {
                    CharacterBrain CharBrain = hit.collider.gameObject.GetComponent<CharacterBrain>();

                    if (CharBrain.CurrentTeam == PlayerTeam) //Ensure player and character are on same team
                    {
                        Debug.Log("Possessing character brain: " + hit.collider.gameObject.name);
                        //removes possession of current character
                        RemovePossession();

                        //finds next character
                        CharacterController = CharBrain.transform;
                        CurrentCharBrain = CharBrain;

                        //possesses new character
                        transform.parent = CharBrain.ViewRoot;
                        Interpolating = true;
                        CharBrain.Possess();
                    }
                }
                if (hit.collider.gameObject.layer == 7 && hit.collider.tag == "Vehicle") //For possessing vehicle drivers
                {
                    //Finds driver of vehicle
                    VehiclePossess Forwarder = hit.collider.gameObject.GetComponent<VehiclePossess>();

                    if (Forwarder.VehicleType == 0)
                    { //Ground vehicles
                        if (Forwarder.Vehicle.Driver != null)
                        {
                            if (Forwarder.Vehicle.Driver.CurrentTeam == PlayerTeam)
                            {
                                //Removes current possession
                                RemovePossession();

                                //finds next character
                                CharacterController = Forwarder.Vehicle.Driver.transform;
                                CurrentCharBrain = Forwarder.Vehicle.Driver;

                                //possesses new character
                                transform.parent = Forwarder.Vehicle.Driver.ViewRoot;
                                Interpolating = true;
                                Forwarder.Vehicle.Driver.Possess();
                            }
                        }
                    }
                    else if (Forwarder.VehicleType == 1)//Air vehicles
                    {
                        if (Forwarder.Aircraft.Driver != null)
                        {
                            if (Forwarder.Aircraft.Driver.CurrentTeam == PlayerTeam)
                            {
                                //Removes current possession
                                RemovePossession();

                                //finds next character
                                CharacterController = Forwarder.Aircraft.Driver.transform;
                                CurrentCharBrain = Forwarder.Aircraft.Driver;

                                //possesses new character
                                transform.parent = Forwarder.Aircraft.Driver.ViewRoot;
                                Interpolating = true;
                                Forwarder.Aircraft.Driver.Possess();
                            }
                        }
                    }
                    else //Mech
                    {
                        //if(Forwarder.Mech)
                    }
                }
            }
        }

        if (Interpolating)
        {
            InterpolateCam();
        }

        //Looking for interactable triggers
        if (CharacterController != transform)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 5))
            {
                if (hit.collider.gameObject.layer == 7) 
                { 
                    InteractPrompt.SetActive(true);
                    Interactable = hit.collider.gameObject;
                }
                else
                { 
                    InteractPrompt.SetActive(false);
                    Interactable = null;
                }
            }
            else
            {
                InteractPrompt.SetActive(false);
                Interactable = null;
            }
            //Interacting
            if (Input.GetButtonDown("Interact") && InteractPrompt.activeSelf)
            {
                Interactable.GetComponent<Interactable>().Interact(CurrentCharBrain);
            }
        }
    }

    private void PlayerRotation()
    {
        float MouseX = Input.GetAxis("Mouse X") * LookSensitivity * Time.deltaTime;
        float MouseY = Input.GetAxis("Mouse Y") * LookSensitivity * Time.deltaTime;

        xRot -= MouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRot, transform.localRotation.eulerAngles.y, 0f);
        CharacterController.Rotate(Vector3.up * MouseX);

        //primarily used by mech char types
        XMouse = MouseX;
        YMouse = MouseY;
    }

    private void InterpolateCam()
    {
        transform.position = Vector3.Slerp(transform.position, CurrentCharBrain.ViewRoot.position, 0.05f);
        transform.rotation = Quaternion.Slerp(transform.rotation, CurrentCharBrain.ViewRoot.rotation, 0.05f);
    }

    public void RemovePossession()
    {
        Interpolating = false;
        if (CharacterController != transform)
        {
            CurrentCharBrain.Unposess();
        }
        transform.parent = null;
        //For fallback/debug instances where the next character is undefined
        CharacterController = transform;
    }

    //For objective script & debugging access primarily, not used often
    public void ForceRepossess(CharacterBrain Char)
    {
        //Removes current character possession
        RemovePossession();

        CharacterController = Char.transform;
        CurrentCharBrain = Char;

        //possesses new character
        transform.parent = Char.ViewRoot;
        Interpolating = true;
        Char.Possess();
    }

    private void ThirdPersonCamOffset(Vector3 CamTrack)
    {
        RaycastHit hit;
        var heading = PlayerCam.parent.position - (PlayerCam.parent.position + CamTrack);

        if (Physics.Raycast(PlayerCam.parent.position, heading / heading.magnitude, out hit, Mathf.Abs(CamTrack.z)))
        {
            PlayerCam.localPosition = Vector3.Slerp(PlayerCam.localPosition, hit.point - PlayerCam.parent.position, 0.1f);
        }
        else
        {
            PlayerCam.localPosition = Vector3.Slerp(PlayerCam.localPosition, CamTrack, 0.1f);
        }
        //Debug.DrawRay(PlayerCam.parent.position, heading / heading.magnitude, Color.blue, 4f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        //Gizmos.DrawSphere(PlayerCam.parent.position - (PlayerCam.parent.position + DefaultCamTrack), 0.15f);
    }
}
