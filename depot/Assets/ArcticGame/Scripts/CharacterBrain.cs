using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.IO;
using TMPro;
using UnityEditor.SearchService;
using UnityEditorInternal;
using UnityEngine.Animations;
using System;
using UnityEngine.Rendering;
using System.Threading;
using UnityEngine.Events;

public class CharacterBrain : MonoBehaviour
{
    [Header("Default Stats")]
    //Character Class
    public CharacterClass CurrentClass;
    public int CurrentTeam;

    //Default Weapons
    [SerializeField] private Weapon w_Primary;
    [SerializeField] private Weapon w_Secondary;
    [HideInInspector] public Weapon w_Ability; //Cannot be swapped to outside of using your ability
    //private Weapon w_Equipped; //not to be set by class, actively updates to currently held weapon for simpler coding

    [HideInInspector] public Weapon[] Weapons = { null, null, null }; //All weapons player has, organized in an array

    //Character States
    [HideInInspector] public bool Possessed, Animated, Downed, Seated, Grounded, Crouched, AiAwait;

    //Gameobject Components
    [HideInInspector]public Rigidbody ThisRb;
    AIPath ThisAi;
    private Seeker ThisSeek;

    [Header("Essential Components")]
    //For orienting chest to fp position
    [SerializeField] private ParentConstraint ChestConstraint;

    //Where the player will view from the character's perspective
    public Transform ViewRoot, WeaponHandle; //WeaponHandle is the root position for weapons to be held

    //Set this value to the commander character if designated as a squadmate
    [HideInInspector] public Transform FollowTarget;

    //MoveTarget is temporary for debugging
    //[SerializeField] private Transform MoveTarget;

    //Character Stats (will be overriden at start with class values if defined)
    float MoveSpeed = 3f, SprintMult = 0.6f, JumpHeight = 3f, LeanAngle = 30f;

    //For vehicles to access
    [HideInInspector] public CapsuleCollider CharacterCollission;
    [HideInInspector] public CameraTrack CameraTrackOverride;

    //For animating the character
    public Animator CharacterAnimator;

    //For locking character visual rotation to a non-local value, especially useful for vehicles
    [HideInInspector] public bool LockVisualRot;
    [HideInInspector] public Quaternion LockRot;

    //Reference to lean visuals
    Transform LeanVisual;

    //Currently equipped weapon
    [HideInInspector] public int CurrentWeapon;
    //0 = primary
    //1 = secondary
    //2 = ability
    GameObject HeldWeapon; //CurrentWeaponModel
    Transform wm_Muzzle; //WeaponMarker for the muzzle
    Animator wm_Visuals; //Weapon model animator for synced reloads and such
    Animator DefaultAnimator; //Original animatorController used by character

    //clambering related variables
    [HideInInspector] public bool clambering;
    Vector3 clamberpos;

    //For weapon aiming, this object also externally senses other characters for the ai
    //Transform ViewCone;

    //Limb health and states:
    //"i" variables represent internal values, while the others are the max possible stats
    float[] HP = { 5, 35, 15, 15, 20, 20 }; //health
    float[] iHP = { 0, 0, 0, 0, 0, 0 };
    float[] AP = { 5, 15, 10, 10, 10, 10 }; //armor
    float[] iAP = { 0, 0, 0, 0, 0, 0 };
    int[] LimbStatus = { 0, 0, 0, 0, 0, 0 }; //for damage status effects
    //Head - 0
    //Body - 1
    //Left Arm - 2
    //Right Arm - 3
    //Left Leg - 4
    //Right Leg - 5
    float DamageTimer; //time since last taken damage, for recharging AP;
    bool awaitingRecharge; //spaghetti

    //Eventually find a way to remove this and do it fully internal
    [SerializeField] private LayerMask BulletMask;

    //mech turret gimbaling
    private bool GimbalTop;
    //ignores vehicles and prevents entering
    [HideInInspector] public bool IgnoreVehicles;

    //Ai update function:
    [SerializeField] private UnityEvent AiUpdate;

