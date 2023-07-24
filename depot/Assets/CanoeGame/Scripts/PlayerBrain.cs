using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerBrain : MonoBehaviour
{
    //Player cam & body rotation
    [SerializeField] private float LookSensitivity = 100f, CamSpeed = 5f;
    private Transform CharacterController;
    [HideInInspector] public CharacterBrain CurrentCharBrain;
    [HideInInspector]public float xRot = 0f, XMouse, YMouse;
    private GameObject Interactable;
    private ContextualInteract InteractPrompt;
    public RawImage FadeInOut;

    //Camera interpolation between characters
    [HideInInspector]public bool Interpolating;

    //Team to filter quick swapping to
    public int PlayerTeam;

    //Default thirdperson camera offset
    public Vector3 DefaultCamTrack;
    [HideInInspector] public bool ThirdPerson, Fading, Faded;
    private Transform PlayerCam;

    public CharacterBrain PlayerCharacter;

    WaterSurface Water;
    WaterSearchParameters WaterSearch;
    WaterSearchResult WaterSearchResult;

    AudioMixerControl MixerControl;

    public int PlayerCash = 15;

    [HideInInspector] public AudioSource UIOneShotSrc;

    [SerializeField] Transform InventoryUI;
    [HideInInspector] public bool IsInInventory;

    //name is decieving, higher value = more food in stomach, if less than zero start slowly taking damage.
    public float PlayerHunger = 100;
    [SerializeField] Volume HungerOverlay; //overlaying hunger
    [SerializeField] Image HungerBar, HealthBar;

    //temp cinematic intro
    [SerializeField] VideoClip IntroCinematic;
    public CinematicVideoManager CinematicsManager;
    float CinematicDelay = -1;
    Animator FadingAnim;
    bool AwaitCinematic;
    [SerializeField] AnimatedCharacter IntroHammockExit;
    [SerializeField] public IntroSeqManager IntroSequencer;
    float AwaitHammockExit = -100;

    [SerializeField][Header("Debug")] bool SkipIntro, SkipBufferProtection;
    [HideInInspector] public bool DevCamLocked;

    void Start()
    {
        FadingAnim = FadeInOut.gameObject.GetComponent<Animator>();
        if (!PlayerPrefs.HasKey("AiKey")) {
            PlayerPrefs.SetString("AiKey", "");
        }
        if (!PlayerPrefs.HasKey("Sensitivity"))
        {
            PlayerPrefs.SetFloat("Sensitivity", 100);
        }
        LookSensitivity = PlayerPrefs.GetFloat("Sensitivity");
        UIOneShotSrc = GetComponent<AudioSource>();

        Water = (WaterSurface)GameObject.FindObjectOfType(typeof(WaterSurface));
        CharacterController = this.transform;
        Cursor.lockState = CursorLockMode.Locked;
        InteractPrompt = GameObject.Find("InteractPrompt").GetComponent<ContextualInteract>();
        InteractPrompt.gameObject.SetActive(false);
        PlayerCam = GameObject.Find("PlayerCam").transform;

        //removes possession of current character
        RemovePossession();

        //finds next character
        CharacterController = PlayerCharacter.transform;
        CurrentCharBrain = PlayerCharacter;

        //possesses new character
        transform.parent = PlayerCharacter.ViewRoot;
        Interpolating = true;
        transform.position = PlayerCharacter.ViewRoot.position;
        PlayerCharacter.Possess();

        MixerControl = GetComponent<AudioMixerControl>();

        if (!PlayerPrefs.HasKey("FinishedIntro" + PlayerPrefs.GetInt("CurrentSave")) && !SkipIntro) //if false, play through intro sequence
        {
            IntroSequencer.gameObject.SetActive(true);
            IntroHammockExit.gameObject.SetActive(true);
            CurrentCharBrain.Animated = true;
            CinematicDelay = 30;
        }
    }

    void Update()
    {
        //Game intro
        if (CinematicDelay > 0 && CinematicDelay != -1)
        {
            CinematicDelay -= Time.deltaTime;
            PlayerCallback.LoadManager.gameObject.SetActive(true);
            PlayerCallback.LoadManager.SetLoadDescriptor("New save file. Please wait...");
            PlayerCallback.LoadManager.SetProgress((30 - CinematicDelay) / 30);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PlayerCallback.AudioMix.SetVolume(9, 0.00001f);
            if (SkipBufferProtection)
            {
                CinematicDelay = 0;
            }
        }
        else if (CinematicDelay != -1) 
        {
            CinematicDelay = -1;
            PlayerCallback.LoadManager.gameObject.SetActive(false);
            CinematicsManager.PlayCinematic(IntroCinematic);
            AwaitCinematic = true;
            CinematicsManager.Playing = true;
        }
        if (AwaitCinematic && !CinematicsManager.Playing) {
            IntroSequencer.Sequence = 0;
            IntroSequencer.CheckSequence();
            Debug.Log("Cinematic over!!");
            AwaitCinematic = false;
            /*CurrentCharBrain.Override = IntroHammockExit;
            CurrentCharBrain.InteractionPos = CurrentCharBrain.transform;
            ThirdPerson = true;
            CurrentCharBrain.AnimatedInteract();*/
            IntroHammockExit.PlayScene();
            ThirdPerson = false;
            AwaitHammockExit = 9;
            //CurrentCharBrain.CharacterAnimator.runtimeAnimatorController = IntroHammockExit;
        }
        if (AwaitHammockExit != -100)
        {
            if (AwaitHammockExit > 0) {
                AwaitHammockExit -= Time.deltaTime;
            }
            else
            {
                AwaitHammockExit = -100;
                IntroSequencer.Sequence = 1;
                IntroSequencer.CheckSequence();
            }
        }

        //Rotates the player & camera
        PlayerRotation();

        //Fade in and out of black
        FadingAnim.SetBool("Fading", Fading);
        /*if (Fading)
        {
            if (!Faded)
            {
                FadeInOut.color = new Color(0, 0, 0, FadeInOut.color.a + Time.unscaledDeltaTime / 2);
                Faded = (FadeInOut.color.a >= 1);
            }
        }
        else
        {
            if (Faded)
            {
                FadeInOut.color = new Color(0, 0, 0, FadeInOut.color.a - Time.unscaledDeltaTime / 2);
                Faded = (FadeInOut.color.a <= 0);
            }
        }*/

        //hunger/starvation
        if (!CurrentCharBrain.Animated) {
            Starve();
        }

        HealthBar.fillAmount = CurrentCharBrain.MainHP / 100;

        //Debug/fallback camera movement
        if (CharacterController == transform && !DevCamLocked)
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

        //Finds water surface position
        WaterSearch.startPosition = transform.position;
        Water.FindWaterSurfaceHeight(WaterSearch, out WaterSearchResult);

        //Drowning
        if (CharacterController != transform)
        {
            if (WaterSearchResult.height > transform.position.y)
            {
                if (PlayerCharacter.Breath <= 0)
                {
                    PlayerCharacter.Hurt(Time.deltaTime * 20);
                }
                else
                {
                    PlayerCharacter.Breath -= Time.deltaTime;
                }

                MixerControl.SetVolume(7, 0.20f); //Underwater Audio Muffling
            }
            else //regaining air
            {
                if (PlayerCharacter.Breath < 3)
                {
                    PlayerCharacter.Breath += Time.deltaTime;
                }

                MixerControl.SetVolume(7, 1);
            }
        }

        if (Input.GetButtonDown("Inventory") && !CurrentCharBrain.Animated)
        {
            IsInInventory = !IsInInventory;
            OpenInventory();
        }


        /*if (Input.GetButtonDown("Thirdperson"))
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
            PlayerCam.localPosition = Vector3.Slerp(PlayerCam.localPosition, new Vector3(0, 0, 0), 0.1f);
        }*/

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
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 3))
            {
                if (hit.collider.gameObject.layer == 7 && ((hit.collider.tag == "Vehicle" || hit.collider.tag == "Item_Pickup" || hit.collider.tag == "Item_WaveHack" || hit.collider.tag == "Item_Battery" || hit.collider.gameObject.tag == "Untagged") || hit.collider.gameObject.tag == "Shop" || hit.collider.gameObject.tag == "Container" || hit.collider.gameObject.tag == "Npc" || hit.collider.gameObject.tag == "Bed" || hit.collider.gameObject.tag == "Chair" || hit.collider.gameObject.tag == PlayerCharacter.Weapons[PlayerCharacter.CurrentWeapon].TagName)) 
                { 
                    InteractPrompt.gameObject.SetActive(true);
                    Interactable = hit.collider.gameObject;

                    //Sets contextual interact promp text
                    if (hit.collider.tag == "Vehicle")
                    {
                        InteractPrompt.UpdateInteractText("Enter");
                    }
                    else if (hit.collider.tag == "Item_Pickup")
                    {
                        InteractPrompt.UpdateInteractText("Grab");
                    }
                    else if (hit.collider.tag == "Untagged")
                    {
                        InteractPrompt.UpdateInteractText("Interact");
                    }
                    else if (hit.collider.gameObject.tag == "Shop")
                    {
                        InteractPrompt.UpdateInteractText("Shop");
                    }
                    else if (hit.collider.gameObject.tag == "Container")
                    {
                        InteractPrompt.UpdateInteractText("Open");
                    }
                    else if (hit.collider.gameObject.tag == "Npc")
                    {
                        InteractPrompt.UpdateInteractText("Talk");
                    }
                    else if (hit.collider.gameObject.tag == "Bed")
                    {
                        InteractPrompt.UpdateInteractText("Sleep");
                    }
                    else if (hit.collider.gameObject.tag == "Chair")
                    {
                        InteractPrompt.UpdateInteractText("Sit");
                    }
                    else if(hit.collider.gameObject.tag == "Item_Battery")
                    {
                        InteractPrompt.UpdateInteractText("Insert Battery");
                    }
                    else if (hit.collider.gameObject.tag == "Item_WaveHack")
                    {
                        InteractPrompt.UpdateInteractText("Hack Panel");
                    }
                    else
                    {
                        InteractPrompt.UpdateInteractText("Use");
                    }
                }
                else
                { 
                    InteractPrompt.gameObject.SetActive(false);
                    Interactable = null;
                }
            }
            else
            {
                InteractPrompt.gameObject.SetActive(false);
                Interactable = null;
            }
            //Interacting
            if (Input.GetButtonDown("Interact") && InteractPrompt.gameObject.activeSelf)
            {
                Interactable.GetComponent<Interactable>().Interact(CurrentCharBrain);
                
                if (Interactable.tag != "Untagged" && Interactable.tag != "Vehicle" && Interactable.tag != "Item_Pickup" && hit.collider.gameObject.tag != "Shop" && hit.collider.gameObject.tag != "Container" && hit.collider.gameObject.tag != "Npc" && hit.collider.gameObject.tag != "Bed" && hit.collider.gameObject.tag != "Chair" && hit.collider.gameObject.tag != "Item_Battery" && hit.collider.gameObject.tag != "Item_WaveHack") //takes "ammo" from item
                {
                    PlayerCharacter.UseItem();
                }
            }
        }
    }

    private void Starve()
    {
        if (PlayerHunger > 0) { //hunger
            PlayerHunger -= (Time.deltaTime / 15);
        }
        else { //starvation
            CurrentCharBrain.Hurt(Time.deltaTime);
        }
        HungerBar.fillAmount = PlayerHunger / 100;
        HungerOverlay.weight = MathF.Abs((PlayerHunger - 100) / 100);
    }

    public void OpenInventory()
    {
        if (IsInInventory)
        {
            InventoryUI.localScale = new Vector3(1, 1, 1);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            InventoryUI.localScale = new Vector3(0, 0, 0);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        PlayerCallback.Inventory.UpdateInventory();
    }

    private void PlayerRotation()
    {
        if (CurrentCharBrain == null || (CurrentCharBrain != null ? !CurrentCharBrain.Animated : false))
        {
            float MouseX = Input.GetAxis("Mouse X") * LookSensitivity * Time.unscaledDeltaTime;
            float MouseY = Input.GetAxis("Mouse Y") * LookSensitivity * Time.unscaledDeltaTime;

            xRot -= MouseY * PlayerPrefs.GetInt("InvertLook");
            xRot = Mathf.Clamp(xRot, -80f, 80f);

            //Debug.Log("Rotating, Dead: " + CurrentCharBrain.Dead + " Seated: " + CurrentCharBrain.Seated + " Animated: " + CurrentCharBrain.Animated);

            if (CurrentCharBrain == null) //character rotation
            {
                transform.localRotation = Quaternion.Euler(xRot, transform.localRotation.eulerAngles.y, 0f);
                CharacterController.Rotate(Vector3.up * MouseX);
            }
            if (!CurrentCharBrain.Dead && !CurrentCharBrain.Animated && !CurrentCharBrain.Seated) //restricted rotation
            {
                transform.localRotation = Quaternion.Euler(xRot, transform.localRotation.eulerAngles.y, 0f);
                CharacterController.Rotate(Vector3.up * MouseX);
            }
            else if (!CurrentCharBrain.Dead && !CurrentCharBrain.Animated && CurrentCharBrain.Seated) //seated in boat
            {
                transform.localRotation = Quaternion.Euler(xRot, transform.localRotation.eulerAngles.y, 0f);
                transform.Rotate(Vector3.up * MouseX);
            }

            //primarily used by mech char types
            XMouse = MouseX;
            YMouse = MouseY;
        }
    }

    private void InterpolateCam()
    {
        transform.position = Vector3.Slerp(transform.position, CurrentCharBrain.ViewRoot.position, 0.05f);
        if (!CurrentCharBrain.Seated)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, CurrentCharBrain.ViewRoot.rotation, 0.05f);
        }
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
