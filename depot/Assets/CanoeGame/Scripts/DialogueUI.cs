using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [HideInInspector] public DialogueTree CurrentDialogue;
    [HideInInspector] public Animator NpcAnims;
    [HideInInspector] public Vector3 LookTo = new Vector3(0,0,0);
    [SerializeField] TextMeshProUGUI CharacterSpeech, Response1, Response2, Response3, Response4;
    public AnimatorOverrideController FallbackAnimator;
    public GameObject DialogueGO, GPTGO, RevealGO1, RevealGO2, RevealGO3, RevealGO4;
    float revealTime = 0;

    private void Update()
    {
        if (CurrentDialogue != null && DialogueGO.activeSelf)
        {
            if ((int)revealTime < CharacterSpeech.text.Length) // reveals dialogue text
            {
                revealTime += Time.deltaTime * 20 * CurrentDialogue.SpeechTimeMult;
                CharacterSpeech.maxVisibleCharacters = (int)revealTime;
                CharacterSpeech.ForceMeshUpdate();
                NpcAnims.SetBool("Talking", true);
            }
            else //reveals response options
            {
                NpcAnims.SetBool("Talking", false);
                if (CurrentDialogue.Response1 != "") //1
                {
                    RevealGO1.SetActive(true);
                }
                if (CurrentDialogue.Response2 != "") //2
                {
                    RevealGO2.SetActive(true);
                }
                if (CurrentDialogue.Response3 != "") //3
                {
                    RevealGO3.SetActive(true);
                }
                if (CurrentDialogue.Response4 != "") //4
                {
                    RevealGO4.SetActive(true);
                }
            }
        }

        if (NpcAnims != null)
        {
            if (LookTo != new Vector3(0,0,0))
            {
                Quaternion LookRotation = Quaternion.LookRotation(LookTo - NpcAnims.transform.position);
                //NpcAnims.transform.rotation = Quaternion.Slerp(NpcAnims.transform.rotation, LookRotation, Time.deltaTime * 0.2f); //looks at player

                NpcAnims.transform.rotation = LookRotation;
                NpcAnims.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y - 180, 0); //clamps rotation to just be horizontal
                //Debug.Log("Looking towards: " + LookTo + " and player is: " + PlayerCallback.PlayerBrain.transform.position);
            }
            else
            {
                //NpcAnims.transform.rotation = Quaternion.Slerp(NpcAnims.transform.rotation, NpcAnims.transform.parent.rotation, Time.deltaTime * 0.2f); //reorients with parent
                NpcAnims.transform.rotation = NpcAnims.transform.parent.rotation;
            }
        }
    }

    public void UpdateDialogue() //resets dialogue and values to new parameters
    {
        revealTime = 0;
        RevealGO1.SetActive(false);
        RevealGO2.SetActive(false);
        RevealGO3.SetActive(false);
        RevealGO4.SetActive(false);
        CharacterSpeech.text = CurrentDialogue.CharacterSpeech;
        Response1.text = CurrentDialogue.Response1;
        Response2.text = CurrentDialogue.Response2;
        Response3.text = CurrentDialogue.Response3;
        Response4.text = CurrentDialogue.Response4;
    }
    //potential responses
    public void Respond1()
    {
        if (CurrentDialogue.Tree1 != null) {

            Animator NpcAnimator = NpcAnims.GetComponent<Animator>();
            if (CurrentDialogue.AnimOverride1 == null) {
                NpcAnimator.runtimeAnimatorController = FallbackAnimator;
            }
            else {
                NpcAnimator.runtimeAnimatorController = CurrentDialogue.AnimOverride1;
            }

            CurrentDialogue = CurrentDialogue.Tree1;
            UpdateDialogue();
        }
        else {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
            DialogueGO.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            NpcAnims.SetTrigger("Stop");
        }
    }
    public void Respond2()
    {
        if (CurrentDialogue.Tree2 != null)
        {

            Animator NpcAnimator = NpcAnims.GetComponent<Animator>();
            if (CurrentDialogue.AnimOverride2 == null)
            {
                NpcAnimator.runtimeAnimatorController = FallbackAnimator;
            }
            else
            {
                NpcAnimator.runtimeAnimatorController = CurrentDialogue.AnimOverride2;
            }

            CurrentDialogue = CurrentDialogue.Tree2;
            UpdateDialogue();
        }
        else
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
            DialogueGO.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            NpcAnims.SetTrigger("Stop");
        }
    }
    public void Respond3()
    {
        if (CurrentDialogue.Tree3 != null)
        {

            Animator NpcAnimator = NpcAnims.GetComponent<Animator>();
            if (CurrentDialogue.AnimOverride3 == null)
            {
                NpcAnimator.runtimeAnimatorController = FallbackAnimator;
            }
            else
            {
                NpcAnimator.runtimeAnimatorController = CurrentDialogue.AnimOverride3;
            }

            CurrentDialogue = CurrentDialogue.Tree3;
            UpdateDialogue();
        }
        else
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
            DialogueGO.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            NpcAnims.SetTrigger("Stop");
        }
    }
    public void Respond4()
    {
        if (CurrentDialogue.Tree4 != null)
        {

            Animator NpcAnimator = NpcAnims.GetComponent<Animator>();
            if (CurrentDialogue.AnimOverride4 == null)
            {
                NpcAnimator.runtimeAnimatorController = FallbackAnimator;
            }
            else
            {
                NpcAnimator.runtimeAnimatorController = CurrentDialogue.AnimOverride4;
            }

            CurrentDialogue = CurrentDialogue.Tree4;
            UpdateDialogue();
        }
        else
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
            DialogueGO.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            NpcAnims.SetTrigger("Stop");
        }
    }
}