    //Universal position for weapon firing for both Player & AI
    private Transform FirePos;

    private void Start()
    {
        //initial value
        FirePos = ViewRoot;

        //Setup weapons array
        Weapons[0] = w_Primary;
        Weapons[1] = w_Secondary;
        Weapons[2] = w_Ability;

        ThisRb = gameObject.GetComponent<Rigidbody>();
        ThisAi = gameObject.GetComponent<AIPath>();
        ThisSeek = gameObject.GetComponent<Seeker>();
        CharacterCollission= gameObject.GetComponent<CapsuleCollider>();
        LeanVisual = ViewRoot.parent.parent;
        ViewRoot.parent.parent = transform;
        DefaultAnimator = CharacterAnimator;

        //ViewCone = ViewRoot.Find("AiViewCone");

        //FP arms constraint
        ConstraintSource Src = new ConstraintSource();
        //Src.sourceTransform = CamRotTrack;
        Src.sourceTransform = Camera.main.transform;
        Src.weight = 1;
        ChestConstraint.AddSource(Src);
        ChestConstraint.SetTranslationOffset(0, new Vector3(-0.03f, -0.3f, -0.1f));
        ChestConstraint.SetRotationOffset(0, new Vector3(0, 45, 0)); //traditionally 45 but then set to 55, but now 45 again????

        //Setting internal AP & HP values
        for (int i = 0; i <= 5; i++)
        {
            iHP[i] = HP[i];
            iAP[i] = AP[i];
        }

        if (CurrentClass != null)
        {
            //Sets character stats to those defined in CurrentClass
            ReadClassValues();
        }

        UpdateWeapon();

        //sets internal ammo variables to expected values
        foreach (Weapon ThisWeapon in Weapons)
        {
            ThisWeapon.a_CurrentClip = ThisWeapon.AmmoClip;
            ThisWeapon.a_Extra = ThisWeapon.AmmoSpare;
        }

        //Ai event startup stuff
        if (AiUpdate == null)
            AiUpdate = new UnityEvent();
    }

    private void Update() //Per frame
    {
        if (!Animated && !Seated && !Downed)
        {
            if (Possessed) //If player is controlling character
            {
                PlayerCharacter();
            }
            else //If ai is controlling character
            {
                AiCharacter();
            }
        }
        else if(Animated || Seated)
        {
            ThisRb.isKinematic = true;

            //Snaps rotation to non-local value
            if (LockVisualRot)
            {
                CharacterAnimator.transform.rotation = LockRot;
            }
        }

        if (!Animated) //changes the animator parameters
        {
            UpdateAnimator();
        }

        if (Possessed) //not to be done in PlayerCharacter() so it still works while in vehicles
        {
            //Re-orient chest to be accurate to screen position if in first person
            ChestConstraint.constraintActive = !PlayerCallback.PlayerBrain.ThirdPerson;
            //fp leg sliding to prevent weird ugly messes when you look down between 55 - 76 degrees (x)
            CharacterAnimator.transform.localPosition = new Vector3(0, 0, -0.28f * Convert.ToInt32(!PlayerCallback.PlayerBrain.ThirdPerson) * Convert.ToInt32(!Seated) * (Mathf.Clamp(PlayerCallback.PlayerBrain.xRot, 55, 76) / 76));
        }

        UpdateHealth();

        if (Weapons[CurrentWeapon].i_FireRate > 0) //updates internal fire rate counter for current weapon
        {
            Weapons[CurrentWeapon].i_FireRate -= Time.deltaTime;
        }
    }

    private void AiCharacter()
    {
        //CheckGrounded();
        //Grounded = GroundCheckNew();
        UpdateAnimator();
        //ThisSeek.StartPath(transform.position, MoveTarget.position);

        AiUpdate.Invoke();
    }

