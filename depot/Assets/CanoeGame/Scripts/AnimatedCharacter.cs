using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedCharacter : MonoBehaviour
{
    public Transform ViewRoot;
    public bool HidePlayer, TakeCamera, HideOnEnd, EndPlayerAnimated;
    public Animator CharacterAnims;

    [HideInInspector] public bool Animating;
    [SerializeField] int IntroSeqUpdate = -1;

    public void PlayScene()
    {
        Animating = true;
        if (HidePlayer)
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.gameObject.SetActive(false);
        }
        if (EndPlayerAnimated)
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = true;
        }
        if (TakeCamera)
        {
            PlayerCallback.PlayerBrain.DevCamLocked = true;
            PlayerCallback.DebugMenu.DetachPlayer();
            PlayerCallback.PlayerBrain.transform.parent = ViewRoot;
            PlayerCallback.PlayerBrain.transform.SetPositionAndRotation(ViewRoot.position, ViewRoot.rotation);
        }
        CharacterAnims.SetTrigger("Play");
    }

    public void SceneFinished()
    {
        Animating = false;
        if (TakeCamera)
        {
            PlayerCallback.PlayerBrain.DevCamLocked = false;
            PlayerCallback.PlayerBrain.ForceRepossess(PlayerCallback.PlayerBrain.PlayerCharacter);
        }
        if (HidePlayer)
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.gameObject.SetActive(true);
        }
        if (EndPlayerAnimated)
        {
            PlayerCallback.PlayerBrain.CurrentCharBrain.Animated = false;
        }
        if (HideOnEnd)
        {
            //Destroy(this.gameObject);
            this.gameObject.SetActive(false);
        }
        if (IntroSeqUpdate != -1)
        {
            PlayerCallback.PlayerBrain.IntroSequencer.Sequence = IntroSeqUpdate;
            PlayerCallback.PlayerBrain.IntroSequencer.CheckSequence();
        }
    }
}
