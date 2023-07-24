using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDialogue : MonoBehaviour
{
    [SerializeField] Animator NPCAnimator;

    public void StartDialogue(DialogueTree Dialogue)
    {
        PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = true;
        PlayerCallback.Dialogue.CurrentDialogue = Dialogue;
        PlayerCallback.Dialogue.NpcAnims = NPCAnimator;
        PlayerCallback.Dialogue.DialogueGO.SetActive(true);
        PlayerCallback.Dialogue.UpdateDialogue();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerCallback.Dialogue.LookTo = PlayerCallback.PlayerBrain.transform.position;
    }
}