    private void PlayerCharacter()
    {
        //check if player is touching ground
        //CheckGrounded();
        //Grounded = GroundCheckNew();

        //ViewCone.rotation = PlayerCallback.PlayerBrain.transform.rotation;

        //Player Movement
        if (Grounded)
        {
            float Speed = MoveSpeed * (1 + Input.GetAxis("Sprint") * SprintMult);

            Vector3 Movement = transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal") * Speed, 0f, Input.GetAxis("Vertical") * Speed)); //downhill fix

            //ThisRb.velocity = transform.right * (Input.GetAxis("Horizontal") * Speed) + transform.forward * (Input.GetAxis("Vertical") * Speed);
            ThisRb.velocity = new Vector3(Movement.x, ThisRb.velocity.y, Movement.z);
        }

        //leaning camera functionality
        Quaternion Lean = Quaternion.Euler(Mathf.Clamp(PlayerCallback.PlayerBrain.xRot / 2, -20, 28), 0, (LeanAngle * Input.GetAxis("Leaning") * (-1 + Input.GetAxis("Sprint"))));
        ViewRoot.parent.localRotation = Quaternion.Slerp(ViewRoot.parent.localRotation, Lean, 0.05f);

        //Leaning & aim orientation visual offset
        Quaternion LeanVisualRot = Quaternion.Euler(-Lean.eulerAngles.z + (PlayerCallback.PlayerBrain.xRot / 2), 65, Lean.eulerAngles.z + (PlayerCallback.PlayerBrain.xRot / 2));
        LeanVisual.localRotation = Quaternion.Slerp(LeanVisual.localRotation, LeanVisualRot, 0.05f);

        //Jumping & clambering
        if (Input.GetButtonDown("Jump"))
        {
            if (Grounded) //Jump
            {
                Grounded = false;
                ThisRb.AddForce(new Vector3(0, JumpHeight * 27, 0), ForceMode.Impulse);
            }
            else //Clamber
            {
                AttemptClamber();
            }
        }

        //Fire held weapon
        if (Input.GetButtonDown("Fire"))
        {
            FireWeapon();
        }

        //Reload held weapon
        if (Input.GetButtonDown("Reload"))
        {
            Reload();
        }

        if (clambering)
        {
            transform.position = Vector3.Slerp(transform.position, clamberpos, 0.04f);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0 && CharacterAnimator.GetBool("SwapWeapon") != true)
        {
            SwapWeapon();
        }
    }

    public void Possess()
    {
        Possessed= true;
        ThisAi.canMove = false;
        ThisRb.isKinematic = false;

        FirePos = ViewRoot.Find("PlayerBrain");
    }

    public void Unposess() //TODO: Currently player velocity isn't transferred into ai velocity when swapping out
    {
        Possessed = false;
        if (!Seated)
        {
            ThisAi.canMove = true;
        }

        ThisRb.isKinematic = true; //keeping it off adds ai pushing but causes desync between visuals and collider
        ChestConstraint.constraintActive = false;
        CharacterAnimator.transform.localPosition = new Vector3(0, 0, 0);

        FirePos = ViewRoot;
        //ViewCone.transform.localEulerAngles = new Vector3(-90, 0, 0);
    }

