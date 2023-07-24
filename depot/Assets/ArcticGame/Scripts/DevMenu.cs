using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SearchService;
using UnityEditor;
using UnityEngine.UI;

public class DevMenu : MonoBehaviour
{
    private bool Active;
    private PlayerBrain Player;
    [SerializeField] private GameObject DevCharacter;
    [SerializeField] private TextMeshProUGUI DevHeader;
    [SerializeField] private TMP_InputField TeamNum, SceneNum, TimescaleNum;
    private bool Moving;
    private Transform MoveObj;

    private Sandbox Sandboxref;
    
    private void Start()
    {
        Player = GameObject.Find("PlayerBrain").GetComponent<PlayerBrain>();
        TeamNum.text = "" + Player.PlayerTeam;
        SceneNum.text = "" + SceneManager.GetActiveScene().buildIndex;
        Sandboxref = GameObject.FindObjectOfType<Sandbox>();
    }

    private void Update()
    {
        if ((Application.isEditor || Debug.isDebugBuild) && Input.GetKeyDown(KeyCode.BackQuote))
        {
            Active = !Active;
            transform.localScale = new Vector3(1 * Convert.ToInt32(Active), 1 * Convert.ToInt32(Active), 1 * Convert.ToInt32(Active));

            if (Active)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else if((Application.isEditor || Debug.isDebugBuild) == false)
        {
            Destroy(this.gameObject);
        }

        if (Active)
        {
            DevHeader.text = Mathf.RoundToInt(1.0f / Time.deltaTime) + " fps - Scene: " + SceneManager.GetActiveScene().name;
        }

        if (Moving)
        {
            RaycastHit hit;
            if (Physics.Raycast(Player.transform.position, Player.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.transform != MoveObj)
                {
                    MoveObj.position = hit.point + new Vector3(0, 2.5f, 0);
                }
            }
        }
    }

    public void DetachPlayer()
    {
        Player.RemovePossession();
        
    }

    public void MakeCharacter()
    {
        CharacterBrain Char = GameObject.Instantiate(DevCharacter).GetComponent<CharacterBrain>();
        Char.transform.position = new Vector3(Player.transform.position.x, Player.transform.position.y - 1, Player.transform.position.z);
        Char.CurrentTeam = Player.PlayerTeam;
        //Player.ForceRepossess(Char);
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetTeam()
    {
        Player.PlayerTeam = int.Parse(TeamNum.text);
    }

    public void SwapToScene(String Scene)
    {
        SceneManager.LoadScene(Scene);
    }

    public void LoadCertainScene()
    {
        SceneManager.LoadScene(int.Parse(SceneNum.text));
    }

    public void ToggleMoving()
    {
        if (!Moving)
        {
            Moving = true;
            RaycastHit hit;
            if (Physics.Raycast(Player.transform.position, Player.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.tag == "Vehicle")
                {
                    VehiclePossess VehicleForwarder = hit.collider.gameObject.GetComponent<VehiclePossess>();
                    if (VehicleForwarder.VehicleType != 1) //ground vehicle
                    {
                        MoveObj = VehicleForwarder.Vehicle.transform;
                    }
                    else //aircraft
                    {
                        MoveObj = VehicleForwarder.Aircraft.transform;
                    }
                }
                else
                {
                    MoveObj = hit.collider.transform;
                }
            }
        }
        else
        {
            Moving = false;
        }
    }

    public void SetGameSpeed()
    {
        Time.timeScale = float.Parse(TimescaleNum.text);
    }

    public void GiveWeapon(Weapon WeaponToGive) //gives player desired weapon by replacing currently held weapon
    {
        if (Player.CurrentCharBrain != null)
        {
            Player.CurrentCharBrain.Weapons[Player.CurrentCharBrain.CurrentWeapon] = WeaponToGive;
            Player.CurrentCharBrain.UpdateWeapon();
        }
    }

    public void RefreshClass()
    {
        if (Player.CurrentCharBrain != null && Player.CurrentCharBrain.CurrentClass != null)
        {
            Player.CurrentCharBrain.ReadClassValues();
        }
    }

    public void RecalcTeams()
    {
        Sandboxref.RecalculateTeams();
    }
}
