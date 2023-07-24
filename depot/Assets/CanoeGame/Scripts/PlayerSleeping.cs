using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSleeping : MonoBehaviour
{
    [SerializeField] LoadingManager UIBlindFold;
    [SerializeField] AnimatedCharacter SleepChar, WakeChar;
    bool Sleeping;
    float TimeStart;

    bool AwaitSleepAnim;
    private void Update()
    {
        if (AwaitSleepAnim && !SleepChar.Animating) //start sleeping
        {
            AwaitSleepAnim= false;
            TimeStart = PlayerCallback.Weather.TimeRaw;
            Sleeping = true;
            PlayerCallback.PlayerBrain.IsInInventory = false;
            PlayerCallback.PlayerBrain.OpenInventory();
            UIBlindFold.gameObject.SetActive(true);
            UIBlindFold.SetLoadDescriptor("Resting...");
        }
        if (Sleeping)
        {
            Time.timeScale = 45;
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = true;

            PlayerCallback.PlayerBrain.Fading = true; //fade out
            PlayerCallback.PlayerBrain.FadeInOut.enabled = false;

            if (PlayerCallback.Weather.TimeRaw >= 85 && PlayerCallback.Weather.TimeRaw < TimeStart) //wake up
            {
                PlayerCallback.PlayerBrain.Fading = false; //fade in
                PlayerCallback.PlayerBrain.FadeInOut.enabled = true;

                Sleeping = false;
                Time.timeScale = 1;
                PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
                UIBlindFold.gameObject.SetActive(false);
                WakeChar.gameObject.SetActive(true);
                WakeChar.PlayScene();
            }

            float Progress;
            //Progress = (PlayerCallback.Weather.TimeRaw - TimeStart) / (805 - TimeStart); //805
            if (PlayerCallback.Weather.TimeRaw > TimeStart) {
                Progress = (PlayerCallback.Weather.TimeRaw - TimeStart) / (805 - TimeStart);
            }
            else {
                Progress = ((PlayerCallback.Weather.TimeRaw + 720) - TimeStart) / (805 - TimeStart);
            }
            //Debug.Log(Progress + " " + PlayerCallback.Weather.TimeRaw);
            UIBlindFold.SetProgress(Progress);
        }
    }

    public void NappyTime()
    {
        if (PlayerCallback.Weather.TimeRaw > 150) //plays lay down animation
        {
            SleepChar.gameObject.SetActive(true);
            SleepChar.PlayScene();
            AwaitSleepAnim = true;
        }
    }

    public void FactCheck()
    {
        if (PlayerCallback.Weather.TimeRaw <= 150)
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
            PlayerCallback.PlayerBrain.CurrentCharBrain.Interacting = false;
            PlayerCallback.PlayerBrain.CurrentCharBrain.AwaitingResponse = false;
            PlayerCallback.PlayerBrain.CurrentCharBrain.EndOverride();
        }
    }
}
