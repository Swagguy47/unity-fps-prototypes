using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ProtoHackingUI : MonoBehaviour
{
    [SerializeField] RawImage PanelIco, DeviceIco;
    [HideInInspector] UnityEvent HackCallback;
    [SerializeField] GameObject FeedbackSlider, Root;
    [SerializeField] UnityEngine.UI.Button HackButton;
    [SerializeField] AudioSource Audio;

    [HideInInspector] public float DeviceWavelength, DeviceSpeed, DeviceAccuracy;
    int PanelWavelength, PanelSpeed;

    public void StartHacking(ProtoHackPanel Panel)
    {
        PanelWavelength = Panel.Wavelength;
        PanelSpeed = Panel.Speed;
        HackCallback = Panel.HackState;
        Root.SetActive(true);
        PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = true;
        DeviceAccuracy = 0;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    public void StopHacking()
    {
        Root.SetActive(false);
        PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public void FinishHacking()
    {
        HackCallback.Invoke();
        StopHacking();
    }

    private void Update()
    {
        //Debug.Log("Panel WL: " + PanelWavelength + " Panel SP: " + PanelSpeed + " Device WL: " + DeviceWavelength + " Device SP: " + DeviceSpeed);
        if (Root.activeSelf)
        {
            PanelIco.uvRect = new Rect(PanelIco.uvRect.x + (Time.deltaTime * PanelSpeed), 1, PanelWavelength, 1);
            DeviceIco.uvRect = new Rect(DeviceIco.uvRect.x + (Time.deltaTime * DeviceSpeed), 1, DeviceWavelength, 1);

            FeedbackSlider.transform.localScale = new Vector3(DeviceAccuracy, 1, 1);

            HackButton.gameObject.SetActive(DeviceAccuracy >= 1);

            Audio.pitch = (DeviceSpeed / 4);

            if (Mathf.RoundToInt(DeviceWavelength) == PanelWavelength && Mathf.RoundToInt(DeviceSpeed) == PanelSpeed) {
                if (DeviceAccuracy < 1) {
                    DeviceAccuracy += (Time.deltaTime / 3);
                }
            } 
            else {
                if (DeviceAccuracy > 0) {
                    DeviceAccuracy -= (Time.deltaTime / 3);
                }
            }
        }
    }

    public void SetSpeed(float speed)
    {
        DeviceSpeed = -speed;
    }

    public void SetWavelength(float wavelength)
    {
        DeviceWavelength = wavelength;
    }
}
