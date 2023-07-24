using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] Transform LightPos, BorderShadows;
    float FadeOut;
    [SerializeField] Image Fade, ProgressBar, BG;
    [SerializeField] Sprite[] BGImages;
    RectTransform UICanvas;
    [SerializeField] TextMeshProUGUI Label;

    private void Start()
    {
        UICanvas = transform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector2 pos;
        pos = Input.mousePosition;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(UICanvas.transform as RectTransform, Input.mousePosition, Camera.main, out pos);

        BorderShadows.transform.localPosition = new Vector3(BorderShadows.transform.localPosition.x, -LightPos.transform.localPosition.y, BorderShadows.transform.localPosition.z);
        if (FadeOut > 0)
        {
            FadeOut -= Time.unscaledDeltaTime / 5;
            Fade.color = new Color(0, 0, 0, FadeOut);
        }

        LightPos.transform.position = pos;
        BG.transform.localPosition = -pos / 13;
    }

    private void OnEnable()
    {
        FadeOut = 1;
        BG.sprite = BGImages[Random.Range(0, BGImages.Length)];
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ProgressBar.fillAmount = 0;
        PlayerCallback.AudioMix.SetVolume(9, 0.0001f);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerCallback.AudioMix.SetVolume(9, 1);
    }

    public void SetLoadDescriptor(string NewLabel)
    {
        Label.text = NewLabel;
    }

    public void SetProgress(float NewProgress)
    {
        ProgressBar.fillAmount = NewProgress;
    }
}
