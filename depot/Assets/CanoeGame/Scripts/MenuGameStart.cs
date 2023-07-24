using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuGameStart : MonoBehaviour
{
    [SerializeField] TMP_InputField KeyInput, QuotaInput;
    [SerializeField] TextMeshProUGUI SensitivityLabel;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("AiKey")) { PlayerPrefs.SetString("AiKey", ""); }
        if (!PlayerPrefs.HasKey("Sensitivity")) { PlayerPrefs.SetFloat("Sensitivity", 100); }
        if (!PlayerPrefs.HasKey("InvertLook")) { PlayerPrefs.SetInt("InvertLook", 1); }
        if (!PlayerPrefs.HasKey("QuotaCap")) { PlayerPrefs.SetFloat("QuotaCap", 0.50f); }
        if (!PlayerPrefs.HasKey("Quality")) { PlayerPrefs.SetInt("Quality", 4); } //Quality is: "very high" by default
        if (!PlayerPrefs.HasKey("QuotaUsage")) { PlayerPrefs.SetFloat("QuotaCap", 0); }
        if (!PlayerPrefs.HasKey("CurrentSave")) { PlayerPrefs.SetFloat("CurrentSave", 0); }

        PlayerPrefs.SetInt("ClearSave", -1);
        SetGraphicsQuality(PlayerPrefs.GetInt("Quality")); //just to refresh it
        QuotaInput.text = PlayerPrefs.GetFloat("QuotaCap").ToString();
        KeyInput.text = PlayerPrefs.GetString("AiKey");
        SensitivityLabel.text = PlayerPrefs.GetFloat("Sensitivity").ToString();
    }

    public void StartGame()
    {
        GameObject[] PersistentGOs = SceneManager.GetSceneByName("PersistentScene").GetRootGameObjects();
        foreach (GameObject GO in PersistentGOs)
        {
            if (GO.name == "EventSystem")
            {
                GO.GetComponent<PersistentScene>().LoadGame();
            }
        }
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void APIQuestion()
    {
        Application.OpenURL("https://platform.openai.com/account/api-keys");
    }

    public void KeyChange(TMP_InputField Input)
    {
        PlayerPrefs.SetString("AiKey", Input.text);
    }

    public void SetSensitivity(Slider ValueSlider)
    {
        PlayerPrefs.SetFloat("Sensitivity", ValueSlider.value);
        SensitivityLabel.text = PlayerPrefs.GetFloat("Sensitivity").ToString();
    }

    public void SetInvert(int Invert)
    {
        PlayerPrefs.SetInt("InvertLook", Invert);
    }

    public void SetGraphicsQuality(int NewQuality)
    {
        PlayerPrefs.SetInt("Quality", NewQuality);

        QualitySettings.SetQualityLevel(NewQuality);
    }

    public void SetCurrentSave(int NewSave)
    {
        PlayerPrefs.SetInt("CurrentSave", NewSave);
    }

    public void ClearSaveFile()
    {
        PlayerPrefs.SetInt("ClearSave", PlayerPrefs.GetInt("CurrentSave"));
        PlayerPrefs.DeleteKey("FinishedIntro" + PlayerPrefs.GetInt("CurrentSave"));
    }
}
