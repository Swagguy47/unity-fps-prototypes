using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContextualInteract : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI InteractText;

    public void UpdateInteractText(string NewText)
    {
        InteractText.text = NewText;
    }
}
