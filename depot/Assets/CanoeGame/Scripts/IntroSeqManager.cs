using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroSeqManager : MonoBehaviour
{
    public int Sequence = 0;
    float freezeTime = 0;

    //General
    [Header("Sequence components")]
    [SerializeField] NpcTest DadNpc;
    [SerializeField] AudioSource DadVoice;
    [SerializeField] Animator FrontDoorAnim;
    [SerializeField] Animator WorkshopDoorAnim;
    //S0 (prep)
    [Header("Sequence 0")]
    [SerializeField] AnimatorOverrideController S0_DadIdle_Anim;
    //S1
    [Header("Sequence 1")]
    [SerializeField] AudioClip S1_DadSit_VO;
    [SerializeField] AnimatorOverrideController S1_DadSit_Anim;
    //S2
    [Header("Sequence 2")]
    [SerializeField] AudioClip S2_TableTalk_VO;
    [SerializeField] AnimatorOverrideController S2_TableTalk_Anim;
    [SerializeField] AnimatedCharacter S2_TableTalk_PlayerAnim;
    [SerializeField] Transform S2_DadSitPos;
    //S3
    [Header("Sequence 3")]
    [SerializeField] Transform S3_DadDock_Pos;
    //S3.5
    [Header("Sequence 4")]
    [SerializeField] AudioClip S3_5_DadDock_VO;
    [SerializeField] AnimatorOverrideController S3_5_DadDock_Anim;
    //S4 / 5
    [Header("Sequence 5 & 6")]
    [SerializeField] AudioClip S4_DadCoins_VO;
    [SerializeField] AnimatorOverrideController S4_DadCoins_Anim;
    //S6
    [Header("Sequence 7")]
    [SerializeField] AudioClip S6_DadSendoff_VO;
    [SerializeField] AnimatorOverrideController S6_DadSendoff_Anim;
    float EnterHomeDelay = 0;
    bool SendoffOver;
    //S7
    [Header("Sequence 8")]
    //[SerializeField] AudioClip S7_GetNails_VO;
    [SerializeField] GameObject S7_TriggerVolume;
    [SerializeField] AudioClip S7_DadProud_VO;
    [SerializeField] AnimatorOverrideController S7_DadProud_Anim;
    [SerializeField] AudioClip S7_DadDisappointed_VO;
    [SerializeField] AnimatorOverrideController S7_DadDisappointed_Anim;


    private void LateUpdate()
    {
        if (freezeTime != 0)
        {
            PlayerCallback.Weather.TimeRaw = freezeTime;
        }
        if (EnterHomeDelay > 0) //S6
        {
            EnterHomeDelay -= Time.deltaTime;
            CheckSequence();
        }
    }

    public void SetSequence(int NewSeq)
    {
        Sequence = NewSeq;
    }

    public void CheckSequence()
    {
        if (Sequence == 0)
        {
            freezeTime = 80; //Early Morning
            DadNpc.OverrideAnims(S0_DadIdle_Anim);
        }
        if (Sequence == 1) //greet player, invite to table
        {
            DadVoice.PlayOneShot(S1_DadSit_VO);
            DadNpc.OverrideAnims(S1_DadSit_Anim);
            DadNpc.AllowExit(false);
        }
        else if (Sequence == 2) //sit down player, dad begins talking
        {
            S2_TableTalk_PlayerAnim.gameObject.SetActive(true);
            S2_TableTalk_PlayerAnim.PlayScene();
            DadNpc.OverrideAnims(S2_TableTalk_Anim);
            DadNpc.AllowExit(true);
            DadVoice.PlayOneShot(S2_TableTalk_VO);
            //DadNpc.transform.SetPositionAndRotation(S2_DadSitPos.position, S2_DadSitPos.rotation);
        }
        else if (Sequence == 3) //open door, dad walks to dock and awaits player
        {
            DadNpc.LookAt = null;
            DadNpc.OverrideAnims(null);
            DadNpc.MoveTo(S3_DadDock_Pos.position);
            FrontDoorAnim.SetBool("Open", true);
        }
        else if (Sequence == 4) //hold out hand full of coins
        {
            DadNpc.LookAt = PlayerCallback.PlayerBrain.transform;
            DadVoice.PlayOneShot(S3_5_DadDock_VO);
            DadNpc.OverrideAnims(S3_5_DadDock_Anim);
            //DadNpc.AllowExit(false);
        }
        else if (Sequence == 5) //hand player coins, point to shop
        {
            DadNpc.OverrideAnims(S4_DadCoins_Anim);
            //DadNpc.AllowExit(true);
            DadVoice.PlayOneShot(S4_DadCoins_VO);
            DadNpc.LookAt = PlayerCallback.PlayerBrain.transform;
        }
        else if (Sequence == 6) //(triggered by prev sequence anim), dad points to market.
        {
            DadNpc.LookAt = PlayerCallback.Landmarks.Market;
        }
        else if (Sequence == 7) //dad yells to be back before sunset & returns indoors
        {
            if (!SendoffOver)
            {
                Debug.Log("<color=green>-E N D   O F   S C R I P T E D   S E Q U E N C E S-</color>");
                freezeTime = 0;
                DadNpc.LookAt = PlayerCallback.PlayerBrain.transform;
                DadVoice.PlayOneShot(S6_DadSendoff_VO);
                DadNpc.OverrideAnims(S6_DadSendoff_Anim);
                SendoffOver = true;
                EnterHomeDelay = 5;
                PlayerPrefs.SetInt("FinishedIntro" + PlayerPrefs.GetInt("CurrentSave"), 1);
                //!PlayerPrefs.HasKey("FinishedIntro" + PlayerPrefs.GetInt("CurrentSave"))
            }
            else if (EnterHomeDelay <= 0) //returns inside after 5 second delay
            {
                DadNpc.OverrideAnims(null);
                DadNpc.LookAt = null;
                DadNpc.MoveTo(DadNpc.HomeIdleSpot.position);
            }
        }
        else if (Sequence == 8) //Player returns home after trip
        {
            DadNpc.LookAt = PlayerCallback.PlayerBrain.transform;
            if (PlayerCallback.Weather.TimeRaw * (PlayerCallback.Weather.Day + 1) < 470) //player returns that day
            {
                if (PlayerCallback.Inventory.CountItem(StaticItemPool.Items.ItemPool[6].BaseClass) > 0) //has nails
                {
                    S7_TriggerVolume.SetActive(false);
                    DadVoice.PlayOneShot(S7_DadProud_VO);
                    DadNpc.OverrideAnims(S7_DadProud_Anim);
                }
                /*else //has no nails
                {
                    DadNpc.LookAt = null;
                    DadVoice.PlayOneShot(S7_GetNails_VO);
                }*/
            }
            else //player returns at night or different day
            {
                S7_TriggerVolume.SetActive(false);
                DadVoice.PlayOneShot(S7_DadDisappointed_VO);
                DadNpc.OverrideAnims(S7_DadDisappointed_Anim);
            }
        }
    }
}