    private bool GroundCheckNew() //Doesn't work
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z), transform.TransformDirection(Vector3.down), out hit, -0.28f))
        {
            if (hit.collider.gameObject.layer == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void CheckGrounded() //Works but is raycast, which causes problems, swapping to this will also require replacing all references to GroundCheckNew() to Grounded
    {
        RaycastHit hit;
        //new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z)
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z), transform.TransformDirection(Vector3.down), out hit, 0.2f))
        {
            //Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject.layer == 0)
            {
                Grounded = true;
            }
            else
            {
                Grounded = false;
            }
        }
        else
        {
            Grounded = false;
        }
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 0.15f, transform.position.z), transform.TransformDirection(Vector3.down), Color.red);
    }

    public void ReadClassValues()
    {
        //Could likely be done more effeciently
        MoveSpeed = CurrentClass.MoveSpeed;
        SprintMult = CurrentClass.SprintMult;
        ThisAi.maxSpeed = MoveSpeed * ((1 + SprintMult));
        JumpHeight = CurrentClass.JumpHeight;
        LeanAngle = CurrentClass.LeanAngle;
        Weapons[0] = CurrentClass.Primaries[UnityEngine.Random.Range(0, CurrentClass.Primaries.Length)]; //primary
        Weapons[1] = CurrentClass.Secondaries[UnityEngine.Random.Range(0, CurrentClass.Secondaries.Length)]; //secondary
        GimbalTop = CurrentClass.TopGimbal;
        IgnoreVehicles = CurrentClass.IgnoreVehicles;

        //Armor points
        AP[0] = CurrentClass.HeadAP;
        AP[1]= CurrentClass.BodyAP;
        AP[2]= CurrentClass.LArmAP;
        AP[3]= CurrentClass.RArmAP;
        AP[4]= CurrentClass.LLegAP;
        AP[5]= CurrentClass.RLegAP;
    }

    public void AiInvestigate(Vector3 LastKnownPos)
    {
        //Investigate the last known position of enemy if they are killed/lost track of
        if (!Possessed)
        {
            ThisSeek.StartPath(transform.position, LastKnownPos);
        }
    }

    //Climbing ledges
    private void AttemptClamber()
    {
        //Detecting ledge
        RaycastHit midHit;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), transform.TransformDirection(Vector3.forward), out midHit, 1f))
        {
            if (midHit.collider.gameObject.layer == 0)
            {
                RaycastHit HighHit;
                if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), transform.TransformDirection(Vector3.forward), out HighHit, 0.25f))
                {}
                else
                {
                    RaycastHit Headhit;
                    if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), transform.TransformDirection(Vector3.up), out Headhit, 1f))
                    {}
                    else
                    {
                        //Initiate clamber
                        RaycastHit Ledgehit;
                        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z) + transform.TransformDirection(Vector3.forward), transform.TransformDirection(Vector3.down), out Ledgehit, 2f))
                        {
                            if (Ledgehit.collider.gameObject.layer == 0 && Ledgehit.normal.y >= 0.8f)
                            {
                                //Debug.Log("Valid ledge!");
                                //transform.position = Ledgehit.point;

                                //Plays animation for climbing ledge
                                ThisRb.isKinematic = true;

                                CharacterAnimator.SetTrigger("Clamber");
                                clambering = true;
                                clamberpos = Ledgehit.point;
                            }
                        }
                    }
                }
            }
        }
    }

    //Used by vehicles to correct the mesh rotation offset when exiting
    public void RecenterVisuals()
    {
        CharacterAnimator.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    //For adjusting the character animator's parameter values
    private void UpdateAnimator()
    {
        CharacterAnimator.SetFloat("x", transform.InverseTransformDirection(ThisRb.velocity).x / 3);
        CharacterAnimator.SetFloat("z", transform.InverseTransformDirection(ThisRb.velocity).z / 3);
        CharacterAnimator.SetFloat("Speed", Mathf.Clamp(ThisRb.velocity.magnitude / 2.5f, 0.75f, 999));
        CharacterAnimator.SetBool("Seated", Seated);
        CharacterAnimator.SetBool("Falling", !Grounded);
        CharacterAnimator.SetBool("Controlled", Possessed); //useful for mechs
        
        CharacterAnimator.SetFloat("Sprint", Mathf.Lerp(CharacterAnimator.GetFloat("Sprint"), Input.GetAxis("Sprint") * System.Convert.ToInt32(Grounded) * System.Convert.ToInt32(Possessed) * ThisRb.velocity.magnitude / 3f, 0.1f));
        //CharacterAnimator.SetFloat("Sprint", (Input.GetAxis("Sprint") * System.Convert.ToInt32(Grounded) * System.Convert.ToInt32(Possessed)) * ThisRb.velocity.magnitude / 3f);

        CharacterAnimator.SetBool("Aiming", Input.GetButton("Aim") && Possessed);
        wm_Visuals.SetBool("Aiming", CharacterAnimator.GetBool("Aiming"));
        //wm_Visuals.SetFloat("Sprint", CharacterAnimator.GetFloat("Sprint"));

        //Mech turret gimbal
        if (GimbalTop)
        {
            if ((Mathf.Abs(CharacterAnimator.GetFloat("x")) + Mathf.Abs(CharacterAnimator.GetFloat("z"))) == 0 && Possessed)
            {
                CharacterAnimator.transform.Rotate(Vector3.up * -PlayerCallback.PlayerBrain.XMouse);
                //CharacterAnimator.transform.localRotation = Quaternion.Euler(CharacterAnimator.transform.localRotation.x, CharacterAnimator.transform.localRotation.x - PlayerCallback.PlayerBrain.XMouse, CharacterAnimator.transform.localRotation.z);
            }
            else
            {
                CharacterAnimator.transform.localRotation = Quaternion.Euler(-180, 180, 180); //specific to mech proto
            }
        }

        //Firstperson weapon sway
        /*if (Possessed && !PlayerCallback.PlayerBrain.ThirdPerson)
        {
            float MouseX = Input.GetAxis("Mouse X");
            float MouseY = Input.GetAxis("Mouse Y");

            //Get target rotation
            Quaternion RotationX = Quaternion.AngleAxis(-MouseY, Vector3.right);
            Quaternion RotationY = Quaternion.AngleAxis(MouseX, Vector3.up);

            Quaternion targetRot = RotationX * RotationY;

            //ChestConstraint.SetTranslationOffset(0, Weapons[CurrentWeapon].FirstPersonOffset);
            ChestConstraint.SetRotationOffset(0, Quaternion.Slerp(Quaternion.Euler(0,45,0), targetRot, Time.deltaTime).eulerAngles);// targetRot.eulerAngles + new Vector3(0, 45, 0));
        }*/
    }

    //For swapping weapons, as to not run in hot path
    public void UpdateWeapon()
    {
        Weapons[2] = w_Ability; //So that it can be modified dynamically

        if (HeldWeapon != null)
        {
            Destroy(HeldWeapon); //removes any previous weapon models
        }

        Weapon WeaponToHold = null;
        WeaponToHold = Weapons[CurrentWeapon];

        HeldWeapon = Instantiate(WeaponToHold.Model, WeaponHandle); //Creates new weapon model
        CharacterAnimator.runtimeAnimatorController = WeaponToHold.WeaponAnimator; //Swaps character animator with weapon animator
        
        //Find new weapon model animator
        wm_Visuals = HeldWeapon.transform.Find("-Visuals-").GetComponent<Animator>();

        //Find new weapon model markers
        wm_Muzzle = HeldWeapon.transform.Find("-Muzzle-"); //Muzzle position

        //Updates first person offset to be relative to weapon
        ChestConstraint.SetTranslationOffset(0, WeaponToHold.FirstPersonOffset);
    }

    private void SwapWeapon()
    {
        if (CurrentWeapon == 0)
        {
            CurrentWeapon = 1; //secondary
        }
        else
        {
            CurrentWeapon = 0; //primary
        }
        CharacterAnimator.SetBool("SwapWeapon", true);
        wm_Visuals.SetBool("SwapWeapon", true);
        //UpdateWeapon();
    }

    private void FireWeapon()
    {
        //Checks if mag has ammo and fire rate is within range to fire
        if (Weapons[CurrentWeapon].i_FireRate <= 0 && Weapons[CurrentWeapon].a_CurrentClip > 0)
        {
            CharacterAnimator.SetTrigger("FireWeapon"); //animates the character accordingly
            wm_Visuals.SetTrigger("FireWeapon");
            RaycastHit hit;
            if (Physics.Raycast(FirePos.position, FirePos.TransformDirection(Vector3.forward), out hit, Weapons[CurrentWeapon].BulletDistance, BulletMask))
            {
                if (hit.collider.gameObject.layer == 8) //checks if hit surface is a hitbox
                {
                    hit.collider.gameObject.GetComponent<Hitbox>().Hit(Weapons[CurrentWeapon].Damage); //damages hitbox
                }
            }
            Debug.DrawRay(FirePos.position, FirePos.TransformDirection(Vector3.forward), Color.red, Weapons[CurrentWeapon].BulletDistance);
            Weapons[CurrentWeapon].i_FireRate = Weapons[CurrentWeapon].FireRate; //resets firerate counter
            Weapons[CurrentWeapon].a_CurrentClip--; //removes ammo from mag
        }
    }

    //For taking damage, refers to specific limbs
    public void Hurt(int limb, float damage)
    {
        Debug.Log("Ouch! (" + damage + ") damage, " + iHP[limb] + " limb HP");
        if (iAP[limb] <= 0)
        { //hp loss
            iHP[limb] -= damage;

            if (iHP[limb] <= 0) //check if dead/downed
            {
                if (limb > 1)
                {
                    NeedRevive();
                }
                else //if chest or head hp <= 0
                {
                    Die();
                }
            }
            else if (iHP[limb] / HP[limb] <= 0.5f) //check if status effects apply
            {
                LimbStatus[limb] = UnityEngine.Random.Range(0, 4);
            }
        }
        else
        { //ap loss
            iAP[limb] -= damage;
            if (iAP[limb] < 0) //overflow armor damage into health
            {
                iHP[limb] += iAP[limb]; //+ since it'll be negative
            }
        }
        DamageTimer = 5;
    }

    public void Die()
    {
        if (Possessed)
        {
            PlayerCallback.PlayerBrain.RemovePossession();
        }
        Destroy(this.gameObject); //Placeholder until more finalized death state can be made
    }

    //For when the character is immobilized, but still alive, and can be healed back into battle
    public void NeedRevive()
    {
        //TODO
        Debug.Log("I'M DOWNED!!!");
        Downed = true;
        //temp to better visualize:
        Seated = true;
    }

    private void UpdateHealth()
    {
        if (DamageTimer > 0) //recharges ap if you have been out of combat for over 5 seconds
        {
            DamageTimer -= Time.deltaTime;
            awaitingRecharge = true;
        }
        else if (awaitingRecharge == true)
        {
            awaitingRecharge= false;
            for (int i = 0; i <= 5; i++)
            {
                iAP[i] = AP[i]; //instant ap recharge
            }
        }
    }

    private void Reload()
    {
        //Major logic and timing are handled by animation which calls upon various other voids
        if (Weapons[CurrentWeapon].a_CurrentClip > 0)
        {
            CharacterAnimator.SetTrigger("ReloadShort");
            wm_Visuals.SetTrigger("ReloadShort");
        }
        else
        {
            CharacterAnimator.SetTrigger("Reload");
            wm_Visuals.SetTrigger("Reload");
        }
    }

    //When reloading and pulling out magazine
    public void MagOut()
    {
        Weapons[CurrentWeapon].a_Extra += Weapons[CurrentWeapon].a_CurrentClip;
        Weapons[CurrentWeapon].a_CurrentClip = 0;
    }
    //When reloading and putting new mag in
    public void MagIn()
    {
        if (Weapons[CurrentWeapon].a_Extra >= Weapons[CurrentWeapon].AmmoClip) //For if you have plenty of spare ammo
        {
            Weapons[CurrentWeapon].a_Extra -= Weapons[CurrentWeapon].AmmoClip;
            Weapons[CurrentWeapon].a_CurrentClip = Weapons[CurrentWeapon].AmmoClip;
        }
        else //If you don't have enough for a full new mag
        {
            Weapons[CurrentWeapon].a_CurrentClip = Weapons[CurrentWeapon].a_Extra;
            Weapons[CurrentWeapon].a_Extra = 0;
        }
    }

    public void EndOverride()
    {
        CharacterAnimator.runtimeAnimatorController = Weapons[CurrentWeapon].WeaponAnimator;
        Animated = false;
    }

    private void OnDrawGizmos()
    {
        if (CurrentTeam == 0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
        else if(CurrentTeam == 1)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
        else
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}