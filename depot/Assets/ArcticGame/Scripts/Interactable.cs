using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent OnInteract;
    [HideInInspector] public CharacterBrain Interactor; //temp storage of sender character to be read from other scripts
    [SerializeField] private bool AllowWhileSeated;

    public void Interact(CharacterBrain Sender)
    {
        if (!Sender.Seated || AllowWhileSeated)
        {
            Interactor = Sender;
            OnInteract.Invoke();
            Interactor = null;
        }
    }
}
