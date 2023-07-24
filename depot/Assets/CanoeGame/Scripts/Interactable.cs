using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent OnInteract, OverrideEnd;
    [HideInInspector] public CharacterBrain Interactor; //temp storage of sender character to be read from other scripts
    [SerializeField] private bool AllowWhileSeated, AllowWhileAnimating = true, AnimatedInteraction;
    [SerializeField] AnimatorOverrideController Override;
    [SerializeField] Transform MoveTo;

    public void Interact(CharacterBrain Sender)
    {
        if (!Sender.Seated || AllowWhileSeated)
        {
            if (!Sender.Animated || AllowWhileAnimating)
            {
                Interactor = Sender;
                OnInteract.Invoke();

                if (AnimatedInteraction) //InteractionAnimation
                {
                    Sender.Override = Override;
                    Sender.InteractionPos = MoveTo;
                    Sender.Interaction = this;
                    Sender.AnimatedInteract();
                }
                Interactor = null;
            }
        }
    }

    public void OverrideOver(CharacterBrain Sender)
    {
        Interactor = Sender;
        OverrideEnd.Invoke();
        Interactor = null;
    }
}
