using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TextMeshProUGUI NpcResponse;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        [SerializeField] private GameObject PlayerInput, PlayerOptions;

        [HideInInspector] public bool NewConversation;

        private float height, revealTime = 0;
        private OpenAIApi openai;
        //private OpenAIApi openai = new OpenAIApi("fdhgjsdghifwshdoifwehAx2"); //broke key testing

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Act as a stranger and reply to the questions. Don't break character. Don't ever add dialogue tags your response (like 'Stranger: ') or finish sentences for the user, even if they're incomplete. Only respond as your character. Don't ever mention that you are an AI model.";

        string PrevResponse = "";

        int MemoryRetentionLength = 3; //they remember things up to and including days this old, past that they forget.

        //memory
        NpcAnimFwd LastNpc;
        [HideInInspector] public List<NpcAnimFwd.Memory> NpcMemory = new List<NpcAnimFwd.Memory>();
        [HideInInspector] public List<NpcAnimFwd.Memory> PlayerMemory = new List<NpcAnimFwd.Memory>();

        private void Start()
        {
            openai = new OpenAIApi(PlayerPrefs.GetString("AiKey"));
            button.onClick.AddListener(SendReply);
        }

        private void OnEnable()
        {
            PrevResponse = "";
            revealTime = 0;
            NpcResponse.text = "What is it?";
            inputField.text = "";
            PlayerCallback.Dialogue.NpcAnims.runtimeAnimatorController = PlayerCallback.Dialogue.FallbackAnimator;
        }

        public void EndConversation()
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
            PlayerCallback.Dialogue.DialogueGO.SetActive(false);
            PlayerCallback.Dialogue.GPTGO.SetActive(false);
            PlayerInput.SetActive(true);
            PlayerOptions.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PlayerCallback.Dialogue.NpcAnims.SetTrigger("Stop");
            PlayerCallback.Dialogue.NpcAnims.SetBool("Talking", false);

            //Assign local memories to npc
            //LastNpc.NpcMemory.Clear();
            //LastNpc.PlayerMemory.Clear();
            //LastNpc.NpcMemory = NpcMemory;
            //LastNpc.PlayerMemory = PlayerMemory;
            //Debug.Log("Npc stored: " + LastNpc.PlayerMemory.Count + " memories");

            //PlayerMemory.Clear();
            //NpcMemory.Clear();

            LastNpc = null;
        }

        public void Remember(NpcAnimFwd NpcRef) //brings back old times :,)
        {
            /*//NpcMemory.Clear();
            foreach(NpcAnimFwd.Memory CurrentMemory in NpcRef.NpcMemory) //forgets memories from over 3 days ago
            {
                if (MemoryAge(CurrentMemory) < 3)
                {
                    NpcMemory.Add(CurrentMemory);
                }
            }
            //PlayerMemory.Clear();
            foreach (NpcAnimFwd.Memory CurrentMemory in NpcRef.PlayerMemory) //forgets memories from over 3 days ago
            {
                if (MemoryAge(CurrentMemory) < 3)
                {
                    PlayerMemory.Add(CurrentMemory);
                }
                else
                {
                    Debug.Log("Memory: " + CurrentMemory.Message + " forgotten");
                }
            }*/
            LastNpc = NpcRef;

            //Debug.Log("I remember: " + PlayerMemory.Count + " things from our previous encounters.");

            //forgets old memories
            /*int removeNum = 0;
            for (int i = 0; i <= NpcRef.PlayerMemory.Count; i++)
            {
                if (MemoryAge(PlayerMemory[i - removeNum]) >= MemoryRetentionLength)
                {
                    NpcRef.PlayerMemory.RemoveAt(i - removeNum);
                    NpcRef.NpcMemory.RemoveAt(i - removeNum);
                    removeNum++;
                }
            }*/
        }

        public int MemoryAge(NpcAnimFwd.Memory MeasuredMemory)
        {
            return Mathf.Abs(MeasuredMemory.Day - PlayerCallback.Weather.Day);
        }

        private void Update()
        {
            //Debug.Log(NewConversation);
            if ((int)revealTime < NpcResponse.text.Length) // reveals dialogue text
            {
                revealTime += Time.deltaTime * 25;
                NpcResponse.maxVisibleCharacters = (int)revealTime;
                NpcResponse.ForceMeshUpdate();
                PlayerCallback.Dialogue.NpcAnims.SetBool("Talking", true);
            }
            else //reveals response options
            {
                PlayerCallback.Dialogue.NpcAnims.SetBool("Talking", false);
                if (!PlayerInput.activeSelf && NpcResponse.text != "") {
                    PlayerOptions.SetActive(true);
                }
            }
        }

        private void AppendMessage(ChatMessage message)
        {
            //NpcResponse.text = message;
            if (message.Role != "user")
            {
                NpcResponse.text = message.Content;
                PrevResponse = message.Content;
            }

            /*scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;*/
        }

        private async void SendReply()
        {
            NpcResponse.text = "";
            PlayerInput.SetActive(false);
            PlayerCallback.Dialogue.NpcAnims.SetTrigger("StartThinking"); //plays hands on hip animation while waiting

            //logs new memory of player's words
            NpcAnimFwd.Memory MemoryPlayer = new NpcAnimFwd.Memory();
            MemoryPlayer.Message = inputField.text;
            MemoryPlayer.Day = PlayerCallback.Weather.Day;

            LastNpc.PlayerMemory.Add(MemoryPlayer);

            //environment context
            string TimeOfDay = "";
            if (PlayerCallback.Weather.TimeRaw > 470 && PlayerCallback.Weather.TimeRaw < 715)
            {
                TimeOfDay = "night, with the spotlights on";
            }
            else
            {
                TimeOfDay = "daytime";
            }
            string Weather = "";
            if (PlayerCallback.Weather.Storm)
            {
                Weather = "raining";
            }
            else
            {
                Weather = "clear skies";
            }

            string append = "";
            NewConversation = true; //dev
            if (NewConversation)
            {
                append = "Pretend you are a video game character who is in a town surrounded by water with a giant megacity surrounded by walls nearby.The town is where the humans live, the ground has been flooded entirely and there is no place to stand and no roads connecting locations, every human traverses the water between houses in establishments via their canoes or other small boats, some locations really close to each other are connected via small walkways, houses are all wooden or scrap metal and pretty small. All the humans are being oppressed by the robots who live in the city(humans used to live in the city alongside the robots, who were servants, but eventually they became fed up with our politics and the way we treated the planet and turned on us, kicking everyone out of the city and relocated into the slums of the human town outside the walls, they claimed they were going to relocate us to some other facility which would be more hospitable, though it was a lie and they've been slowly killing everyone off, there is absolutely zero way for humans to enter the city nowadays). The human town is seperated into multiple distinct sectors, Sector A is a shipyard which is dangerous, so you avoid it, Sector B is overrun by cultists who can be quite hostile (but is the only place to obtain robot hardware or melee weaponry if the player asks, not that you've ever been there to find out if these rumors are true(note that the only weapons around are spears or stolen flamethrowers, as guns and ammo production have been ceased by robots long ago when they kicked everyone out)), Sector C is a peaceful set of houses, farms and small establishments, Sector D has a lot more shops and the main market where you buy a majority of your food and items, Sector E is being locked down and destroyed with flamethrowers by robots due to a virus outbreak, you avoid that place too. on one side of the town is the giant megacity wall, impossible to approach without danger, and the other, open ocean with giant dangerous waves guaranteed to destroy any watercraft attempting to cross it, if it was possible you'd be free from the oppression of the robots, but nobody has ever survived the trip, humans are trapped living in their town in the only safe zone between the two. You are generally distressed, fearful, and cautious of everyone as humans have resorted to scavenging for resources and forming cults. You're just a generic, peaceful human though, living out your days in Sector C, where your house is.Somewhat infrequently visiting the market for items in Sector D(but never for food as it only offers basic supplies(like nails, rope, seeds, metal, lanters, life preservers, tarps, motors, floats, fish boxes), crops(only good for making into bread, but you don't do that), and scrap), or the carpenter if you need wood items also in Sector D (they sell thing such as fishing rods, canoes and other boats, containers, and other small wood items), the bait shop for fishing supplies in Sector C (but does not sell fish or fishing rods, purely bait), and rarely visit a restaurant near the border of Sector C & D (while you like their food, you generally think its far cheaper to catch and cook your own stuff). " + LastNpc.Traits.FishingSpot +", and always try your best to return home before sunset. After sunset the giant spotlights on the robot megacity's walls illuminate the town and scan for anyone still outside, which you avoid entirely(this only occurs in the night, in the day the spotlights are always off, even if its raining.Though it is completely safe to be outside during the day.). You primarily eat fish, but occasionally pair it with basic crops from the market for some variety. It is currently " + TimeOfDay + " and " + Weather +  ". Your character's name is: '" + LastNpc.Traits.Name + "', age: " + LastNpc.Traits.Age + ". You're appearance is: " + LastNpc.Traits.ClothingDesc + " (Note that all these clothing items and descriptions of your character's appearance are permanent, not flexible, and limited to just items listed. Even if the player insists you have another appearance trait, you do not. This also means you cannot remove any of these items / traits under any circumstances). With that all in mind, assume the role as your character, respond only with what the character would say and nothing else, you do not know you are in a video game and certainly do not know anything about OpenAI, ChatGPT, or related subjects. Any time in your response that you mention a sector, add '<color=orange>' before mentioning it (without quotes) and '</color>' at the end, looking like: '<color=orange>Sector C</color>' for example. You've just been standing around doing nothing of much importance when the player approaches you and begins talking, ";
                if (LastNpc.PlayerMemory.Count - 1 > 0) //remember past conversations
                {
                    Debug.Log("They recall: " + LastNpc.PlayerMemory.Count + " memories with you");
                    append = append + "these are all your previous interactions with the player: ("; //you remember the player, and in an instant you recall all of your previous interactions together: (
                    int MemCount = 0;
                    foreach (NpcAnimFwd.Memory CurrentMem in LastNpc.PlayerMemory)
                    {
                        if (MemCount != LastNpc.PlayerMemory.Count - 1)
                        {
                            if (MemoryAge(CurrentMem) <= MemoryRetentionLength)
                            {
                                Debug.Log("processed memory #" + MemCount + " (" + CurrentMem.Message + ")" + " (" + LastNpc.NpcMemory[MemCount].Message + ")");
                                append = append + (MemoryAge(CurrentMem) == 0 ? "today they told you: " : MemoryAge(CurrentMem) + " days ago they told you: '") + CurrentMem.Message + "' and you responded with: '" + LastNpc.NpcMemory[MemCount].Message + "' - ";
                                MemCount++;
                            }
                        }
                        else
                        {
                            Debug.Log("Skipping memory: #" + MemCount + " (" + CurrentMem.Message + ") Too recent to have response");
                        }
                    }
                    append = append + ") Keep this all in mind as the player may mention something from the past in their statement to you. If they ask something ambiguous remember to think back to these memories of previous interactions for help. ";
                }
                if (PrevResponse != "") //remember current conversation
                {
                    append = append + "a moment ago you told them: '" + PrevResponse + "', and now they're telling you: '";
                }
                else
                {
                    append = append + "a couple seconds into the conversation they tell you something: '";
                }
            }
            else
            {
                append = "(remember not to respond with anything other than what the character would say) The player responds: '";
            }

            messages.Clear();
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = append + inputField.text + ".'"
            };
            Debug.Log(append);
            if (messages.Count == 0) newMessage.Content = prompt + "\n" + append + inputField.text; 
            
            messages.Add(newMessage);
            
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0301",
                //Model = "text-davinci-003",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                AppendMessage(message);

                //logs new memory of npcs response
                NpcAnimFwd.Memory MemoryNpc = new NpcAnimFwd.Memory();
                MemoryNpc.Message = message.Content;
                MemoryNpc.Day = PlayerCallback.Weather.Day;

                LastNpc.NpcMemory.Add(MemoryNpc);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
                NpcResponse.text = "<allcaps><color=red>[Failed to generate a response. Try again or </color>check that your API key is correct and within quota<color=red>]</color></allcaps>";
            }

            button.enabled = true;
            inputField.enabled = true;

            revealTime = 0;
            NewConversation = false;
        }
    }
}
