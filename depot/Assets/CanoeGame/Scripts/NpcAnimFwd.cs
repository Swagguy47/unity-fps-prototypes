using OpenAI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NpcAnimFwd : MonoBehaviour
{
    [Serializable]
    public struct Memory
    {
        public string Message;
        public int Day;
    }

    [Serializable]
    public struct CharacterTraits
    {
        public string Name;
        public int Age;
        public string ClothingDesc;
        public string FishingSpot;
    }
    [HideInInspector] public CharacterTraits Traits;
    [SerializeField] NpcDialogue NPCDialogue;
    [Header("<color=red>IMPORTANT!!!</color>")]
    [SerializeField] string Identifier;
    public List<Memory> NpcMemory = new List<Memory>();
    public List<Memory> PlayerMemory = new List<Memory>();

    [Header("Appearance")]
    [SerializeField] GameObject Shirt;
    [SerializeField] GameObject Hoodie, Pants, Scarf, Hat, Hair, Beard, Mustach, Boots, ArmBand, CultMask;

    [HideInInspector] public bool Override;

    private void Start() //creates unique character traits.
    {

        if (PlayerPrefs.HasKey("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_Name") && PlayerPrefs.GetInt("ClearSave") != PlayerPrefs.GetInt("CurrentSave"))
        {
            Traits.Name = PlayerPrefs.GetString("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_Name");
            Traits.Age = PlayerPrefs.GetInt("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_Age");
            Traits.ClothingDesc = PlayerPrefs.GetString("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_Clothes");
            Traits.FishingSpot = PlayerPrefs.GetString("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_FishSpot");
        }
        else
        {
            string[] Names = { "William", "Oliver", "Daniel", "Ben", "Micheal", "Jack", "Liam", "Alex", "David", "John", "Joseph", "Jacob", "Theodore", "Anthony", "Ethan", "Hunter", "Tobias", "Kyler", "Aden", "Tyler", "Blake", "Adam", "Conner", "Connor", "Evan", "Marcel", "Noah", "Liam", "Elijah", "Lucas" };
            Traits.Age = UnityEngine.Random.Range(18, 40); //age
            Traits.Name = Names[UnityEngine.Random.Range(0, Names.Length)]; //name
            Traits.ClothingDesc = ""; //Clothing / appearance
            if (Shirt.activeSelf) { Traits.ClothingDesc += "- a grey wool shirt"; }
            if (Hoodie.activeSelf) { Traits.ClothingDesc += "- a red flanel jacket"; }
            if (Pants.activeSelf) { Traits.ClothingDesc += "- a blue pair of jeans"; }
            if (Scarf.activeSelf) { Traits.ClothingDesc += "- a brown scarf around your neck"; }
            if (ArmBand.activeSelf) { Traits.ClothingDesc += "- a light blue arm band on your left arm"; }
            if (Hat.activeSelf) { Traits.ClothingDesc += "- a grey hat"; }
            if (Boots.activeSelf) { Traits.ClothingDesc += "- a pair of brown boots"; }
            if (CultMask.activeSelf) { Traits.ClothingDesc += "- your cult mask, which is shaped to look like a hawk"; }
            if (Mustach.activeSelf) { Traits.ClothingDesc += "- you have a mustach"; }
            if (Beard.activeSelf) { Traits.ClothingDesc += "- you have a beard"; }
            if (!Hair.activeSelf) { Traits.ClothingDesc += "- and you are bald"; }
            //fav fishing spot
            string[] FishingSpots = { "the outskirts of Sector B", "the robot wall in Sector C", "the edge of the town before the waves get too rough", "the resturaunt in Sector C, since dropped food attracts the fish", "the market in Sector D", "some old destroyed houses in Sector C", "the edge of Sector E" };
            Traits.FishingSpot = "You know of a really good fishing spot near ";
            Traits.FishingSpot += FishingSpots[UnityEngine.Random.Range(0, FishingSpots.Length)];

            //Saves information to current save file
            PlayerPrefs.SetString("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_Name", Traits.Name);
            PlayerPrefs.SetInt("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_Age", Traits.Age);
            PlayerPrefs.SetString("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_Clothes", Traits.ClothingDesc);
            PlayerPrefs.SetString("Save" + PlayerPrefs.GetInt("CurrentSave") + "Npc" + Identifier + "_FishSpot", Traits.FishingSpot);
        }
    }

    public void StopTalking()
    {
        PlayerCallback.Dialogue.LookTo = new Vector3(0,0,0);
    }

    public void StartTalking()
    {
        PlayerCallback.Dialogue.LookTo = PlayerCallback.PlayerBrain.transform.position;
    }

    public void PointBaitShop()
    {
        PlayerCallback.Dialogue.LookTo = PlayerCallback.Landmarks.BaitShop.position;
    }

    public void PointResturaunt()
    {
        PlayerCallback.Dialogue.LookTo = PlayerCallback.Landmarks.Resturaunt.position;
    }

    public void AskGPT()
    {
        PlayerCallback.Dialogue.DialogueGO.SetActive(false);
        PlayerCallback.Dialogue.GPTGO.SetActive(true);
        ChatGPT Gpt = PlayerCallback.Dialogue.GPTGO.GetComponent<ChatGPT>();
        Gpt.Remember(this);
        Gpt.NewConversation = true;
        Debug.Log(PlayerMemory.Count + " locally stored memories");
    }

    //literally just for checking if it's playing override anims for very specific scripted scenarios
    public void StartOverride()
    {
        Override = true;
    }
    public void EndOverride()
    {
        Override = false;
    }
}
