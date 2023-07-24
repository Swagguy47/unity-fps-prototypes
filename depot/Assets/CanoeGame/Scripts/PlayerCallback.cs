using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCallback : MonoBehaviour
{
    [HideInInspector] public static PlayerBrain PlayerBrain;
    [HideInInspector] public static WeatherSystem Weather;
    [HideInInspector] public static AudioMixerControl AudioMix;
    [HideInInspector] public static LoadingManager LoadManager;
    [HideInInspector] public static PlayerInventory Inventory;
    [HideInInspector] public static ContainerDescriptor Container;
    [HideInInspector] public static DialogueUI Dialogue;
    [HideInInspector] public static Landmarks Landmarks;
    [HideInInspector] public static DevMenu DebugMenu;
    [HideInInspector] public static StaticItemPool ItemPool;

    private void Start()
    {
        PlayerBrain = GetComponent<PlayerBrain>();
        Weather = GameObject.Find("-Weather-").GetComponent<WeatherSystem>();
        AudioMix = GetComponent<AudioMixerControl>();
        LoadManager = GetComponent<LoadingFwd>().Manager;
        Inventory = GetComponent<PlayerInventory>();
        Container = GetComponent<ContainerDescriptor>();
        Dialogue = GetComponent<DialogueUI>();
        Landmarks= GetComponent<Landmarks>();
        DebugMenu = GameObject.Find("DebugMenu").GetComponent<DevMenu>();
        ItemPool = GetComponent<StaticItemPool>();
    }
}
